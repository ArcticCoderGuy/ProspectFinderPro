# Test Script for ProspectFinderPro

param(
    [ValidateSet("unit", "integration", "e2e", "all")]
    [string]$TestType = "all",
    
    [string]$Filter = "",
    [switch]$Coverage,
    [switch]$Verbose
)

Write-Host "🧪 Running tests for ProspectFinderPro..." -ForegroundColor Green

$rootDir = Join-Path $PSScriptRoot ".."
$testDir = Join-Path $rootDir "tests"

# Build test arguments
$testArgs = @()
if ($Coverage) {
    $testArgs += "--collect:`"XPlat Code Coverage`""
}
if ($Verbose) {
    $testArgs += "--verbosity", "detailed"
}
if ($Filter) {
    $testArgs += "--filter", $Filter
}

Push-Location $rootDir

try {
    switch ($TestType) {
        "unit" {
            Write-Host "Running unit tests..." -ForegroundColor Yellow
            dotnet test "$testDir/Unit" @testArgs
        }
        "integration" {
            Write-Host "Running integration tests..." -ForegroundColor Yellow
            dotnet test "$testDir/Integration" @testArgs
        }
        "e2e" {
            Write-Host "Running E2E tests..." -ForegroundColor Yellow
            # Start services first
            & "$PSScriptRoot/deploy.ps1" -Environment development -Build
            dotnet test "$testDir/Integration/ProspectFinderPro.E2E.Tests" @testArgs
        }
        "all" {
            Write-Host "Running all tests..." -ForegroundColor Yellow
            dotnet test $testDir @testArgs
        }
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ All tests passed!" -ForegroundColor Green
    } else {
        Write-Host "❌ Some tests failed!" -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "❌ Test execution failed: $_" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}