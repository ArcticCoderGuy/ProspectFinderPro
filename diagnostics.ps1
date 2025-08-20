# ProspectFinderPro Diagnostic Script
# Kerää tiedot ASP.NET Core konfiguraation debuggausta varten

Write-Host "=== ProspectFinderPro Diagnostic Report ===" -ForegroundColor Green
Write-Host "Generated: $(Get-Date)" -ForegroundColor Yellow
Write-Host ""

# 1. Docker container status
Write-Host "1. DOCKER CONTAINERS STATUS:" -ForegroundColor Cyan
try {
    docker compose -f docker-compose.min.yml ps
} catch {
    Write-Host "Error checking Docker containers: $_" -ForegroundColor Red
}
Write-Host ""

# 2. Container environment variables
Write-Host "2. WEBAPP CONTAINER ENVIRONMENT:" -ForegroundColor Cyan
try {
    docker exec finprsql-webapp-1 env | Sort-Object
} catch {
    Write-Host "Error checking container environment: $_" -ForegroundColor Red
}
Write-Host ""

# 3. Container appsettings.json
Write-Host "3. WEBAPP CONTAINER APPSETTINGS.JSON:" -ForegroundColor Cyan
try {
    docker exec finprsql-webapp-1 cat /app/appsettings.json
    Write-Host ""
    Write-Host "Production appsettings:"
    docker exec finprsql-webapp-1 ls -la /app/appsettings*.json
} catch {
    Write-Host "Error reading appsettings: $_" -ForegroundColor Red
}
Write-Host ""

# 4. API Gateway connectivity from WebApp container
Write-Host "4. NETWORK CONNECTIVITY (WebApp -> API Gateway):" -ForegroundColor Cyan
Write-Host "Testing internal Docker network (api-gateway:8080):"
try {
    docker exec finprsql-webapp-1 wget -qO- --timeout=5 http://api-gateway:8080/health
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Write-Host ""

Write-Host "Testing external localhost (localhost:5000):"
try {
    docker exec finprsql-webapp-1 wget -qO- --timeout=5 http://host.docker.internal:5000/health
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Write-Host ""

# 5. Host machine connectivity
Write-Host "5. HOST MACHINE CONNECTIVITY:" -ForegroundColor Cyan
Write-Host "API Gateway health check:"
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/health" -TimeoutSec 5
    Write-Host "✓ API Gateway: $response" -ForegroundColor Green
} catch {
    Write-Host "✗ API Gateway not reachable: $_" -ForegroundColor Red
}

Write-Host "WebApp accessibility:"
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5001/search" -TimeoutSec 5 -UseBasicParsing
    Write-Host "✓ WebApp: HTTP $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "✗ WebApp not reachable: $_" -ForegroundColor Red
}
Write-Host ""

# 6. Container logs (last 20 lines)
Write-Host "6. WEBAPP CONTAINER LOGS (Last 20 lines):" -ForegroundColor Cyan
try {
    docker compose -f docker-compose.min.yml logs --tail 20 webapp
} catch {
    Write-Host "Error reading logs: $_" -ForegroundColor Red
}
Write-Host ""

# 7. Local configuration files
Write-Host "7. LOCAL CONFIGURATION FILES:" -ForegroundColor Cyan
Write-Host "Local appsettings.json:"
if (Test-Path "src\Services\ProspectFinderPro.WebApp\appsettings.json") {
    Get-Content "src\Services\ProspectFinderPro.WebApp\appsettings.json"
} else {
    Write-Host "File not found!" -ForegroundColor Red
}
Write-Host ""

Write-Host "Docker compose configuration:"
if (Test-Path "docker-compose.min.yml") {
    Write-Host "WebApp section from docker-compose.min.yml:"
    $content = Get-Content "docker-compose.min.yml" -Raw
    if ($content -match "(?s)webapp:(.*?)(?=\s+\w+:|$)") {
        $matches[0]
    }
} else {
    Write-Host "docker-compose.min.yml not found!" -ForegroundColor Red
}
Write-Host ""

# 8. ASP.NET Core configuration precedence test
Write-Host "8. ASP.NET CORE CONFIGURATION TEST:" -ForegroundColor Cyan
Write-Host "Testing configuration precedence inside container..."
try {
    docker exec finprsql-webapp-1 printenv | grep -i api
    Write-Host ""
    Write-Host "Files in /app directory:"
    docker exec finprsql-webapp-1 ls -la /app/*.json
} catch {
    Write-Host "Error testing configuration: $_" -ForegroundColor Red
}
Write-Host ""

# 9. Browser vs Server context analysis
Write-Host "9. BROWSER VS SERVER CONTEXT ANALYSIS:" -ForegroundColor Cyan
Write-Host "The issue: Browser needs 'localhost:5000' but container shows 'api-gateway:8080'"
Write-Host ""
Write-Host "Expected behavior:"
Write-Host "- Browser HTTP requests must go to: http://localhost:5000 (host machine)"
Write-Host "- Container internal calls can use: http://api-gateway:8080 (Docker network)"
Write-Host ""
Write-Host "Current problem:"
Write-Host "- WebApp shows 'API Base: http://api-gateway:8080' regardless of configuration"
Write-Host "- This suggests ASP.NET Core configuration is overridden somewhere"
Write-Host ""

Write-Host "=== DIAGNOSTIC REPORT COMPLETE ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps for Claude Code analysis:" -ForegroundColor Yellow
Write-Host "1. Examine why appsettings.json is ignored"
Write-Host "2. Check if environment variables override configuration"
Write-Host "3. Verify Blazor Interactive rendering mode behavior"
Write-Host "4. Test if OperatingSystem.IsBrowser() works correctly"
Write-Host "5. Consider alternative configuration approaches"