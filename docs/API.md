# ProspectFinderPro API Documentation

## Endpoints

### Health Check
- **GET** `/health` - System health status

### Companies
- **GET** `/api/companies/search` - Search companies with filters
  - Parameters:
    - `minTurnover` (optional): Minimum turnover amount
    - `maxTurnover` (optional): Maximum turnover amount  
    - `hasOwnProducts` (optional): Filter by product ownership
    - `page` (optional): Page number (default: 1)
    - `pageSize` (optional): Items per page (default: 10)

- **GET** `/api/companies/{businessId}` - Get company details
- **POST** `/api/seed-demo` - Load demo data (development only)

## Response Examples

### Company Search Response
```json
{
  "items": [
    {
      "businessId": "1234567-8",
      "name": "Example Manufacturing Oy",
      "turnover": 8500000,
      "industry": "Industrial Machinery Manufacturing",
      "hasOwnProducts": true,
      "location": "Helsinki, 00100",
      "employeeCount": 45
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 10
}
```

## Authentication

Currently in development mode - no authentication required.
Production will use API keys.