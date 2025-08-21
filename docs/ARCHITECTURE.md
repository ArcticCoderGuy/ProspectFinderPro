# Architecture Overview

## System Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Web Browser   │────│   Web App        │────│  API Gateway    │
│                 │    │  (Blazor Server) │    │  (ASP.NET Core) │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                                         │
                       ┌──────────────────┐             │
                       │ Business Intel   │─────────────┤
                       │ Service          │             │
                       └──────────────────┘             │
                                                         │
                       ┌──────────────────┐             │
                       │ Data Ingestion   │─────────────┤
                       │ Service          │             │
                       └──────────────────┘             │
                                                         │
                       ┌──────────────────┐             │
                       │ Notification     │─────────────┤
                       │ Service          │             │
                       └──────────────────┘             │
                                                         │
                       ┌──────────────────┐    ┌─────────────────┐
                       │   SQL Server     │────│     Redis       │
                       │   Database       │    │     Cache       │
                       └──────────────────┘    └─────────────────┘
```

## Services Description

### 1. API Gateway (`ProspectFinderPro.ApiGateway`)
- External API for clients
- Request routing and aggregation
- Authentication and authorization
- Rate limiting
- Health checks

### 2. Web App (`ProspectFinderPro.WebApp`) 
- Blazor Server application
- User interface for searching companies
- Dashboard and analytics
- SignalR for real-time updates

### 3. Data Ingestion Service (`ProspectFinderPro.DataIngestion`)
- Multi-source data collection
- Data processing and normalization
- Background sync jobs
- Data quality validation

### 4. Business Intelligence Service (`ProspectFinderPro.BusinessIntelligence`)
- AI-powered company classification
- Product ownership analysis
- Financial health scoring
- Market insights

### 5. Notification Service (`ProspectFinderPro.Notifications`)
- Email notifications
- Webhook integrations  
- Alert management
- Event streaming

## Technology Stack

- **Backend**: ASP.NET Core 8.0
- **Frontend**: Blazor Server
- **Database**: SQL Server with Entity Framework Core
- **Cache**: Redis
- **Containerization**: Docker & Docker Compose
- **Communication**: HTTP/REST, SignalR

## Data Sources

- CompanyFacts.eu - Nordic company registry
- Avoindata.fi - Finnish open data portal
- YTJ - Finnish Business Information System
- Vero.fi - Tax administration data