using System;
using System.ComponentModel.DataAnnotations;

namespace Ancheta.WebApi.Model.Input
{
    public class PollCreationModel
    {

        [Required(ErrorMessage = "You must provide a question for your poll.")]
        [MinLength(10, ErrorMessage = "The poll question is too short! (min. 10 characters)")]
        [MaxLength(100, ErrorMessage = "The poll question is too long! (max. 100 characters)")]
        public string Question { get; set; }

        [Required(ErrorMessage = "You need to provide at least a couple of options.")]
        [MinLength(2, ErrorMessage = "At least two answers are required.")]
        [MaxLength(20, ErrorMessage = "You cannot have more than 20 answers.")]
        public AnswerModel[] Answers { get; set; }

        /// <summary>
        /// Duration in seconds, null for infinite duration.
        /// </summary>
        [Range(60 * 30, int.MaxValue, ErrorMessage = "The duration of the poll must be at least 30 minutes.")]
        public int? Duration { get; set; }
        
        public bool AllowMultipleVotesPerIp { get; set; }
        
        public bool IsPublic { get; set; }

    }
}