using System.Collections.Generic;

namespace OrganicShopAPI.Models
{
    public class ShoppingCart
    {

        public int Id { get; set; }

        public IEnumerable<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();

        public string AppUserId { get; set; }

        public string AppUserName { get; set; } = string.Empty;

        public string DateCreated { get; set; }

        public string DateModified { get; set; }
    }
}