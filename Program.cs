using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using ShopNet.Data;
using ShopNet.Data.Seed;
using ShopNet.Models;
using ShopNet.Repository;
using ShopNet.Repository.IRepository;
using ShopNet.Services;
using ShopNet.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database — Register DbContext with PostGreSQL Via Npgsql
builder.Services.AddDbContext<ShopNestDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity — registered BEFORE anything that depends on it
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{

    // ── Password Policy ───────────────────────────────────
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // ── Lockout Policy ─────────────────────────────────────
    // After 5 wrong attempts → lock for 15 minutes
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // ── User Policy ────────────────────────────────────────
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false; // Set true for email verification

}).AddEntityFrameworkStores<ShopNestDbContext>()    // Use out DbContext for Identity Tables
.AddDefaultTokenProviders();                        // For password reset, email confirm tokens

// Configure the auth cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";                       // Redirect here if not authenticated
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";          // Redirect here if not authorized
    options.SlidingExpiration = true;                           // Reset expiry on each request.
    options.ExpireTimeSpan = TimeSpan.FromDays(7);              // Cookie valid for 7 days with RememberMe
    options.Cookie.HttpOnly = true;                             // Js Cannot access auth cookie
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;    // HTTPS only
});

// Authorization Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AdminOrStoreManager", policy => policy.RequireRole("Admin", "StoreManager"));
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
});

// Session - for shopping cart
// Session uses an in-memory cache + a cookie to identiy the user
builder.Services.AddDistributedMemoryCache(); // Backing store for session.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Cart expired after 30min idle
    options.Cookie.HttpOnly = true;                 // JS Cant read the session cookie
    options.Cookie.IsEssential = true;              // GDPR - session works even without cookie consent.
});

// HTTPContextAccessor - Lets CartSerice access session outside of Controllers
builder.Services.AddHttpContextAccessor();

//Repositories - Scoped means one instance per HTTP Request
// This is correct for EF Core because DbContext is also Scoped.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Services - also Scoped
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();


// ------ PIPELINE --------------
var app = builder.Build();

// Seed the database on Startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var context = scope.ServiceProvider.GetRequiredService<ShopNestDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        await DbSeeder.SeedAsync(context, roleManager, userManager, logger);
        // await DbSeedThousandRecords.SeedAsync(context, logger);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Seeding failed: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();    // ← Must come AFTER UseRouting, BEFORE UseAuthorization


// ORDER IS CRITICAL: Authentication BEFORE Authorization
// UseAuthentication reads the cookie and populates HttpContext.User
// UseAuthorization checks HttpContext.User against [Authorize] attributes
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
