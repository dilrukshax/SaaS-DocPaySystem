using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using System.Text;
using Shared.Kernel.Middleware;
using Shared.Kernel.Services;
using Shared.Kernel.HealthChecks;
using Shared.Kernel.Monitoring;

namespace Shared.Kernel.Configuration;

public static class ServiceConfiguration
{
    public static IServiceCollection AddSharedServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // Add Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        // Add Authorization
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole", policy =>
                policy.RequireRole("Admin"));
            
            options.AddPolicy("RequireManagerRole", policy =>
                policy.RequireRole("Admin", "Manager"));
            
            options.AddPolicy("RequireUserRole", policy =>
                policy.RequireRole("Admin", "Manager", "User"));

            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        // Add Controllers with validation
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });

        // Add FluentValidation
        services.AddValidatorsFromAssemblyContaining<ServiceConfiguration>();

        // Add Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = $"{serviceName} API",
                Version = "v1",
                Description = $"API for {serviceName}",
                Contact = new OpenApiContact
                {
                    Name = "SaaS DocPay System",
                    Email = "support@docpaysystem.com"
                }
            });

            // Add JWT Authentication to Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Add shared services
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddHostedService<EventBusConsumer>();
        services.AddMetrics();

        // Add comprehensive health checks
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var serviceBusConnectionString = configuration.GetConnectionString("ServiceBus");
        var storageConnectionString = configuration.GetConnectionString("Storage");

        services.AddComprehensiveHealthChecks(
            connectionString!,
            serviceBusConnectionString,
            storageConnectionString);

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            if (configuration.GetValue<bool>("Logging:EnableApplicationInsights"))
            {
                builder.AddApplicationInsights();
            }
        });

        return services;
    }

    public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Error handling
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at apps root
            });
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        // Security headers
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Add("Content-Security-Policy", 
                "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'");
            
            await next();
        });

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("AllowAll");

        // Custom middleware
        app.UseMiddleware<TenantResolutionMiddleware>();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<PerformanceMonitoringMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        // Health checks
        app.UseComprehensiveHealthChecks();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        return app;
    }

    public static IServiceCollection AddDatabase<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection")
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString(connectionStringName),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            
            options.EnableSensitiveDataLogging(false);
            options.EnableServiceProviderCaching();
        });

        return services;
    }
}

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            context.Result = new BadRequestObjectResult(new
            {
                Message = "Validation failed",
                Errors = errors
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Implementation not needed
    }
}
