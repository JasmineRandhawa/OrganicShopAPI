namespace OrganicShopAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public ShoppingCart ShoppingCart { get; set; }

        public int ShoppingCartId { get; set; }

        public AppUser User { get; set; }

        public int AppUserId { get; set; }

        public string OrderDate { get; set; }
    }
}