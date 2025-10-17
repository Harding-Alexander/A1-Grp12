using Assignment1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Assignment1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Company> Company { get; set; } = default!;
        public DbSet<Product> Product { get; set; } = default!;
        public DbSet<Employee> Employee { get; set; } = default!;
        public DbSet<Message> Messages { get; set; } = default!;



        // data Tables 



    }
}
