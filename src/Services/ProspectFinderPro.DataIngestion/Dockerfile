# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/Services/ProspectFinderPro.DataIngestion/ProspectFinderPro.DataIngestion.csproj", "src/Services/ProspectFinderPro.DataIngestion/"]
COPY ["src/Shared/ProspectFinderPro.Shared/ProspectFinderPro.Shared.csproj", "src/Shared/ProspectFinderPro.Shared/"]

RUN dotnet restore "src/Services/ProspectFinderPro.DataIngestion/ProspectFinderPro.DataIngestion.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/Services/ProspectFinderPro.DataIngestion"
RUN dotnet build "ProspectFinderPro.DataIngestion.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ProspectFinderPro.DataIngestion.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Create non-root user
RUN addgroup --system --gid 1001 appuser && \
    adduser --system --uid 1001 --gid 1001 appuser

USER appuser

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ProspectFinderPro.DataIngestion.dll"]

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1