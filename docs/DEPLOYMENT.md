# Deployment Guide

## Development Environment

### Prerequisites
- .NET 8.0 SDK
- Docker Desktop
- SQL Server (or use Docker container)

### Quick Start with Docker Compose
```bash
# Navigate to docker directory
cd infrastructure/docker

# Start all services
docker compose -f docker-compose.yml up -d

# Check service health
docker compose logs api-gateway
docker compose logs webapp

# Access services
# API Gateway: http://localhost:5000
# Web App: http://localhost:5001
# Swagger: http://localhost:5000/swagger
```

### Manual Setup
```bash
# Restore dependencies
dotnet restore

# Database migrations
cd src/Services/ProspectFinderPro.ApiGateway
dotnet ef database update

# Start services individually
dotnet run --project src/Services/ProspectFinderPro.ApiGateway
dotnet run --project src/Services/ProspectFinderPro.WebApp
```

## Production Deployment

### Railway.app
Files: `infrastructure/docker/railway.toml`

### Render.com  
Files: `infrastructure/docker/render.yaml`

### Configuration
Update connection strings and API endpoints in:
- `appsettings.Production.json`
- `docker-compose.production.yml`

## Environment Variables
- `ConnectionStrings__DefaultConnection`: SQL Server connection
- `Redis__ConnectionString`: Redis connection
- `ApiBaseUrl`: API Gateway URL for WebApp