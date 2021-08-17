using System.ComponentModel.DataAnnotations.Schema;

namespace OrganicShopAPI.Models
{
    public class Product {

        public int Id { get; set; }

        public string Title { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal Price { get; set; }

        public Category Category { get; set; }

        public int CategoryId { get; set; }

        public string ImageURL { get; set; }

        public bool IsActive { get; set; }
    }
}
