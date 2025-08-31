using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using iServiceSeeker.Core.Entities;

namespace iServiceSeeker.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<UserServiceProvider> UserServiceProviders { get; set; }
        // DbSets
        public DbSet<HomeownerProfile> HomeownerProfiles { get; set; }
        public DbSet<ServiceProviderProfile> ServiceProviderProfiles { get; set; }
        public DbSet<UserServiceProvider> ServiceProviders { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<ServiceProviderServiceArea> ServiceProviderServiceAreas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure HomeownerProfile relationship (One-to-One)
            builder.Entity<HomeownerProfile>()
                .HasOne(h => h.User)
                .WithOne(u => u.HomeownerProfile)
                .HasForeignKey<HomeownerProfile>(h => h.UserId);

            // Configure UserServiceProvider relationships (Many-to-Many junction table)
            builder.Entity<UserServiceProvider>()
                .HasOne(usp => usp.User)
                .WithMany(u => u.ServiceProviders)
                .HasForeignKey(usp => usp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserServiceProvider>()
                .HasOne(usp => usp.ServiceProviderProfile)
                .WithMany(sp => sp.ServiceProviders)
                .HasForeignKey(usp => usp.ServiceProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ServiceProviderServiceArea relationships
            builder.Entity<ServiceProviderServiceArea>()
                .HasOne(spsa => spsa.ServiceProviderProfile)
                .WithMany(sp => sp.ServiceAreas)
                .HasForeignKey(spsa => spsa.ServiceProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ServiceProviderServiceArea>()
                .HasOne(spsa => spsa.ServiceCategory)
                .WithMany(sc => sc.ServiceProviderServiceAreas)
                .HasForeignKey(spsa => spsa.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting categories that are in use

            // Add indexes for better performance
            builder.Entity<UserServiceProvider>()
                .HasIndex(usp => new { usp.UserId, usp.ServiceProviderProfileId })
                .IsUnique();

            builder.Entity<ServiceProviderProfile>()
                .HasIndex(sp => sp.CompanyName);

            builder.Entity<ServiceProviderProfile>()
                .HasIndex(sp => sp.LicenseNumber)
                .IsUnique()
                .HasFilter("[LicenseNumber] IS NOT NULL");

            builder.Entity<ServiceCategory>()
                .HasIndex(sc => sc.Name)
                .IsUnique();

            // Configure decimal precision
            builder.Entity<ServiceProviderProfile>()
                .Property(sp => sp.ServiceRadius)
                .HasPrecision(10, 2);

            // Configure string lengths (optional - can also be handled by attributes)
            builder.Entity<ApplicationUser>()
                .Property(u => u.FirstName)
                .HasMaxLength(100);

            builder.Entity<ApplicationUser>()
                .Property(u => u.LastName)
                .HasMaxLength(100);

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