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
            var polls = await _dbContext.Polls.Include(p => p.Answers)
                                              .ThenInclude(a => a.Votes)
                                              .ToListAsync();
            return polls.AsReadOnly();
        }

        public async Task<Poll> GetById(Guid id)
        {
            return await _dbContext.Polls.Include(p => p.Answers)
                                         .ThenInclude(a => a.Votes)
                                         .FirstOrDefaultAsync(p => p.Id == id);
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
                                              .Where(p => p.Duration == null || p.CreatedOn + p.Duration > DateTime.Now)
                                              .Skip(offset)
                                              .Take(count)
                                              .Include(p => p.Answers)
                                              .ThenInclude(a => a.Votes)
                                              .ToListAsync();
            return polls;
        }

        public async Task<bool> Delete(Poll poll)
        {
            var removalStatus = _dbContext.Remove(poll);
            await _dbContext.SaveChangesAsync();

            var isDeleted = removalStatus.State == EntityState.Deleted || removalStatus.State == EntityState.Detached;
            return isDeleted;
        }

        public async Task<bool> DeteleById(Guid id)
        {
            var poll = await GetById(id);
            if(poll == null) return true;
            
            return await Delete(poll);
        }

        public async Task<bool> DeleteById(Guid id)
        {
            var poll = await _dbContext.Polls.FindAsync(id);
            if (poll != null) return await Delete(poll);
            return true;
        }

        /// <summary>
        /// Update the poll entity in the database.
        /// </summary>
        /// <param name="poll"></param>
        /// <returns></returns>
        public async Task<bool> Update(Poll poll)
        {
            var state = _dbContext.Polls.Update(poll);
            await _dbContext.SaveChangesAsync();

            return state.State == EntityState.Modified;
        }
    }
}