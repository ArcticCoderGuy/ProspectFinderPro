using System.Text.Json;
using ProspectFinderPro.DataIngestion.Models.ApiResponses;

namespace ProspectFinderPro.DataIngestion.Services.DataSourceClients;

public class YTJClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YTJClient> _logger;

    public YTJClient(HttpClient httpClient, ILogger<YTJClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<YTJRecord?> GetCompanyDetailsAsync(string businessId)
    {
        try
        {
            _logger.LogInformation("Fetching YTJ company details for BusinessId: {BusinessId}", businessId);

            // YTJ API endpoint
            var endpoint = $"datastore_search?resource_id=962db41c-57b6-4e5d-a68c-4e45002d7329&filters={{\"business_id\":\"{businessId}\"}}";
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("YTJ API request failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<YTJApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return apiResponse?.Result?.Records?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching YTJ company details for BusinessId: {BusinessId}", businessId);
            return null;
        }
    }

    public async Task<IEnumerable<YTJRecord>> SearchCompaniesByNameAsync(string companyName, int limit = 50)
    {
        try
        {
            _logger.LogInformation("Searching YTJ companies by name: {CompanyName}", companyName);

            var filters = JsonSerializer.Serialize(new { name = companyName });
            var endpoint = $"datastore_search?resource_id=962db41c-57b6-4e5d-a68c-4e45002d7329&filters={Uri.EscapeDataString(filters)}&limit={limit}";

            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("YTJ name search failed: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<YTJRecord>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<YTJApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return apiResponse?.Result?.Records ?? Enumerable.Empty<YTJRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching YTJ companies by name: {CompanyName}", companyName);
            return Enumerable.Empty<YTJRecord>();
        }
    }

    public async Task<IEnumerable<YTJRecord>> GetCompaniesByIndustryAsync(string industryCode, int limit = 100)
    {
        try
        {
            _logger.LogInformation("Fetching YTJ companies by industry code: {IndustryCode}", industryCode);

            var filters = JsonSerializer.Serialize(new { business_line_code = industryCode });
            var endpoint = $"datastore_search?resource_id=962db41c-57b6-4e5d-a68c-4e45002d7329&filters={Uri.EscapeDataString(filters)}&limit={limit}";

            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("YTJ industry search failed: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<YTJRecord>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<YTJApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return apiResponse?.Result?.Records ?? Enumerable.Empty<YTJRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching YTJ companies by industry: {IndustryCode}", industryCode);
            return Enumerable.Empty<YTJRecord>();
        }
    }
}