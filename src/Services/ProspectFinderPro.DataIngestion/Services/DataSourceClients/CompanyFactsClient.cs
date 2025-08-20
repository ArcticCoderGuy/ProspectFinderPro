using System.Text.Json;
using ProspectFinderPro.DataIngestion.Models.ApiResponses;

namespace ProspectFinderPro.DataIngestion.Services.DataSourceClients;

public class CompanyFactsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CompanyFactsClient> _logger;

    public CompanyFactsClient(HttpClient httpClient, ILogger<CompanyFactsClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<CompanyFactsRecord>> SearchCompaniesAsync(string query, string? country = "FI", int limit = 20)
    {
        try
        {
            _logger.LogInformation("Searching CompanyFacts for: {Query} in {Country}", query, country);

            var requestUrl = $"/api-proxy/search?query={Uri.EscapeDataString(query)}&limit={limit}";
            if (!string.IsNullOrEmpty(country))
            {
                requestUrl += $"&trade_register={country}";
            }

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CompanyFacts API request failed: {StatusCode} - {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                return Enumerable.Empty<CompanyFactsRecord>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var records = JsonSerializer.Deserialize<List<CompanyFactsRecord>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return records ?? Enumerable.Empty<CompanyFactsRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching CompanyFacts for query: {Query}", query);
            return Enumerable.Empty<CompanyFactsRecord>();
        }
    }

    public async Task<CompanyFactsRecord?> GetCompanyByBusinessIdAsync(string businessId)
    {
        try
        {
            _logger.LogInformation("Getting CompanyFacts data for BusinessId: {BusinessId}", businessId);

            var companies = await SearchCompaniesAsync(businessId, "FI", 1);
            return companies.FirstOrDefault(c => c.BusinessId == businessId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CompanyFacts data for BusinessId: {BusinessId}", businessId);
            return null;
        }
    }

    public async Task<IEnumerable<CompanyFactsRecord>> GetFinlandCompaniesAsync(int offset = 0, int limit = 100)
    {
        try
        {
            _logger.LogInformation("Getting Finnish companies from CompanyFacts (offset: {Offset}, limit: {Limit})", 
                offset, limit);

            // Use wildcard search for Finnish companies
            return await SearchCompaniesAsync("*", "FI", limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Finnish companies from CompanyFacts");
            return Enumerable.Empty<CompanyFactsRecord>();
        }
    }
}