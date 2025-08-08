# NotificationService Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["shared/Shared.Kernel/Shared.Kernel.csproj", "shared/Shared.Kernel/"]
COPY ["services/NotificationService/NotificationService.API/NotificationService.API.csproj", "services/NotificationService/NotificationService.API/"]

# Restore dependencies
RUN dotnet restore "services/NotificationService/NotificationService.API/NotificationService.API.csproj"

# Copy source code
COPY ["shared/Shared.Kernel/", "shared/Shared.Kernel/"]
COPY ["services/NotificationService/", "services/NotificationService/"]

WORKDIR "/src/services/NotificationService/NotificationService.API"
RUN dotnet build "NotificationService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=https://+:443;http://+:80

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl --fail https://localhost/health || exit 1

ENTRYPOINT ["dotnet", "NotificationService.API.dll"]
