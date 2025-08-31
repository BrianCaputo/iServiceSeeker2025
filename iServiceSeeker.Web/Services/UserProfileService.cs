using iServiceSeeker.Core.Entities;
using iServiceSeeker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iServiceSeeker.Web.Services
{
    public class UserProfileService
    {
        private readonly ApplicationDbContext _context;

        public UserProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all available service categories
        /// </summary>
        public async Task<List<ServiceCategory>> GetAllServiceCategoriesAsync()
        {
            return await _context.ServiceCategories
                .Where(sc => sc.IsActive)
                .OrderBy(sc => sc.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Completes homeowner profile creation
        /// </summary>
        public async Task CompleteHomeownerProfileAsync(string userId, HomeownerProfile profile)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Set the user ID
                profile.UserId = userId;

                // Add the homeowner profile
                _context.HomeownerProfiles.Add(profile);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Completes service provider profile creation
        /// </summary>
        public async Task CompleteServiceProviderProfileAsync(
            string userId,
            ServiceProviderProfile profile,
            List<int> serviceCategories,
            ServiceProviderRole role)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Add the service provider profile
                _context.ServiceProviderProfiles.Add(profile);
                await _context.SaveChangesAsync(); // Save to get the profile ID

                // Create the user-service provider relationship
                var userServiceProvider = new UserServiceProvider
                {
                    UserId = userId,
                    ServiceProviderProfileId = profile.Id,
                    Role = role,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow,
                    CanManageProfile = role == ServiceProviderRole.Owner || role == ServiceProviderRole.Manager,
                    CanManageBookings = role != ServiceProviderRole.Member,
                    CanViewReports = role == ServiceProviderRole.Owner || role == ServiceProviderRole.Manager
                };

                _context.UserServiceProviders.Add(userServiceProvider);

                // Add service areas
                foreach (var categoryId in serviceCategories)
                {
                    var serviceArea = new ServiceProviderServiceArea
                    {
                        ServiceProviderProfileId = profile.Id,
                        ServiceCategoryId = categoryId,
                        IsActive = true
                    };
                    _context.ServiceProviderServiceAreas.Add(serviceArea);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Gets service provider profiles for a user
        /// </summary>
        public async Task<List<ServiceProviderProfile>> GetUserServiceProviderProfilesAsync(string userId)
        {
            return await _context.UserServiceProviders
                .Where(usp => usp.UserId == userId && usp.IsActive)
                .Include(usp => usp.ServiceProviderProfile)
                    .ThenInclude(sp => sp.ServiceAreas)
                    .ThenInclude(sa => sa.ServiceCategory)
                .Select(usp => usp.ServiceProviderProfile)
                .ToListAsync();
        }

        /// <summary>
        /// Gets homeowner profile for a user
        /// </summary>
        public async Task<HomeownerProfile?> GetHomeownerProfileAsync(string userId)
        {
            return await _context.HomeownerProfiles
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.UserId == userId);
        }

        /// <summary>
        /// Checks if user has any active service provider profiles
        /// </summary>
        public async Task<bool> HasActiveServiceProviderProfilesAsync(string userId)
        {
            return await _context.UserServiceProviders
                .AnyAsync(usp => usp.UserId == userId && usp.IsActive);
        }

        /// <summary>
        /// Gets user's role in a specific service provider profile
        /// </summary>
        public async Task<ServiceProviderRole?> GetUserRoleInServiceProviderAsync(string userId, int serviceProviderProfileId)
        {
            var userServiceProvider = await _context.UserServiceProviders
                .FirstOrDefaultAsync(usp => usp.UserId == userId
                    && usp.ServiceProviderProfileId == serviceProviderProfileId
                    && usp.IsActive);

            return userServiceProvider?.Role;
        }

        /// <summary>
        /// Updates homeowner profile
        /// </summary>
        public async Task UpdateHomeownerProfileAsync(HomeownerProfile profile)
        {
            _context.HomeownerProfiles.Update(profile);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates service provider profile (only if user has permission)
        /// </summary>
        public async Task UpdateServiceProviderProfileAsync(string userId, ServiceProviderProfile profile)
        {
            // Check if user has permission to manage this profile
            var userServiceProvider = await _context.UserServiceProviders
                .FirstOrDefaultAsync(usp => usp.UserId == userId
                    && usp.ServiceProviderProfileId == profile.Id
                    && usp.IsActive
                    && usp.CanManageProfile);

            if (userServiceProvider == null)
            {
                throw new UnauthorizedAccessException("User does not have permission to manage this service provider profile.");
            }

            _context.ServiceProviderProfiles.Update(profile);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Adds user to an existing service provider profile
        /// </summary>
        public async Task AddUserToServiceProviderAsync(
            string userId,
            int serviceProviderProfileId,
            ServiceProviderRole role,
            bool canManageProfile = false,
            bool canManageBookings = false,
            bool canViewReports = false)
        {
            // Check if relationship already exists
            var existing = await _context.UserServiceProviders
                .FirstOrDefaultAsync(usp => usp.UserId == userId
                    && usp.ServiceProviderProfileId == serviceProviderProfileId);

            if (existing != null)
            {
                if (existing.IsActive)
                {
                    throw new InvalidOperationException("User is already associated with this service provider profile.");
                }

                // Reactivate existing relationship
                existing.IsActive = true;
                existing.Role = role;
                existing.JoinedAt = DateTime.UtcNow;
                existing.LeftAt = null;
                existing.CanManageProfile = canManageProfile;
                existing.CanManageBookings = canManageBookings;
                existing.CanViewReports = canViewReports;
            }
            else
            {
                // Create new relationship
                var userServiceProvider = new UserServiceProvider
                {
                    UserId = userId,
                    ServiceProviderProfileId = serviceProviderProfileId,
                    Role = role,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow,
                    CanManageProfile = canManageProfile,
                    CanManageBookings = canManageBookings,
                    CanViewReports = canViewReports
                };

                _context.UserServiceProviders.Add(userServiceProvider);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes user from service provider profile
        /// </summary>
        public async Task RemoveUserFromServiceProviderAsync(string userId, int serviceProviderProfileId)
        {
            var userServiceProvider = await _context.UserServiceProviders
                .FirstOrDefaultAsync(usp => usp.UserId == userId
                    && usp.ServiceProviderProfileId == serviceProviderProfileId
                    && usp.IsActive);

            if (userServiceProvider != null)
            {
                userServiceProvider.IsActive = false;
                userServiceProvider.LeftAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}