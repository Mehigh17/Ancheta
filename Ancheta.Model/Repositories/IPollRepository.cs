using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ancheta.Model.Data;

namespace Ancheta.Model.Repositories
{
    public interface IPollRepository : IRepository<Poll, Guid>
    {
        
        /// <summary>
        /// Fetch a list of public polls.
        /// </summary>
        /// <param name="offset">The offset within the list.</param>
        /// <param name="count">The number of polls to retrieve.</param>
        /// <returns>A readonly list of polls.</returns>
        Task<IReadOnlyList<Poll>> GetPublicPolls(int offset, int count);

    }
}
