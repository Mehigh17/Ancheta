using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ancheta.Model.Data;
using Ancheta.Model.Repositories;
using Ancheta.WebApi.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Ancheta.WebApi.Repositories
{
    public class VoteRepository : IVoteRepository
    {

        private readonly ApplicationDbContext _dbContext;

        public VoteRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task Add(Vote entity)
        {
            await _dbContext.Votes.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Vote entity)
        {
            _dbContext.Votes.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteById(Guid id)
        {
            var vote = await GetById(id);
            await Delete(vote);
        }

        public async Task<IReadOnlyList<Vote>> GetAll()
        {
            return await _dbContext.Votes.ToListAsync();
        }

        public async Task<Vote> GetById(Guid id)
        {
            return await _dbContext.Votes.FindAsync(id);
        }

        public async Task Update(Vote entity)
        {
            _dbContext.Votes.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}