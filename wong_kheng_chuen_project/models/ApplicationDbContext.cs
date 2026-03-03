using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using wong_kheng_chuen_project.data;

namespace wong_kheng_chuen_project.models
{
         public class ApplicationDbContext : IdentityDbContext<IdentityUser>
         {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {

            }
            public DbSet<facility>? facility { get; set; }
            protected override void OnModelCreating(ModelBuilder builder)
            {
                base.OnModelCreating(builder);
            }
    }
}