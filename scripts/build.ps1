# Build Script for ProspectFinderPro

param(
    [switch]$NoCache,
    [switch]$Production,
    [string]$Service = ""
)

Write-Host "🔨 Building ProspectFinderPro..." -ForegroundColor Green

$dockerDir = Join-Path $PSScriptRoot ".." "infrastructure" "docker"
$composeFile = if ($Production) { "docker-compose.production.yml" } else { "docker-compose.yml" }

Push-Location $dockerDir

try {
    if ($NoCache) {
        Write-Host "Building with --no-cache..." -ForegroundColor Yellow
        docker compose -f $composeFile build --no-cache $Service
    } else {
        docker compose -f $composeFile build $Service
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Build completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "❌ Build failed!" -ForegroundColor Red
        exit 1
    }
} finally {
    Pop-Location
}