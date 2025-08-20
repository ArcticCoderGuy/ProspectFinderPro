using System.Text.Json;
using ProspectFinderPro.DataIngestion.Models.ApiResponses;

namespace ProspectFinderPro.DataIngestion.Services.DataSourceClients;

public class AvoinDataClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AvoinDataClient> _logger;

    public AvoinDataClient(HttpClient httpClient, ILogger<AvoinDataClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<AvoinDataRecord>> GetCompanyFinancialDataAsync(string businessId)
    {
        try
        {
            _logger.LogInformation("Fetching financial data from Avoindata.fi for BusinessId: {BusinessId}", businessId);

            // YTJ financial data endpoint
            var endpoint = $"datastore_search?resource_id=c5b7877f-d8f8-46e0-b3df-80cfc7ac5a13&filters={{\"business_id\":\"{businessId}\"}}";
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Avoindata.fi API request failed: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<AvoinDataRecord>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<AvoinDataApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return apiResponse?.Result?.Records ?? Enumerable.Empty<AvoinDataRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Avoindata.fi financial data for BusinessId: {BusinessId}", businessId);
            return Enumerable.Empty<AvoinDataRecord>();
        }
    }

    public async Task<IEnumerable<AvoinDataRecord>> SearchCompaniesByTurnoverAsync(decimal minTurnover, decimal maxTurnover, int limit = 100)
    {
        try
        {
            _logger.LogInformation("Searching Avoindata.fi companies with turnover {MinTurnover}-{MaxTurnover}", 
                minTurnover, maxTurnover);

            var filters = JsonSerializer.Serialize(new
            {
                turnover = new { gte = minTurnover, lte = maxTurnover }
            });

            var endpoint = $"datastore_search?resource_id=c5b7877f-d8f8-46e0-b3df-80cfc7ac5a13&filters={Uri.EscapeDataString(filters)}&limit={limit}";
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Avoindata.fi turnover search failed: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<AvoinDataRecord>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<AvoinDataApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return apiResponse?.Result?.Records ?? Enumerable.Empty<AvoinDataRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Avoindata.fi companies by turnover");
            return Enumerable.Empty<AvoinDataRecord>();
        }
    }

    public async Task<AvoinDataRecord?> GetCompanyDetailsAsync(string businessId)
    {
        var records = await GetCompanyFinancialDataAsync(businessId);
        return records.FirstOrDefault();
    }
}