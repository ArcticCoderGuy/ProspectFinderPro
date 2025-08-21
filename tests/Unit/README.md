# Unit Tests

This directory contains unit tests for individual services and components.

## Structure
- `ProspectFinderPro.ApiGateway.Tests/` - API Gateway unit tests
- `ProspectFinderPro.DataIngestion.Tests/` - Data Ingestion service tests  
- `ProspectFinderPro.Shared.Tests/` - Shared components tests

## Running Tests
```bash
# From project root
dotnet test tests/Unit

# Specific project
dotnet test tests/Unit/ProspectFinderPro.ApiGateway.Tests

# With coverage
dotnet test tests/Unit --collect:"XPlat Code Coverage"
```