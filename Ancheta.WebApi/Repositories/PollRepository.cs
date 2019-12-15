using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ancheta.Model.Data;
using Ancheta.Model.Repositories;
using Ancheta.WebApi.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Ancheta.Repositories
{
    public class PollRepository : IPollRepository
    {

        private readonly ApplicationDbContext _dbContext;

        public PollRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task Add(Poll poll)
        {
            await _dbContext.AddAsync(poll);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Poll>> GetAll()
        {
            var polls = await _dbContext.Polls.ToListAsync();
            return polls.AsReadOnly();
        }

        public async Task<Poll> GetById(Guid id)
        {
            return await _dbContext.Polls.FindAsync(id);
        }

        /// <summary>
        /// Fetches polls within given bounds that are marked as public.
        /// </summary>
        /// <param name="offset">The offset from the first poll in the database.</param>
        /// <param name="count">The amount of polls to take.</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<Poll>> GetPublicPolls(int offset, int count)
        {
            var polls = await _dbContext.Polls.Where(p => p.IsPublic)
                                              .Where(p => p.CreatedOn + p.Duration > DateTime.Now)
                                              .Skip(offset)
                                              .Take(count)
                                              .Include(p => p.Answers)
                                              .ToListAsync();
            return polls;
        }
        
        public async Task<bool> Remove(Poll poll)
        {
            var removalStatus = _dbContext.Remove(poll);
            await _dbContext.SaveChangesAsync();

            var isDeleted = removalStatus.State == EntityState.Deleted || removalStatus.State == EntityState.Detached;
            return isDeleted;
        }

        public async Task<bool> RemoveById(Guid id)
        {
            var poll = await _dbContext.Polls.FindAsync(id);
            if (poll != null) return await Remove(poll);
            return true;
        }
    }
}