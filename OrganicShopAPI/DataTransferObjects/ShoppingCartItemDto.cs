namespace OrganicShopAPI.DataTransferObjects
{
    public class ShoppingCartItemDto
    {
        public ProductDto Product { get; set; }

        public int Quantity { get; set; }
    }
}