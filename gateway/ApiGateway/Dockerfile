# API Gateway Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared kernel first
COPY ["shared/Shared.Kernel/Shared.Kernel.csproj", "shared/Shared.Kernel/"]

# Copy gateway project
COPY ["gateway/ApiGateway/ApiGateway.csproj", "gateway/ApiGateway/"]

# Restore dependencies
RUN dotnet restore "gateway/ApiGateway/ApiGateway.csproj"

# Copy source code
COPY ["shared/Shared.Kernel/", "shared/Shared.Kernel/"]
COPY ["gateway/ApiGateway/", "gateway/ApiGateway/"]

WORKDIR "/src/gateway/ApiGateway"
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p logs

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=https://+:443;http://+:80

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl --fail https://localhost/health || exit 1

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
