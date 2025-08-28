using iServiceSeeker.Core.Entities;
using iServiceSeeker.Infrastructure.Data;
using iServiceSeeker.Web.Components;
using iServiceSeeker.Web.Components.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using iServiceSeeker.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using AspNet.Security.OAuth.LinkedIn;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// --- START: Identity, Database, & External Auth Configuration ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// This single block configures authentication, including Identity cookies and external providers.
builder.Services.AddAuthentication(options =>
{
    // This sets the default scheme for handling logins to be the cookie-based one from Identity.
    // It is the key to preventing the infinite redirect loop with external providers.
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
}).AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.SaveTokens = true;

    // Important: Set the callback path
    options.CallbackPath = "/signin-google";

    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.ClaimActions.MapJsonKey("picture", "picture");
})
/*.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
    options.SaveTokens = true;

    // Optional: Add additional scopes
    options.Scope.Add("https://graph.microsoft.com/user.read");

    // Optional: Map additional claims
    options.ClaimActions.MapJsonKey("picture", "picture");
    options.ClaimActions.MapJsonKey("locale", "locale");
})*/
.AddLinkedIn(LinkedInAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration["Authentication:LinkedIn:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:LinkedIn:ClientSecret"];
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.SaveTokens = true;

        // Map LinkedIn claims
        options.ClaimActions.MapJsonKey("picture", "profilePicture");
        options.ClaimActions.MapJsonKey("locale", "locale");
    })  .AddIdentityCookies();

// This configures the core Identity system with your custom ApplicationUser.
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // Enables the Roles system (for "Admin", etc.)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// --- END: Identity, Database, & External Auth Configuration ---

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// This allows the frontend to make HTTP calls to the backend API service.
builder.Services.AddHttpClient<UserProfileService>(client => client.BaseAddress = new("http://apiservice"));

builder.Services.AddScoped<UserProfileService>();
var app = builder.Build();


// --- START: Database Reset and Seeding Logic (For Development Only) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // WARNING: This is for development only. It deletes the database on every startup.
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Seed the "Admin" role into the database if it doesn't exist.
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Create a default admin user if one doesn't exist.
        var adminUser = await userManager.FindByEmailAsync("admin@serviceseeker.com");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin@serviceseeker.com",
                Email = "admin@serviceseeker.com",
                FirstName = "Admin",
                LastName = "User",
                UserType = UserType.Admin, // Or another default
                EmailConfirmed = true // Confirm email immediately for the admin
            };
            // IMPORTANT: Use a strong, secure password from your secrets file in a real app!
            await userManager.CreateAsync(adminUser, "AdminPassword1!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating and seeding the DB.");
    }
}
// --- END: Database Reset and Seeding Logic ---


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapRazorPages();
app.MapBlazorHub();

app.Run();