namespace OrganicShopAPI.Models
{
    public class ShoppingCartItem
    {
        public int Id { get; set; }

        public Product Product { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public int ShoppingCartId { get; set; }
    }
}