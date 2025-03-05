using System.ComponentModel.DataAnnotations;

namespace OpticalShopWebsite.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Current stock is required.")]
        public int CurrentStock { get; set; }

        // Relative path to the uploaded image
        public string ProductImage { get; set; }
    }
}
