using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Assignment1.Models
{
    public class ApplicationUser : IdentityUser

    {
        [Display(Name= "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "City")]
        public string? City { get; set; }


    }
}
