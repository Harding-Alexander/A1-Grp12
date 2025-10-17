using System.ComponentModel.DataAnnotations;

namespace Assignment1.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        [Display(Name = "Job Position")]
        public string Position { get; set; } = string.Empty;
        [Required]
        [Range(0, int.MaxValue)]
        public decimal Salary { get; set; }
        [Required]
        public DateTime HireDate { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

    }
}
