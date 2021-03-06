using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ancheta.Model.Data
{
    public class Answer
    {

        [Key]
        public Guid Id { get; set; }
        
        public string Content { get; set; }

        /// <summary>
        /// The poll to whom the answer is assigned to.
        /// </summary>
        public Poll OwnerPoll { get; set; }

        public List<Vote> Votes { get; set; }

    }
}