using System.Collections.Generic;

namespace OrganicShopAPI.Models
{
    public class ShoppingCart
    {

        public int Id { get; set; }

        public IEnumerable<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();

        public AppUser AppUser { get; set; }

        public int AppUserId { get; set; }

        public string DateCreated { get; set; }

        public string DateModified { get; set; }
    }
}