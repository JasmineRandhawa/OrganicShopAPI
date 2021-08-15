namespace OrganicShopAPI.Models
{
    public class Product {

        public int Id { get; set; }

        public string Title { get; set; }

        public Category Category { get; set; }

        public int CategoryId { get; set; }

        public string ImageURL { get; set; }

        public bool IsActive { get; set; }
    }
}
