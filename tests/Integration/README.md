# Integration Tests

This directory contains integration and end-to-end tests.

## Structure
- `ProspectFinderPro.Integration.Tests/` - Service integration tests
- `ProspectFinderPro.E2E.Tests/` - Full system end-to-end tests

## Running Tests
```bash
# From project root
dotnet test tests/Integration

# E2E tests (requires running services)
scripts/deploy.ps1 -Environment development -Build
dotnet test tests/Integration/ProspectFinderPro.E2E.Tests
```