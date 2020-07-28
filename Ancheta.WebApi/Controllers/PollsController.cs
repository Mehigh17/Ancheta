using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    [Route("api/[controller]")]
    public class PollsController : ControllerBase
    {

        private const int MaxPollChunk = 8;
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
            if (count > MaxPollChunk)
            {
                ModelState.TryAddModelError("limits", $"You can request at most {MaxPollChunk} polls in a request.");
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
            if (!Guid.TryParse(pollId, out var pid))
            {
                ModelState.TryAddModelError("ParsingError", $"Poll ID {pollId} is not valid.");
                return BadRequest(ModelState);
            }

            var poll = await _pollRepository.GetById(pid);
            if (poll == null)
            {
                return NotFound();
            }

            var pollVm = _mapper.Map<Poll, PollDetailViewModel>(poll);
            return pollVm;
        }

        /// <summary>
        /// Permanently remove a poll from the database.
        /// </summary>
        /// <param name="pollId">The id of the poll to be removed.</param>
        /// <param name="secretCode">Secret code required to manage the poll.</param>
        /// <response code="200">The poll has been removed.</response>
        /// <response code="401">If the secret code is not valid.</response>
        /// <response code="400">If the poll id has invalid format.</response>
        /// <response code="404">If the poll has not been found in the database.</response>
        /// <returns></returns>
        [HttpDelete("{pollId}")]
        public async Task<IActionResult> RemovePoll([FromRoute] string pollId, [FromQuery] string secretCode)
        {
            if (!Guid.TryParse(pollId, out var id))
            {
                ModelState.TryAddModelError("ParsingError", $"Poll ID {pollId} is not valid.");
                return BadRequest(ModelState);
            }

            var poll = await _pollRepository.GetById(id);
            if (poll == null)
            {
                return NotFound();
            }

            var isAuthorized = _pollService.IsCodeValid(secretCode, poll.SecretCodeHash);
            if (!isAuthorized)
            {
                return Unauthorized();
            }

            await _pollRepository.Delete(poll);
            return Ok();
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (code, codeHash) = _pollService.GenerateSecretCode();
            var poll = new Poll
            {
                Id = Guid.NewGuid(),
                Question = model.Question,
                CreatedOn = DateTime.Now,
                Duration = model.Duration.HasValue ? TimeSpan.FromSeconds(model.Duration.Value) : default(TimeSpan?),
                IsPublic = model.IsPublic,
                SecretCodeHash = codeHash,
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
                PollId = poll.Id.ToString(),
                SecretCode = code
            };

            return Ok(output);
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
        /// <response code="410">If the poll's duration has expired.</response>
        /// <returns></returns>
        [HttpPost("{pollId}/votes")]
        [RecaptchaValidation]
        public async Task<IActionResult> CastVote([FromRoute] string pollId, [FromQuery] string answerId)
        {
            if (!Guid.TryParse(pollId, out var id))
            {
                ModelState.TryAddModelError("ParsingError", $"Poll ID {pollId} is not valid.");
                return BadRequest(ModelState);
            }

            var poll = await _pollRepository.GetById(id);
            if (poll == null)
            {
                return NotFound();
            }

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

            if (!Guid.TryParse(answerId, out var answId))
            {
                ModelState.TryAddModelError("ParsingError", $"Answer ID {answerId} is not valid.");
                return BadRequest(ModelState);
            }

            var answer = poll.Answers.FirstOrDefault(a => a.Id.Equals(answId));
            if (answer == null)
            {
                return NotFound();
            }

            if (poll.Duration != null && poll.CreatedOn.Add(poll.Duration.Value) < DateTime.Now)
            {
                // Poll is expired
                return StatusCode((int) HttpStatusCode.Gone);
            }

            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                Source = addressBytes,
                CastedOn = DateTime.Now,
                OwnerAnswer = answer
            };

            await _voteRepository.Add(vote);

            _messengerHub.Publish(new VoteCastedMessage(poll.Id, answer.Id));

            return Ok();
        }

    }
}
