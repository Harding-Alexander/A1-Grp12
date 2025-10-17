using System.ComponentModel.DataAnnotations;

namespace Assignment1.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Company Name")]
        public string Name { get; set; }

        [Required]
        [Range(0, 300)]
        [Display(Name = "Years In Business")]
        public int YearsInBusiness { get; set; }

        [Required]
        [Url]
        [Display(Name = "Company Website")]
        public string Website { get; set; }

        [Display(Name = "Province")]
        public string? Province { get; set; }
    }
}
