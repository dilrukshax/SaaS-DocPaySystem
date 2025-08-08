using WorkflowService.Infrastructure.Data;
using Shared.Kernel.Configuration;
using FluentValidation;
using WorkflowService.API.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add shared services
builder.Services.AddSharedServices(builder.Configuration, "WorkflowService");

// Add database
builder.Services.AddDatabase<WorkflowDbContext>(builder.Configuration);

// Add validators
builder.Services.AddScoped<IValidator<CreateWorkflowRequest>, CreateWorkflowRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateWorkflowRequest>, UpdateWorkflowRequestValidator>();
builder.Services.AddScoped<IValidator<StartWorkflowRequest>, StartWorkflowRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSharedMiddleware(app.Environment);

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
