using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public int Votes { get; set; }

    }
}