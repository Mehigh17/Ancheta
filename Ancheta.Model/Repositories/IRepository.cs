using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ancheta.Model.Data;

namespace Ancheta.Model.Repositories
{
    /// <summary>
    /// General repository interface.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be stored.</typeparam>
    public interface IRepository<TEntity, TId>
    {
        Task Add(TEntity entity);
        
        Task<bool> Delete(TEntity entity);
        Task<bool> DeteleById(TId id);

        Task<IReadOnlyList<TEntity>> GetAll();
        Task<TEntity> GetById(TId id);

        Task<bool> Update(TEntity entity);
    }
}