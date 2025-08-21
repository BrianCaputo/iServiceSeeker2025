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
        public ContractorProfile? ContractorProfile { get; set; }

        // Computed property
        public string FullName => $"{FirstName} {LastName}";
    }

    public enum UserType
    {
        Homeowner = 1,
        Contractor = 2,
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

    public class ContractorProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

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
        public decimal ServiceRadius { get; set; } = 25; // miles

        [StringLength(1000)]
        public string? Description { get; set; }
        public string? Website { get; set; }

        // Service areas
        public List<ContractorServiceArea> ServiceAreas { get; set; } = new();
    }

    public class ContractorServiceArea
    {
        public int Id { get; set; }
        public int ContractorProfileId { get; set; }
        public ContractorProfile ContractorProfile { get; set; } = null!;
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
        public List<ContractorServiceArea> ContractorServiceAreas { get; set; } = new();
    }
}