using System;
using System.ComponentModel.DataAnnotations;

namespace Ancheta.Model.Data
{
    public class Vote
    {
        
        [Key]
        public Guid Id { get;set; }

        // The answer that his vote has been casted to.
        public Answer OwnerAnswer { get;set ;}

        /// <summary>
        /// The IP address bytes, in IPv4 format.
        /// </summary>
        public byte[] Source { get; set; }

        public DateTime CastedOn { get; set; }


    }
}