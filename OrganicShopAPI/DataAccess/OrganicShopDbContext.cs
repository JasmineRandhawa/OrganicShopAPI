using Microsoft.EntityFrameworkCore;
using OrganicShopAPI.Models;

namespace OrganicShopAPI.DataAccess
{
    public class OrganicShopDbContext:DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ShoppingCart> ShoppingCart { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }

        public DbSet<AppUser> AppUsers { get; set; }

        public OrganicShopDbContext(DbContextOptions<OrganicShopDbContext> options):base(options)
        {

        }
    }
}