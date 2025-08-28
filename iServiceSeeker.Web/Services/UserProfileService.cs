using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using iServiceSeeker.Core.Entities;
using iServiceSeeker.Infrastructure.Data;

namespace iServiceSeeker.Web.Services
{
    public class UserProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserProfileService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ApplicationUser?> GetUserWithProfileAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.HomeownerProfile)
                .Include(u => u.ContractorProfile)
                    .ThenInclude(cp => cp!.ServiceAreas)
                    .ThenInclude(sa => sa.ServiceCategory)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> CompleteHomeownerProfileAsync(string userId, HomeownerProfile profile)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            profile.UserId = userId;
            _context.HomeownerProfiles.Add(profile);

            user.UserType = UserType.Homeowner;
            user.IsProfileComplete = true;

            await _userManager.UpdateAsync(user);
            await _userManager.AddToRoleAsync(user, "Homeowner");
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> CompleteContractorProfileAsync(string userId, ContractorProfile profile, List<int> serviceCategories)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            profile.UserId = userId;
            _context.ContractorProfiles.Add(profile);

            // Add service areas
            foreach (var categoryId in serviceCategories)
            {
                profile.ServiceAreas.Add(new ContractorServiceArea
                {
                    ServiceCategoryId = categoryId,
                    IsActive = true
                });
            }

            user.UserType = UserType.Contractor;
            user.IsProfileComplete = true;

            await _userManager.UpdateAsync(user);
            await _userManager.AddToRoleAsync(user, "Contractor");
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ServiceCategory>> GetAllServiceCategoriesAsync()
        {
            return await _context.ServiceCategories
                .Where(sc => sc.IsActive)
                .OrderBy(sc => sc.Name)
                .ToListAsync();
        }
    }
}