using System.Text.Json;
using ProspectFinderPro.DataIngestion.Models.ApiResponses;

namespace ProspectFinderPro.DataIngestion.Services.DataSourceClients;

public class VeroClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VeroClient> _logger;

    public VeroClient(HttpClient httpClient, ILogger<VeroClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<VeroExportRecord?> GetExportDataAsync(string businessId)
    {
        try
        {
            _logger.LogInformation("Fetching export data from Vero.fi for BusinessId: {BusinessId}", businessId);

            // This would typically require authentication and specific endpoints
            // For now, we'll simulate the structure
            var endpoint = $"api/export-statistics?business_id={businessId}";
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Vero.fi API request failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var exportData = JsonSerializer.Deserialize<VeroExportRecord>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return exportData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Vero.fi export data for BusinessId: {BusinessId}", businessId);
            return null;
        }
    }

    public async Task<IEnumerable<VeroExportRecord>> GetExportStatisticsByIndustryAsync(string industryCode)
    {
        try
        {
            _logger.LogInformation("Fetching export statistics by industry: {IndustryCode}", industryCode);

            var endpoint = $"api/export-statistics/industry?industry_code={industryCode}";
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Vero.fi industry export stats failed: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<VeroExportRecord>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var exportData = JsonSerializer.Deserialize<List<VeroExportRecord>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return exportData ?? Enumerable.Empty<VeroExportRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Vero.fi industry export statistics: {IndustryCode}", industryCode);
            return Enumerable.Empty<VeroExportRecord>();
        }
    }

    public async Task<VeroTaxRecord?> GetPublicTaxDataAsync(string businessId, int year)
    {
        try
        {
            _logger.LogInformation("Fetching public tax data for BusinessId: {BusinessId}, Year: {Year}", 
                businessId, year);

            var endpoint = $"api/public-tax-data?business_id={businessId}&year={year}";
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Vero.fi tax data request failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var taxData = JsonSerializer.Deserialize<VeroTaxRecord>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return taxData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Vero.fi tax data for BusinessId: {BusinessId}", businessId);
            return null;
        }
    }
}