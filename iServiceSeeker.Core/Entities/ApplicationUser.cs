using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace iServiceSeeker.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public UserType UserType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsProfileComplete { get; set; } = false;

        // Navigation properties
        public HomeownerProfile? HomeownerProfile { get; set; }

        // Many-to-many relationship with ServiceProviderProfiles
        public List<UserServiceProvider> ServiceProviders { get; set; } = new();

        // Computed property
        public string FullName => $"{FirstName} {LastName}";
    }

    public enum UserType
    {
        Homeowner = 1,
        ServiceProvider = 2, 
        Admin = 3
    }

    public class HomeownerProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [StringLength(500)]
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public bool ReceiveEmailNotifications { get; set; } = true;
        public bool ReceiveSmsNotifications { get; set; } = false;
    }

    // Junction table for many-to-many relationship
    public class UserServiceProvider
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public int ServiceProviderProfileId { get; set; }
        public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;

        // Role within this service provider (Owner, Employee, Partner, etc.)
        public ServiceProviderRole Role { get; set; } = ServiceProviderRole.Member;
        public bool IsActive { get; set; } = true;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }

        // Permissions within this service provider
        public bool CanManageProfile { get; set; } = false;
        public bool CanManageBookings { get; set; } = false;
        public bool CanViewReports { get; set; } = false;
    }

    public enum ServiceProviderRole
    {
        Member = 1,
        Manager = 2,
        Owner = 3,
        Employee = 4,
        Partner = 5
    }

    public class ServiceProviderProfile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? LicenseNumber { get; set; }

        [StringLength(500)]
        public string? BusinessAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }

        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public decimal ServiceRadius { get; set; } = 50; // miles

        [StringLength(1000)]
        public string? Description { get; set; }
        public string? Website { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public List<UserServiceProvider> ServiceProviders { get; set; } = new();
        public List<ServiceProviderServiceArea> ServiceAreas { get; set; } = new();
    }

    public class ServiceProviderServiceArea
    {
        public int Id { get; set; }
        public int ServiceProviderProfileId { get; set; }
        public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;
        public int ServiceCategoryId { get; set; }
        public ServiceCategory ServiceCategory { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }

    public class ServiceCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public List<ServiceProviderServiceArea> ServiceProviderServiceAreas { get; set; } = new();
    }
}