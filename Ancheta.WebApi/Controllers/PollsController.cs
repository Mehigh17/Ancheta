using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ancheta.Model.Data;
using Ancheta.Model.Messages;
using Ancheta.Model.Repositories;
using Ancheta.Model.Services;
using Ancheta.Model.ViewModels;
using Ancheta.WebApi.Model.Input;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharpCatch.Asp.Filters;

namespace Ancheta.WebApi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class PollsController : ControllerBase
    {

        private const int _MaxPollChunk = 8;
        private readonly IPollRepository _pollRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly IMapper _mapper;
        private readonly IPollService _pollService;
        private readonly ITinyMessengerHub _messengerHub;

        public PollsController(IPollRepository pollRepository,
                               IVoteRepository voteRepository,
                               IMapper mapper,
                               IPollService pollService,
                               ITinyMessengerHub messengerHub)
        {
            _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _pollRepository = pollRepository ?? throw new ArgumentNullException(nameof(pollRepository));
            _voteRepository = voteRepository ?? throw new ArgumentNullException(nameof(voteRepository));
            _messengerHub = messengerHub ?? throw new ArgumentNullException(nameof(messengerHub));
        }

        /// <summary>
        /// Fetch a list of unexpired public polls.
        /// </summary>
        /// <param name="offset">The count of polls to ignore from the newest to oldest ones.</param>
        /// <param name="count">The amount of polls to fetch.</param>
        /// <response code="200">Returns a list of polls.</response>
        /// <response code="400">If the limits or the bounds are invalid.</response>
        /// <returns>A list of polls.</returns>
        [HttpGet]
        public async Task<ActionResult<IList<PollDetailViewModel>>> GetPublicPolls([FromQuery] int offset, [FromQuery] int count)
        {
            if (count > _MaxPollChunk)
            {
                ModelState.TryAddModelError("limits", $"You can request at most {_MaxPollChunk} polls in a request.");
                return ValidationProblem(ModelState);
            }

            var polls = await _pollRepository.GetPublicPolls(offset, count);
            var publicPolls = polls.Select(p => _mapper.Map<Poll, PollDetailViewModel>(p));
            return publicPolls.ToList();
        }

        /// <summary>
        /// Fetch a poll with given id.
        /// </summary>
        /// <param name="pollId">The id of the poll to be fetched.</param>
        /// <response code="200">Returns a poll.</response>
        /// <response code="400">If poll id has invalid format.</response>
        /// <response code="404">If poll has not been found.</response>
        /// <returns>A poll.</returns>
        [HttpGet("{pollId}")]
        public async Task<ActionResult<PollDetailViewModel>> GetPoll([FromRoute] string pollId)
        {
            if (Guid.TryParse(pollId, out var pid))
            {
                var poll = await _pollRepository.GetById(pid);
                if (poll is null) return NotFound();

                var pollVm = _mapper.Map<Poll, PollDetailViewModel>(poll);
                return pollVm;
            }
            return BadRequest();
        }

        /// <summary>
        /// Permanently remove a poll from the database.
        /// </summary>
        /// <param name="id">Id of the poll.</param>
        /// <param name="secretCode">Secret code required to manage the poll.</param>
        /// <response code="200">The poll has been removed.</response>
        /// <response code="401">If the secret code is not valid.</response>
        /// <response code="400">If the poll id has invalid format.</response>
        /// <response code="404">If the poll has not been found in the database.</response>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> RemovePoll([FromQuery] string id, [FromQuery] string secretCode)
        {
            if (Guid.TryParse(id, out var pollId))
            {
                var poll = await _pollRepository.GetById(pollId);
                if (poll == null) return NotFound();

                var isAuthorized = _pollService.IsPasswordValid(secretCode, poll.SecretCodeHash);
                if (!isAuthorized) return Unauthorized();

                var success = await _pollRepository.Delete(poll);
                if (success) return Ok();
            }

            return BadRequest();
        }

        /// <summary>
        /// Create a poll.
        /// </summary>
        /// <param name="model">The input data for the poll.</param>
        /// <response code="200">Returns the successfully created poll.</response>   
        /// <response code="400">If the model information is invalid.</response>
        /// <returns></returns>
        [HttpPost]
        [RecaptchaValidation]
        public async Task<ActionResult<PollCreatedViewModel>> CreatePoll([FromBody] PollCreationModel model)
        {
            if (ModelState.IsValid)
            {
                var secret = _pollService.GenerateSecretCode();
                var pollId = Guid.NewGuid();
                var poll = new Poll
                {
                    Id = pollId,
                    Question = model.Question,
                    CreatedOn = DateTime.Now,
                    Duration = model.Duration.HasValue ? TimeSpan.FromSeconds(model.Duration.Value) : default(TimeSpan?),
                    IsPublic = model.IsPublic,
                    SecretCodeHash = secret.Item2,
                    AllowMultipleVotesPerIp = model.AllowMultipleVotesPerIp,
                    Answers = model.Answers.Select(a => new Answer
                    {
                        Id = Guid.NewGuid(),
                        Content = a.Content,
                        Votes = new List<Vote>()
                    }).ToList()
                };

                await _pollRepository.Add(poll);

                var output = new PollCreatedViewModel
                {
                    PollId = pollId.ToString(),
                    SecretCode = secret.Item1
                };

                return Ok(output);
            }
            return BadRequest();
        }

        /// <summary>
        /// Cast a vote on a given answer.
        /// </summary>
        /// <param name="pollId">The id of the poll the vote should be casted in.</param>
        /// <param name="answerId">The id of the answer that the vote should be casted on.</param>
        /// <response code="200">The vote has been casted successfully.</response>   
        /// <response code="400">If the poll or answer id are in invalid formats.</response>   
        /// <response code="403">If the user has already voted before.</response>
        /// <response code="404">If the poll or answer are not found.</response>
        /// <returns></returns>
        [HttpPost("{pollId}/votes")]
        [RecaptchaValidation]
        public async Task<IActionResult> CastVote([FromRoute] string pollId, [FromQuery] string answerId)
        {
            if (Guid.TryParse(pollId, out var id))
            {
                var poll = await _pollRepository.GetById(id);
                if (poll == null) return NotFound();

                var addressBytes = HttpContext.Connection.RemoteIpAddress.MapToIPv4().GetAddressBytes();

                if (!poll.AllowMultipleVotesPerIp)
                {
                    var votes = poll.Answers.SelectMany(s => s.Votes);
                    var sourceIpBytes = addressBytes;
                    if (votes.FirstOrDefault(v => v.Source.SequenceEqual(sourceIpBytes)) != null)
                    {
                        return StatusCode(StatusCodes.Status403Forbidden);
                    }
                }

                if (Guid.TryParse(answerId, out var answId))
                {
                    var answer = poll.Answers.FirstOrDefault(a => a.Id.Equals(answId));
                    if (answer == null) return NotFound();

                    var vote = new Vote
                    {
                        Id = Guid.NewGuid(),
                        Source = addressBytes,
                        CastedOn = DateTime.Now,
                        OwnerAnswer = answer
                    };

                    await _voteRepository.Add(vote);

                    _messengerHub.Publish(new VoteCastedMessage(pollId, vote));

                    return Ok();
                }
            }

            return BadRequest();
        }

    }
}