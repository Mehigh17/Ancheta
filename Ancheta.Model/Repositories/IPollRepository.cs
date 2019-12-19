using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ancheta.Model.Data;

namespace Ancheta.Model.Repositories
{
    public interface IPollRepository : IRepository<Poll, Guid>
    {

        Task<IReadOnlyList<Poll>> GetPublicPolls(int offset, int limit);

    }
}