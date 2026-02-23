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
    var context = scope.ServiceProvider.GetRequiredService<ShopNestDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DbSeeder.SeedAsync(context, logger);
    // await DbSeedThousandRecords.SeedAsync(context, logger);
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

app.UseAuthorization();

app.MapStaticAssets();


app.MapControllerRoute(
    name: "productDetails",
    pattern: "products/details/{id:int}",
    defaults: new { controller = "Products", action = "Index" }
    )
    .WithStaticAssets();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
