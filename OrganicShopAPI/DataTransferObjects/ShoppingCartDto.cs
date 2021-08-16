using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicShopAPI.DataTransferObjects
{
    public class ShoppingCartDto
    {
        public int Id { get; set; }

        public List<ShoppingCartItemDto> Items { get; set; } = new();

        public int AppUserId { get; set; }

        public string AppUserName { get; set; }
    }
}
