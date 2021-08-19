namespace OrganicShopAPI.DataTransferObjects
{
    public class ProductDto {

        public int Id { get; set; }
        public string Title { get; set; }

        public decimal Price { get; set; }

        public string Category { get; set; }

        public string ImageURL { get; set; }
    }
}
