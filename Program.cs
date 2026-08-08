using Microsoft.EntityFrameworkCore;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;


var builder = WebApplication.CreateBuilder(args);

// Add MVC services (Controllers + Views)
builder.Services.AddControllersWithViews();

// Add memory cache and Allows application to call external APIs.
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// Register WeatherService
builder.Services.AddScoped<Smart_Farm_and_Crop_Yeild_Management_System.Services.IWeatherService, Smart_Farm_and_Crop_Yeild_Management_System.Services.WeatherService>();

// Register the Database Context to use SQL Server Reads Database Connections
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SmartFarmDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add HTTP context to access (used controllers & views)
builder.Services.AddHttpContextAccessor();

// Add Session support — keeps track of the logged-in user
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session lasts 30 minutes
    options.Cookie.HttpOnly = true;                 // Prevent JavaScript from reading the cookie
    options.Cookie.IsEssential = true;              // Required for the app to work
});

var app = builder.Build();

// Apply migrations and backfill legacy role profiles.
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SmartFarmDbContext>();

    db.Database.Migrate();

    var agronomistUsers = db.Users
        .Where(u => u.RoleId == 4 && !u.IsDeleted)
        .ToList();
    var fieldOfficerUsers = db.Users
        .Where(u => u.RoleId == 5 && !u.IsDeleted)
        .ToList();
    var managerUsers = db.Users
        .Where(u => u.RoleId == 6 && !u.IsDeleted)
        .ToList();

    var hasChanges = false;

    foreach (var user in agronomistUsers)
    {
        if (db.Agronomists.Any(a => a.UserId == user.UserId))
        {
            continue;
        }

        db.Agronomists.Add(new Agronomist
        {
            UserId = user.UserId,
            FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName,
            MobileNumber = string.IsNullOrWhiteSpace(user.Phone) ? "N/A" : user.Phone,
            Specialization = "General",
            CreatedDate = DateTime.Now
        });

        hasChanges = true;
    }

    foreach (var user in fieldOfficerUsers)
    {
        if (db.FieldOfficers.Any(f => f.UserId == user.UserId))
        {
            continue;
        }

        db.FieldOfficers.Add(new FieldOfficer
        {
            UserId = user.UserId,
            FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName,
            MobileNumber = string.IsNullOrWhiteSpace(user.Phone) ? "N/A" : user.Phone
        });

        hasChanges = true;
    }

    foreach (var user in managerUsers)
    {
        if (db.CooperativeManagers.Any(m => m.UserId == user.UserId))
        {
            continue;
        }

        db.CooperativeManagers.Add(new CooperativeManager
        {
            UserId = user.UserId,
            FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName,
            CooperativeName = "N/A",
            MobileNumber = string.IsNullOrWhiteSpace(user.Phone) ? "N/A" : user.Phone
        });

        hasChanges = true;
    }

    if (hasChanges)
    {
        db.SaveChanges();
    }
}
catch
{
    // Continue startup even if migration/bootstrap steps fail.
}

// Configure the request to MiddleWare pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();  // Serve CSS, JS, images from wwwroot

app.UseRouting();

app.UseSession();      // login session will work because of this line.
app.UseAuthorization();

// Conventional Routing : Home controller → Index action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
