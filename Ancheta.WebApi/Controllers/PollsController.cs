using System;
using System.Linq;
using System.Threading.Tasks;
using Ancheta.Model.Data;
using Ancheta.Model.Repositories;
using Ancheta.Model.Services;
using Ancheta.Model.ViewModels;
using Ancheta.WebApi.Model.Input;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Ancheta.WebApi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class PollsController : ControllerBase
    {

        private const int _MaxPollChunk = 8;
        private readonly IPollRepository _pollRepository;
        private readonly IMapper _mapper;
        public readonly IPollService _pollService;

        public PollsController(IPollRepository pollRepository,
                               IMapper mapper,
                               IPollService pollService)
        {
            _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _pollRepository = pollRepository ?? throw new System.ArgumentNullException(nameof(pollRepository));
        }

        /// <summary>
        /// Fetch a list of unexpired public polls.
        /// </summary>
        /// <param name="offset">The count of polls to ignore from the newest to oldest ones.</param>
        /// <param name="count">The amount of polls to fetch.</param>
        /// <returns>A list of polls.</returns>
        [HttpGet]
        public async Task<IActionResult> GetPublicPolls([FromQuery] int offset, [FromQuery] int count)
        {
            if (count > _MaxPollChunk)
            {
                ModelState.TryAddModelError("limits", $"You can request at most {_MaxPollChunk} polls in a request.");
                return ValidationProblem(ModelState);
            }

            var polls = (await _pollRepository.GetPublicPolls(offset, count)).Select(p => _mapper.Map<Poll, PollDetailViewModel>(p));
            return Ok(polls);
        }

        /// <summary>
        /// Permanently remove a poll from the database.
        /// </summary>
        /// <param name="id">Id of the poll.</param>
        /// <param name="secretCode">Secret code required to manage the poll.</param>
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

                var success = await _pollRepository.Remove(poll);
                if (success) return Ok();
            }

            ModelState.TryAddModelError("InvaidId", "The poll id is not valid.");
            return ValidationProblem(ModelState);
        }

        /// <summary>
        /// Create a poll.
        /// </summary>
        /// <param name="model">The input data for the poll.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreatePoll([FromBody] PollCreationModel model)
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
                    Duration = TimeSpan.FromDays(30.0),
                    IsPublic = model.IsPublic,
                    SecretCodeHash = secret.Item2,
                    Answers = model.Answers.Select(a => new Answer
                    {
                        Id = Guid.NewGuid(),
                        Content = a.Content,
                        Votes = 0
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

    }
}