using Microsoft.EntityFrameworkCore;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using Smart_Farm_and_Crop_Yeild_Management_System;

// TEST: Ensure console is working
Console.WriteLine("============================================");
Console.WriteLine("🚀 SMART FARM APPLICATION STARTING...");
Console.WriteLine("============================================");
System.Diagnostics.Debug.WriteLine("🚀 DEBUG: Smart Farm starting...");

var builder = WebApplication.CreateBuilder(args);

// Force Development environment if not already set
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
{
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
}
builder.Environment.EnvironmentName = "Development";

// Force correct web root path - works from both project root and bin folder
var projectRoot = Directory.GetCurrentDirectory();
if (projectRoot.Contains("bin"))
{
    // Running from bin folder (Visual Studio) - go up to project root
    projectRoot = Path.GetFullPath(Path.Combine(projectRoot, "..", "..", ".."));
}
builder.Environment.WebRootPath = Path.Combine(projectRoot, "wwwroot");
builder.Environment.ContentRootPath = projectRoot;

Console.WriteLine("========================================");
Console.WriteLine($"🔧 Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"📁 Project Root: {projectRoot}");
Console.WriteLine($"📁 Content Root: {builder.Environment.ContentRootPath}");
Console.WriteLine($"🌐 Web Root: {builder.Environment.WebRootPath}");
Console.WriteLine($"✅ Web Root Exists: {Directory.Exists(builder.Environment.WebRootPath)}");
if (Directory.Exists(builder.Environment.WebRootPath))
{
    var cssPath = Path.Combine(builder.Environment.WebRootPath, "css");
    Console.WriteLine($"✅ CSS Folder Exists: {Directory.Exists(cssPath)}");
}
Console.WriteLine("========================================");
Console.WriteLine();

// Add MVC services (Controllers + Views)
builder.Services.AddControllersWithViews();

// Add memory cache and HTTP client factory for weather API
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// Register WeatherService
builder.Services.AddScoped<Smart_Farm_and_Crop_Yeild_Management_System.Services.IWeatherService, Smart_Farm_and_Crop_Yeild_Management_System.Services.WeatherService>();

// Register the Database Context to use SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SmartFarmDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    // Suppress pending model changes warning (TEMPORARY - for development only)
    options.ConfigureWarnings(warnings => 
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Add HTTP context accessor (used in controllers & views)
builder.Services.AddHttpContextAccessor();

// Add Session support — keeps track of the logged-in user
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session lasts 30 minutes
    options.Cookie.HttpOnly = true;                 // Prevent JavaScript from reading the cookie
    options.Cookie.IsEssential = true;              // Required for the app to work
});

var app = builder.Build();

// ===== AUTO-APPLY MIGRATIONS AND SEED DATA (CODE FIRST) =====
Console.WriteLine("========================================");
Console.WriteLine("🔄 CHECKING DATABASE STATUS...");
Console.WriteLine("========================================");

try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SmartFarmDbContext>();

        Console.WriteLine($"📊 Database: {db.Database.GetDbConnection().Database}");

        // Check if we should reset the database (only during development)
        bool resetDatabase = false;  // Set to true to force reset

        if (resetDatabase)
        {
            Console.WriteLine("🗑️  Force reset enabled - Deleting old database...");
            db.Database.EnsureDeleted();
            Console.WriteLine("✅ Database deleted");

            // Wait for SQL Server to release locks
            Console.WriteLine("⏳ Waiting for SQL Server to release locks...");
            System.Threading.Thread.Sleep(3000);  // Increased to 3 seconds
        }

        // Apply migrations (creates database from migration files)
        Console.WriteLine("🔧 Applying pending migrations...");
        db.Database.Migrate();
        Console.WriteLine("✅ Migrations applied, waiting for database initialization...");

        // Additional wait to ensure all tables are created
        System.Threading.Thread.Sleep(2000);

        Console.WriteLine("✅ DATABASE MIGRATIONS APPLIED!");

        // Verify seed data
        Console.WriteLine("🔍 Verifying data...");
        var rolesCount = db.Roles.Count();
        var usersCount = db.Users.Count();
        Console.WriteLine($"✅ Found: {rolesCount} roles, {usersCount} users");

        if (usersCount > 0)
        {
            var admin = db.Users.FirstOrDefault(u => u.Username == "admin");
            Console.WriteLine($"✅ Admin user: {admin?.Username} / {admin?.Email}");
        }

        // ===== ONE-TIME BACKFILL: create missing role-specific profile rows =====
        // Older accounts created before AdminController.CreateUser was fixed have a
        // row in Users but no matching Agronomist/FieldOfficer profile row, which
        // makes them invisible in staff-selection dropdowns. Create any missing rows.
        Console.WriteLine("🔧 Backfilling missing Agronomist/FieldOfficer profiles...");

        // Agronomists (RoleId = 4)
        var agronomistUsers = db.Users
            .Where(u => u.RoleId == 4 && !u.IsDeleted)
            .ToList();
        int agronomistsAdded = 0;
        foreach (var u in agronomistUsers)
        {
            if (!db.Agronomists.Any(a => a.UserId == u.UserId))
            {
                db.Agronomists.Add(new Agronomist
                {
                    UserId = u.UserId,
                    FullName = string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName,
                    MobileNumber = string.IsNullOrWhiteSpace(u.Phone) ? "N/A" : u.Phone,
                    Specialization = "General",
                    CreatedDate = DateTime.Now
                });
                agronomistsAdded++;
            }
        }

        // Field Officers (RoleId = 5)
        var fieldOfficerUsers = db.Users
            .Where(u => u.RoleId == 5 && !u.IsDeleted)
            .ToList();
        int fieldOfficersAdded = 0;
        foreach (var u in fieldOfficerUsers)
        {
            if (!db.FieldOfficers.Any(f => f.UserId == u.UserId))
            {
                db.FieldOfficers.Add(new FieldOfficer
                {
                    UserId = u.UserId,
                    FullName = string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName,
                    MobileNumber = string.IsNullOrWhiteSpace(u.Phone) ? "N/A" : u.Phone
                });
                fieldOfficersAdded++;
            }
        }

        // Cooperative Managers (RoleId = 6)
        var managerUsers = db.Users
            .Where(u => u.RoleId == 6 && !u.IsDeleted)
            .ToList();
        int managersAdded = 0;
        foreach (var u in managerUsers)
        {
            if (!db.CooperativeManagers.Any(m => m.UserId == u.UserId))
            {
                db.CooperativeManagers.Add(new CooperativeManager
                {
                    UserId = u.UserId,
                    FullName = string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName,
                    CooperativeName = "N/A",
                    MobileNumber = string.IsNullOrWhiteSpace(u.Phone) ? "N/A" : u.Phone
                });
                managersAdded++;
            }
        }

        if (agronomistsAdded > 0 || fieldOfficersAdded > 0 || managersAdded > 0)
        {
            db.SaveChanges();
        }
        Console.WriteLine($"✅ Backfill complete: {agronomistsAdded} agronomists, {fieldOfficersAdded} field officers, {managersAdded} managers added.");

        Console.WriteLine("✅ SETUP COMPLETE!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
    }
    Console.WriteLine("⚠️  App will continue, but database may not be initialized.");
}

// DEBUG: Print web root path for troubleshooting
Console.WriteLine("========================================");
Console.WriteLine($"🌐 Web Root Path: {app.Environment.WebRootPath}");
Console.WriteLine($"📁 Content Root Path: {app.Environment.ContentRootPath}");
Console.WriteLine("========================================");
Console.WriteLine();

// Configure the request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();  // Serve CSS, JS, images from wwwroot

app.UseRouting();

app.UseSession();      // Must be before UseAuthorization and MapControllerRoute
app.UseAuthorization();

// Default route: Home controller → Index action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
