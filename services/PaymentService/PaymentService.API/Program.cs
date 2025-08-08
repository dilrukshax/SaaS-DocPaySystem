using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Services;
using PaymentService.Infrastructure.Repositories;
using PaymentService.Domain.Interfaces;
using Shared.Kernel.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add shared services
builder.Services.AddSharedServices(builder.Configuration, "PaymentService");

// Add database
builder.Services.AddDatabase<PaymentDbContext>(builder.Configuration);

// Add repositories and services
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IStripePaymentGatewayService, StripePaymentGatewayService>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSharedMiddleware(app.Environment);

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
