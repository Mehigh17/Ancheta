using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ancheta.Model.Data
{
    public class Poll
    {

        [Key]
        public Guid Id { get; set; }

        public string Question { get; set; }
        
        public List<Answer> Answers { get; set; }

        public bool IsPublic { get; set; }

        public DateTime CreatedOn { get; set; }

        public TimeSpan? Duration { get; set; }

        public bool AllowMultipleVotesPerIp { get; set; }

        public string SecretCodeHash { get; set; }

    }
}