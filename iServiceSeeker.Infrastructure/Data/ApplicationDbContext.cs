using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using iServiceSeeker.Core.Entities;

namespace iServiceSeeker.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<HomeownerProfile> HomeownerProfiles { get; set; }
        public DbSet<ContractorProfile> ContractorProfiles { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<ContractorServiceArea> ContractorServiceAreas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<HomeownerProfile>()
                .HasOne(h => h.User)
                .WithOne(u => u.HomeownerProfile)
                .HasForeignKey<HomeownerProfile>(h => h.UserId);

            builder.Entity<ContractorProfile>()
                .HasOne(c => c.User)
                .WithOne(u => u.ContractorProfile)
                .HasForeignKey<ContractorProfile>(c => c.UserId);

            builder.Entity<ContractorServiceArea>()
                .HasOne(csa => csa.ContractorProfile)
                .WithMany(cp => cp.ServiceAreas)
                .HasForeignKey(csa => csa.ContractorProfileId);

            builder.Entity<ContractorServiceArea>()
                .HasOne(csa => csa.ServiceCategory)
                .WithMany(sc => sc.ContractorServiceAreas)
                .HasForeignKey(csa => csa.ServiceCategoryId);

            // Seed data
            builder.Entity<ServiceCategory>().HasData(
                new ServiceCategory { Id = 1, Name = "General Contracting", Description = "General construction and renovation services" },
                new ServiceCategory { Id = 2, Name = "Plumbing", Description = "Plumbing installation, repair, and maintenance" },
                new ServiceCategory { Id = 3, Name = "Electrical", Description = "Electrical installation, repair, and maintenance" },
                new ServiceCategory { Id = 4, Name = "HVAC", Description = "Heating, ventilation, and air conditioning services" },
                new ServiceCategory { Id = 5, Name = "Roofing", Description = "Roof installation, repair, and maintenance" },
                new ServiceCategory { Id = 6, Name = "Flooring", Description = "Floor installation and refinishing" },
                new ServiceCategory { Id = 7, Name = "Painting", Description = "Interior and exterior painting services" },
                new ServiceCategory { Id = 8, Name = "Landscaping", Description = "Landscape design and maintenance" }
            );
        }
    }
}