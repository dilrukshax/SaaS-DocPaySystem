using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using DocumentService.Application.Interfaces;
using DocumentService.Infrastructure.Persistence;
using DocumentService.Infrastructure.Repositories;
using DocumentService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<DocumentDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Azure Services
        services.AddSingleton<BlobServiceClient>(provider =>
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            return new BlobServiceClient(connectionString);
        });

        services.AddSingleton<ServiceBusSender>(provider =>
        {
            var connectionString = configuration.GetConnectionString("ServiceBus");
            var topicName = configuration["ServiceBus:DocumentTopicName"] ?? "document-events";
            var client = new ServiceBusClient(connectionString);
            return client.CreateSender(topicName);
        });

        services.AddSingleton<DocumentAnalysisClient>(provider =>
        {
            var endpoint = configuration["AzureFormRecognizer:Endpoint"];
            var credential = new DefaultAzureCredential();
            return new DocumentAnalysisClient(new Uri(endpoint!), credential);
        });

        // Application Services
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IBlobStorageService>(provider =>
        {
            var blobClient = provider.GetRequiredService<BlobServiceClient>();
            var containerName = configuration["AzureStorage:ContainerName"] ?? "documents";
            return new BlobStorageService(blobClient, containerName);
        });
        services.AddScoped<IOCRService, OCRService>();
        services.AddScoped<IEventPublisher, EventPublisher>();

        return services;
    }
}
