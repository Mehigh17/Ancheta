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
        public AnswerModel[] Answers { get; set; }

        /// <summary>
        /// Duration in seconds.
        /// </summary>
        [Range(3600, 3600 * 24 * 30, ErrorMessage = "The duration should be between 1 hour and 30 days.")]
        public int Duration { get; set; }

        public bool IsPublic { get; set; }

    }
}