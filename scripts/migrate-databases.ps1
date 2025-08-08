#!/usr/bin/env powershell
# Database Migration Script for SaaS DocPay System
# Run this script to create and apply all Entity Framework migrations

param(
    [string]$Action = "migrate",  # migrate, create, update, reset
    [string]$Service = "all"      # all, user, payment, notification, workflow
)

function Write-Section {
    param([string]$Title)
    Write-Host "`n" -NoNewLine
    Write-Host "=" * 60 -ForegroundColor Cyan
    Write-Host " $Title" -ForegroundColor Yellow
    Write-Host "=" * 60 -ForegroundColor Cyan
}

function Run-Migration {
    param(
        [string]$ServicePath,
        [string]$ServiceName,
        [string]$ConnectionString
    )
    
    Write-Host "`n🔄 Processing $ServiceName Service..." -ForegroundColor Green
    
    Push-Location $ServicePath
    
    try {
        if ($Action -eq "create" -or $Action -eq "migrate") {
            Write-Host "📝 Creating migration for $ServiceName..." -ForegroundColor Blue
            $migrationName = "Initial_$ServiceName`_Migration_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
            dotnet ef migrations add $migrationName --verbose
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Migration created successfully for $ServiceName" -ForegroundColor Green
            } else {
                Write-Host "❌ Failed to create migration for $ServiceName" -ForegroundColor Red
                return $false
            }
        }
        
        if ($Action -eq "migrate" -or $Action -eq "update") {
            Write-Host "🗄️  Updating database for $ServiceName..." -ForegroundColor Blue
            dotnet ef database update --verbose
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Database updated successfully for $ServiceName" -ForegroundColor Green
            } else {
                Write-Host "❌ Failed to update database for $ServiceName" -ForegroundColor Red
                return $false
            }
        }
        
        if ($Action -eq "reset") {
            Write-Host "🗑️  Resetting database for $ServiceName..." -ForegroundColor Yellow
            dotnet ef database drop --force --verbose
            dotnet ef database update --verbose
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Database reset successfully for $ServiceName" -ForegroundColor Green
            } else {
                Write-Host "❌ Failed to reset database for $ServiceName" -ForegroundColor Red
                return $false
            }
        }
        
        return $true
    }
    catch {
        Write-Host "❌ Error processing $ServiceName`: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    finally {
        Pop-Location
    }
}

function Test-Prerequisites {
    Write-Section "Testing Prerequisites"
    
    # Check if dotnet EF tool is installed
    Write-Host "🔍 Checking for dotnet-ef tool..." -ForegroundColor Blue
    $efTool = dotnet tool list --global | Select-String "dotnet-ef"
    
    if (-not $efTool) {
        Write-Host "📦 Installing dotnet-ef tool..." -ForegroundColor Yellow
        dotnet tool install --global dotnet-ef
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ dotnet-ef tool installed successfully" -ForegroundColor Green
        } else {
            Write-Host "❌ Failed to install dotnet-ef tool" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "✅ dotnet-ef tool is already installed" -ForegroundColor Green
    }
    
    # Check if SQL Server is running
    Write-Host "🔍 Checking SQL Server connectivity..." -ForegroundColor Blue
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:1433" -TimeoutSec 5 -ErrorAction SilentlyContinue
    } catch {
        # Expected to fail, but if it connects, SQL Server is running
    }
    
    # Test with docker
    $sqlContainer = docker ps --filter "name=saas-sqlserver" --format "{{.Status}}"
    if ($sqlContainer -like "*Up*") {
        Write-Host "✅ SQL Server container is running" -ForegroundColor Green
    } else {
        Write-Host "❌ SQL Server container is not running. Please start with: docker-compose up -d sqlserver" -ForegroundColor Red
        exit 1
    }
}

function Main {
    Write-Section "SaaS DocPay System - Database Migration Tool"
    Write-Host "Action: $Action" -ForegroundColor Cyan
    Write-Host "Service: $Service" -ForegroundColor Cyan
    
    Test-Prerequisites
    
    # Define services and their paths
    $services = @{
        "user" = @{
            "path" = "services/UserService/UserService.API"
            "name" = "User"
        }
        "payment" = @{
            "path" = "services/PaymentService/PaymentService.API"
            "name" = "Payment"
        }
        "notification" = @{
            "path" = "services/NotificationService/NotificationService.API"
            "name" = "Notification"
        }
        "workflow" = @{
            "path" = "services/WorkflowService/WorkflowService.API"
            "name" = "Workflow"
        }
    }
    
    $baseDir = Split-Path -Parent $PSScriptRoot
    if (-not $baseDir) {
        $baseDir = Get-Location
    }
    
    $successCount = 0
    $totalCount = 0
    
    Write-Section "Running Migrations"
    
    foreach ($serviceKey in $services.Keys) {
        if ($Service -eq "all" -or $Service -eq $serviceKey) {
            $serviceInfo = $services[$serviceKey]
            $servicePath = Join-Path $baseDir $serviceInfo.path
            
            if (Test-Path $servicePath) {
                $totalCount++
                if (Run-Migration -ServicePath $servicePath -ServiceName $serviceInfo.name) {
                    $successCount++
                }
            } else {
                Write-Host "⚠️  Service path not found: $servicePath" -ForegroundColor Yellow
            }
        }
    }
    
    Write-Section "Migration Summary"
    Write-Host "Total Services: $totalCount" -ForegroundColor Cyan
    Write-Host "Successful: $successCount" -ForegroundColor Green
    Write-Host "Failed: $($totalCount - $successCount)" -ForegroundColor Red
    
    if ($successCount -eq $totalCount) {
        Write-Host "`n🎉 All migrations completed successfully!" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "`n❌ Some migrations failed. Please check the errors above." -ForegroundColor Red
        exit 1
    }
}

# Show usage if help is requested
if ($args -contains "--help" -or $args -contains "-h") {
    Write-Host @"
SaaS DocPay System - Database Migration Tool

Usage:
  .\migrate-databases.ps1 [Action] [Service]

Actions:
  migrate  - Create migration and update database (default)
  create   - Create migration only
  update   - Update database only
  reset    - Drop and recreate database

Services:
  all           - Process all services (default)
  user          - User Service only
  payment       - Payment Service only
  notification  - Notification Service only
  workflow      - Workflow Service only

Examples:
  .\migrate-databases.ps1                     # Migrate all services
  .\migrate-databases.ps1 create user         # Create migration for User service only
  .\migrate-databases.ps1 reset payment       # Reset Payment service database
  .\migrate-databases.ps1 update all          # Update all databases

Prerequisites:
  - Docker containers must be running (docker-compose up -d)
  - .NET 8 SDK installed
  - Entity Framework CLI tool (will be installed if missing)
"@
    exit 0
}

# Run the main function
Main
