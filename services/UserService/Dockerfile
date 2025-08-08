# UserService Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["shared/Shared.Kernel/Shared.Kernel.csproj", "shared/Shared.Kernel/"]
COPY ["services/UserService/UserService.Domain/UserService.Domain.csproj", "services/UserService/UserService.Domain/"]
COPY ["services/UserService/UserService.Infrastructure/UserService.Infrastructure.csproj", "services/UserService/UserService.Infrastructure/"]
COPY ["services/UserService/UserService.API/UserService.API.csproj", "services/UserService/UserService.API/"]

# Restore dependencies
RUN dotnet restore "services/UserService/UserService.API/UserService.API.csproj"

# Copy source code
COPY ["shared/Shared.Kernel/", "shared/Shared.Kernel/"]
COPY ["services/UserService/", "services/UserService/"]

WORKDIR "/src/services/UserService/UserService.API"
RUN dotnet build "UserService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=https://+:443;http://+:80

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl --fail https://localhost/health || exit 1

ENTRYPOINT ["dotnet", "UserService.API.dll"]
