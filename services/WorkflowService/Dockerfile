# WorkflowService Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["shared/Shared.Kernel/Shared.Kernel.csproj", "shared/Shared.Kernel/"]
COPY ["services/WorkflowService/WorkflowService.Domain/WorkflowService.Domain.csproj", "services/WorkflowService/WorkflowService.Domain/"]
COPY ["services/WorkflowService/WorkflowService.Infrastructure/WorkflowService.Infrastructure.csproj", "services/WorkflowService/WorkflowService.Infrastructure/"]
COPY ["services/WorkflowService/WorkflowService.API/WorkflowService.API.csproj", "services/WorkflowService/WorkflowService.API/"]

# Restore dependencies
RUN dotnet restore "services/WorkflowService/WorkflowService.API/WorkflowService.API.csproj"

# Copy source code
COPY ["shared/Shared.Kernel/", "shared/Shared.Kernel/"]
COPY ["services/WorkflowService/", "services/WorkflowService/"]

WORKDIR "/src/services/WorkflowService/WorkflowService.API"
RUN dotnet build "WorkflowService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WorkflowService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=https://+:443;http://+:80

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl --fail https://localhost/health || exit 1

ENTRYPOINT ["dotnet", "WorkflowService.API.dll"]
