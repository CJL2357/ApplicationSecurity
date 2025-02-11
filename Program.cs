using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Model;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddTransient<EmailSender>();
builder.Services.AddHttpClient(); // Register IHttpClientFactory

// Configure the DbContext to use SQL Server with the connection string from appsettings.json
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthConnectionString")));

// Configure Identity to use the custom ApplicationUser  class
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IPasswordValidator<ApplicationUser>, CustomPasswordValidator>();

// Register the EncryptionService with a secure key
builder.Services.AddSingleton<EncryptionService>(provider =>
    new EncryptionService("eW91ci0zMi1jaGFyLWtleS1oZXJlICAgICAgICAgICA=")); // Replace with your actual key

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure cookie authentication
builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.Cookie.Name = "MyCookieAuth";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBelongToHRDepartment",
        policy => policy.RequireClaim("Department", "HR"));
});

// Configure application cookie settings
builder.Services.ConfigureApplicationCookie(config =>
{
    config.LoginPath = "/Login";
});

builder.Services.AddScoped<AuditService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStatusCodePagesWithRedirects("/errors/{0}");

app.UseRouting();

// Add session middleware before authentication and authorization
app.UseSession(); // Ensure this is before UseAuthentication and UseAuthorization

app.UseAuthentication(); // Ensure authentication middleware is used
app.UseAuthorization(); // Ensure authorization middleware is used

app.MapRazorPages(); // Map Razor Pages

app.Run();