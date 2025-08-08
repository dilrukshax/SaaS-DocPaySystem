using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Services;
using Shared.Kernel.Configuration;
using FluentValidation;
using NotificationService.API.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add shared services
builder.Services.AddSharedServices(builder.Configuration, "NotificationService");

// Add database
builder.Services.AddDatabase<NotificationDbContext>(builder.Configuration);

// Add notification services
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();

// Add validators
builder.Services.AddScoped<IValidator<SendNotificationRequest>, SendNotificationRequestValidator>();
builder.Services.AddScoped<IValidator<CreateTemplateRequest>, CreateTemplateRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateTemplateRequest>, UpdateTemplateRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSharedMiddleware(app.Environment);

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
