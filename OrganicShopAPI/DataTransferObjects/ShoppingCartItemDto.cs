namespace OrganicShopAPI.DataTransferObjects
{
    public class ShoppingCartItemDto
    {
        public int Id { get; set; }

        public ProductDto Product { get; set; }

        public int Quantity { get; set; }

        public int ShoppingCartId { get; set; }
    }
}