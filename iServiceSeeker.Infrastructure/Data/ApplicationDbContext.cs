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

            // Note: Seed data is now handled by the SQL script
            // No need to add seed data here since it's already in the database
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // This helps with some EF Core issues when migrations aren't used
                optionsBuilder.EnableSensitiveDataLogging(false);
                optionsBuilder.EnableDetailedErrors(true);
            }
        }
    }
}