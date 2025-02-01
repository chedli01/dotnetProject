using Cars.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Cars.AppDbContext
{
    public class VroomDbContext : IdentityDbContext<IdentityUser>
    {
        public VroomDbContext(DbContextOptions<VroomDbContext> options) :
            base(options)
        {

        }
        public DbSet<Make> Makes { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<Car> Cars { get; set; }

    }
}
