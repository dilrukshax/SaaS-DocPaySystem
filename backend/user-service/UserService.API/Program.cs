using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserService.Application;
using UserService.Domain.Entities;
using UserService.Infrastructure;
using UserService.Infrastructure.Persistence;
using Shared.Kernel.Middleware;
using Shared.Kernel.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<UserDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JWT");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy("RequireManagerRole", policy => policy.RequireRole(Roles.Admin, Roles.Manager));
    options.AddPolicy("RequireApproverRole", policy => policy.RequireRole(Roles.Admin, Roles.Manager, Roles.Approver));
});

// Add Application and Infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<UserDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Global exception handling
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

// Tenant resolution
app.UseMiddleware<TenantResolutionMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    context.Database.EnsureCreated();
    
    // Seed default roles
    await SeedDefaultRoles(scope.ServiceProvider);
}

app.Run();

async Task SeedDefaultRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    
    var defaultRoles = new[]
    {
        new { Name = Roles.Admin, Permissions = new[] { Permissions.UserCreate, Permissions.UserRead, Permissions.UserUpdate, Permissions.UserDelete, Permissions.UserManageRoles } },
        new { Name = Roles.Manager, Permissions = new[] { Permissions.UserRead, Permissions.UserUpdate, Permissions.DocumentCreate, Permissions.DocumentRead, Permissions.DocumentUpdate, Permissions.InvoiceCreate, Permissions.InvoiceRead, Permissions.InvoiceUpdate } },
        new { Name = Roles.Approver, Permissions = new[] { Permissions.DocumentRead, Permissions.InvoiceRead, Permissions.WorkflowApprove, Permissions.WorkflowReject } },
        new { Name = Roles.Viewer, Permissions = new[] { Permissions.DocumentRead, Permissions.InvoiceRead, Permissions.PaymentRead } },
        new { Name = Roles.User, Permissions = new[] { Permissions.DocumentRead, Permissions.InvoiceRead } }
    };

    foreach (var roleData in defaultRoles)
    {
        if (!await roleManager.RoleExistsAsync(roleData.Name))
        {
            var role = new ApplicationRole
            {
                Name = roleData.Name,
                NormalizedName = roleData.Name.ToUpperInvariant(),
                Description = $"Default {roleData.Name} role",
                IsSystemRole = true,
                Permissions = roleData.Permissions.ToList(),
                CreatedAt = DateTime.UtcNow
            };

            await roleManager.CreateAsync(role);
        }
    }
}
