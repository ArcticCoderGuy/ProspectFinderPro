# Deployment Script for ProspectFinderPro

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("development", "production")]
    [string]$Environment,
    
    [switch]$Build,
    [switch]$Migrate,
    [switch]$Seed
)

Write-Host "🚀 Deploying ProspectFinderPro to $Environment..." -ForegroundColor Green

$dockerDir = Join-Path $PSScriptRoot ".." "infrastructure" "docker"
$composeFile = if ($Environment -eq "production") { "docker-compose.production.yml" } else { "docker-compose.yml" }

Push-Location $dockerDir

try {
    # Build if requested
    if ($Build) {
        Write-Host "Building images..." -ForegroundColor Yellow
        docker compose -f $composeFile build --no-cache
    }
    
    # Start services
    Write-Host "Starting services..." -ForegroundColor Yellow
    docker compose -f $composeFile up -d
    
    # Wait for services to be ready
    Write-Host "Waiting for services to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
    
    # Health check
    $apiHealth = Invoke-RestMethod -Uri "http://localhost:5000/health" -ErrorAction SilentlyContinue
    if ($apiHealth -eq "OK") {
        Write-Host "✅ API Gateway is healthy" -ForegroundColor Green
    } else {
        Write-Host "❌ API Gateway health check failed" -ForegroundColor Red
    }
    
    # Database migration
    if ($Migrate) {
        Write-Host "Running database migrations..." -ForegroundColor Yellow
        # Add migration logic here
    }
    
    # Seed data
    if ($Seed) {
        Write-Host "Seeding demo data..." -ForegroundColor Yellow
        Invoke-RestMethod -Uri "http://localhost:5000/api/seed-demo" -Method Post
    }
    
    Write-Host "✅ Deployment completed!" -ForegroundColor Green
    Write-Host "🌐 Web App: http://localhost:5001" -ForegroundColor Cyan
    Write-Host "📚 API Docs: http://localhost:5000/swagger" -ForegroundColor Cyan
    
} catch {
    Write-Host "❌ Deployment failed: $_" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}