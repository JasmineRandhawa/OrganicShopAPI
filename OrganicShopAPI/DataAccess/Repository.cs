using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicShopAPI.DataAccess
{
    public class Repository<TEntity>: IRepository<TEntity> where TEntity:class 
    {
       protected readonly OrganicShopDbContext _context;

        public Repository(OrganicShopDbContext context)
        {
            _context = context;
        }

        public IQueryable<TEntity> GetAll()
        {
            return _context.Set<TEntity>();
        }

        public async Task<TEntity> Get(int id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public async Task Add(TEntity tEntity)
        {
            await _context.Set<TEntity>().AddAsync(tEntity);
        }

        public void Delete(TEntity tEntity)
        {
             _context.Set<TEntity>().Remove(tEntity);
        }
    }
}
