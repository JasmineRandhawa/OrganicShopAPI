﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicShopAPI.DataAccess
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll();

        Task<TEntity> Get(int Id);

        Task Add(TEntity tEntity);

        void Delete(TEntity tEntity);
        
    }
}