using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace ShopNet.Models
{
    public class Product : BaseEntity
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters")]
        [DisplayName("Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between ₹0.01 and ₹9,99,999.99")]
        [Column(TypeName = "decimal(18,2)")] // Explicit DB colume type
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, 100000, ErrorMessage = "Stock must be between 0 and 10,000")]
        [Display(Name = "Stock Quantity")]
        public int Stock { get; set; }

        [Display(Name = "Product Image URL")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        // Navigation property - EF Core uses this to JOIN Table
        public Category? Category { get; set; } = null;

        //Computed properties - not stored in DB
        public bool IsInStock => Stock > 0;
        public decimal GetDiscountedPrice(decimal discountPercentage) => Price - (Price * discountPercentage / 100);
    }
}
