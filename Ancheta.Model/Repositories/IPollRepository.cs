using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ancheta.Model.Data;

namespace Ancheta.Model.Repositories
{
    public interface IPollRepository
    {

        Task Add(Poll poll);

        Task<bool> Remove(Poll poll);
        Task<bool> RemoveById(Guid id);

        Task<IReadOnlyList<Poll>> GetAll();
        Task<IReadOnlyList<Poll>> GetPublicPolls(int offset, int limit);

        Task<Poll> GetById(Guid id);

    }
}