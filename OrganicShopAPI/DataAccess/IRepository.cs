using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganicShopAPI.DataAccess
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IEnumerable<TEntity> GetAll();
        Task Add(TEntity tEntity);

        Task<TEntity> Get(int Id);
    }
}