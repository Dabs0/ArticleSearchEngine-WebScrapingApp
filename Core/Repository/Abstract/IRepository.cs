using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repository.Abstract
{
    public interface IRepository<TEntity> where TEntity : class, new()
    {
         GetManyResult<TEntity> GetAll();
         Task<GetManyResult<TEntity>> GetAllAsync();
         GetManyResult<TEntity> FilterBy(Expression<Func<TEntity, bool>> filter);
        public Task<GetManyResult<TEntity>> FilterByAsync(Expression<Func<TEntity, bool>> filter);
        GetOneResult <TEntity> GetById(string id);
        Task<GetOneResult<TEntity>> GetByIdAsync(string id);
        GetOneResult<TEntity> InsertOne(TEntity entity);
        Task<GetOneResult<TEntity>> InsertOneAsync(TEntity entity);
        GetManyResult<TEntity> InsertMany(ICollection<TEntity> entity);
        Task<GetManyResult<TEntity>> InsertManyAsync(ICollection<TEntity> entity);
        GetOneResult<TEntity> ReplaceOne(TEntity entity,string id);
        Task<GetOneResult<TEntity>> ReplaceOneAsync (TEntity entity,string id);
       


    }
}