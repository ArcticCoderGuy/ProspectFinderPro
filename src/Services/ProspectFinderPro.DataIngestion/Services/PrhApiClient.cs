using System.Text.Json;
using ProspectFinderPro.DataIngestion.Models;

namespace ProspectFinderPro.DataIngestion.Services;

public class PrhApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PrhApiClient> _logger;

    public PrhApiClient(HttpClient httpClient, ILogger<PrhApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PrhRecord?> GetCompanyDetailsAsync(string businessId)
    {
        try
        {
            _logger.LogInformation("Fetching PRH company details for BusinessId: {BusinessId}", businessId);

            // PRH API endpoint for company details
            var endpoint = $"datastore_search?resource_id=962db41c-57b6-4e5d-a68c-4e45002d7329&filters={{\"businessId\":\"{businessId}\"}}";
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PRH API request failed with status: {StatusCode}", response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<PrhApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Success == true && apiResponse.Result?.Records?.Any() == true)
            {
                return apiResponse.Result.Records.First();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching PRH company details for BusinessId: {BusinessId}", businessId);
            throw;
        }
    }

    public async Task<IEnumerable<PrhRecord>> SearchCompaniesByNameAsync(string companyName, int limit = 50)
    {
        try
        {
            _logger.LogInformation("Searching PRH companies by name: {CompanyName}", companyName);

            var filters = new { name = companyName };
            var filtersJson = JsonSerializer.Serialize(filters);
            var endpoint = $"datastore_search?resource_id=962db41c-57b6-4e5d-a68c-4e45002d7329&filters={Uri.EscapeDataString(filtersJson)}&limit={limit}";

            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PRH API search request failed with status: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<PrhRecord>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<PrhApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Success == true && apiResponse.Result?.Records?.Any() == true)
            {
                return apiResponse.Result.Records;
            }

            return Enumerable.Empty<PrhRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching PRH companies by name: {CompanyName}", companyName);
            throw;
        }
    }

    public async Task<IEnumerable<PrhRecord>> GetCompaniesAsync(int offset = 0, int limit = 100)
    {
        try
        {
            _logger.LogInformation("Fetching PRH companies with offset: {Offset}, limit: {Limit}", offset, limit);

            var endpoint = $"datastore_search?resource_id=962db41c-57b6-4e5d-a68c-4e45002d7329&offset={offset}&limit={limit}";
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PRH API companies request failed with status: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<PrhRecord>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<PrhApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Success == true && apiResponse.Result?.Records?.Any() == true)
            {
                return apiResponse.Result.Records;
            }

            return Enumerable.Empty<PrhRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching PRH companies with offset: {Offset}", offset);
            throw;
        }
    }
}