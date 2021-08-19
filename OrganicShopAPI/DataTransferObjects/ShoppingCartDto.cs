using System.Collections.Generic;

namespace OrganicShopAPI.DataTransferObjects
{
    public class ShoppingCartDto
    {
        public int Id { get; set; }

        public List<ShoppingCartItemDto> Items { get; set; } = new();

        public string AppUserId { get; set; }

        public string AppUserName { get; set; }
    }
}
