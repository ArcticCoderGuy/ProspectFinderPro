using ProspectFinderPro.DataIngestion.Services;

namespace ProspectFinderPro.DataIngestion.Services;

public class DataIngestionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataIngestionBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    public DataIngestionBackgroundService(
        IServiceProvider serviceProvider, 
        ILogger<DataIngestionBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Ingestion Background Service started");

        // Wait for initial startup
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDataIngestionAsync(stoppingToken);

                // Run every 6 hours
                var delay = _configuration.GetValue<int>("DataIngestion:IntervalHours", 6);
                await Task.Delay(TimeSpan.FromHours(delay), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Data Ingestion Background Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Data Ingestion Background Service");
                
                // Wait before retrying on error
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }

    private async Task ProcessDataIngestionAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var avoinDataClient = scope.ServiceProvider.GetRequiredService<AvoinDataApiClient>();
        var prhClient = scope.ServiceProvider.GetRequiredService<PrhApiClient>();
        var dataProcessor = scope.ServiceProvider.GetRequiredService<CompanyDataProcessor>();

        _logger.LogInformation("Starting data ingestion process");

        try
        {
            // Phase 1: Fetch companies with turnover between €5-10M
            var targetCompanies = await avoinDataClient.SearchCompaniesByTurnoverAsync(5_000_000, 10_000_000, 500);
            
            _logger.LogInformation("Found {CompanyCount} companies in target turnover range", targetCompanies.Count());

            var processedCount = 0;
            
            foreach (var company in targetCompanies.Take(100)) // Process in batches
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    // Get additional details from PRH
                    var prhData = await prhClient.GetCompanyDetailsAsync(company.BusinessId);
                    
                    // Process and save company data
                    await dataProcessor.ProcessCompanyDataAsync(company, prhData);
                    
                    processedCount++;

                    // Rate limiting
                    await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing company {BusinessId}: {CompanyName}", 
                        company.BusinessId, company.Name);
                }
            }

            _logger.LogInformation("Data ingestion completed. Processed {ProcessedCount} companies", processedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data ingestion process");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Data Ingestion Background Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}