using System.ComponentModel.DataAnnotations;

namespace Ancheta.WebApi.Model.Input
{
    public class AnswerModel
    {

        [Required]
        [MinLength(1, ErrorMessage = "The answer is too short. (At least 1 character required)")]
        [MaxLength(100, ErrorMessage = "The answer is too long. (max. 100 characters)")]
        public string Content { get; set; }

    }
}