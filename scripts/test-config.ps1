# Test script to verify Docker configuration
Write-Host "=== Docker Configuration Test ==="

Write-Host "`n1. Testing API Gateway accessibility:"
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/companies/search?page=1&pageSize=1" -Method GET -TimeoutSec 10
    Write-Host "✅ API Gateway accessible at localhost:5000" -ForegroundColor Green
    Write-Host "   Status: $($response.StatusCode)"
} catch {
    Write-Host "❌ API Gateway not accessible at localhost:5000" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)"
}

Write-Host "`n2. Checking Docker containers:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | Where-Object { $_ -match "prospectfinder" }

Write-Host "`n3. Testing WebApp accessibility:"
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5001" -Method GET -TimeoutSec 10
    Write-Host "✅ WebApp accessible at localhost:5001" -ForegroundColor Green
} catch {
    Write-Host "❌ WebApp not accessible at localhost:5001" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)"
}

Write-Host "`n4. Environment Variables Check:"
Write-Host "Expected configuration:"
Write-Host "  ApiBaseUrlBrowser=http://localhost:5000"
Write-Host "  ApiBaseUrlServer=http://api-gateway:8080"

Write-Host "`n5. Next Steps:"
Write-Host "  1. Run: docker-compose down"
Write-Host "  2. Run: docker-compose up --build"
Write-Host "  3. Navigate to http://localhost:5001/search"
Write-Host "  4. Check that 'API Base:' shows 'http://localhost:5000'"