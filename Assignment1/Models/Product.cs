using System.ComponentModel.DataAnnotations;

namespace Assignment1.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Product Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string Category { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public decimal Price { get; set; }
        [Required]
        [Range (0, int.MaxValue)]
        public int StockQuantity { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Company Name")]
        public string CompanyName {  get; set; }


    }
}
