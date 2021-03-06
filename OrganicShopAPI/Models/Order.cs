namespace OrganicShopAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Address { get; set; }

        public ShoppingCart ShoppingCart { get; set; }

        public int ShoppingCartId { get; set; }

        public string DateCreated { get; set; }
    }
}