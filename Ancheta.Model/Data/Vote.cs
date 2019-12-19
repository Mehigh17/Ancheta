using System;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Ancheta.Model.Data
{
    public class Vote
    {
        
        [Key]
        public Guid Id { get;set; }

        // The answer that his vote has been casted to.
        public Answer OwnerAnswer { get;set ;}

        public IPAddress Source { get; set; }

        public DateTime CastedOn { get; set; }


    }
}