# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["DocumentService.API/DocumentService.API.csproj", "DocumentService.API/"]
COPY ["DocumentService.Application/DocumentService.Application.csproj", "DocumentService.Application/"]
COPY ["DocumentService.Domain/DocumentService.Domain.csproj", "DocumentService.Domain/"]
COPY ["DocumentService.Infrastructure/DocumentService.Infrastructure.csproj", "DocumentService.Infrastructure/"]
COPY ["../shared/Shared.Kernel/Shared.Kernel.csproj", "../shared/Shared.Kernel/"]

# Restore dependencies
RUN dotnet restore "DocumentService.API/DocumentService.API.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/DocumentService.API"
RUN dotnet build "DocumentService.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "DocumentService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Add health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "DocumentService.API.dll"]
