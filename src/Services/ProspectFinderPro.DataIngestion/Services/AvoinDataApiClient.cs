using System.Text.Json;
using ProspectFinderPro.DataIngestion.Models;

namespace ProspectFinderPro.DataIngestion.Services;

public class AvoinDataApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AvoinDataApiClient> _logger;

    public AvoinDataApiClient(HttpClient httpClient, ILogger<AvoinDataApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CompanyRegistryResponse?> GetCompanyDataAsync(string businessId)
    {
        try
        {
            _logger.LogInformation("Fetching company data for BusinessId: {BusinessId}", businessId);

            var endpoint = $"datastore_search?resource_id=c5b7877f-d8f8-46e0-b3df-80cfc7ac5a13&filters={{\"BusinessId\":\"{businessId}\"}}";
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API request failed with status: {StatusCode}", response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<AvoinDataApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Success == true && apiResponse.Result?.Records?.Any() == true)
            {
                var record = apiResponse.Result.Records.First();
                return new CompanyRegistryResponse
                {
                    BusinessId = record.BusinessId,
                    Name = record.Name,
                    Industry = record.Industry,
                    Address = record.Address,
                    PostalCode = record.PostalCode,
                    City = record.City,
                    Phone = record.Phone,
                    Website = record.Website,
                    RegistrationDate = record.RegistrationDate
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching company data for BusinessId: {BusinessId}", businessId);
            throw;
        }
    }

    public async Task<IEnumerable<CompanyRegistryResponse>> SearchCompaniesByTurnoverAsync(decimal minTurnover, decimal maxTurnover, int limit = 100)
    {
        try
        {
            _logger.LogInformation("Searching companies with turnover between {MinTurnover} and {MaxTurnover}", minTurnover, maxTurnover);

            var filters = new
            {
                Turnover = new { gte = minTurnover, lte = maxTurnover }
            };

            var filtersJson = JsonSerializer.Serialize(filters);
            var endpoint = $"datastore_search?resource_id=c5b7877f-d8f8-46e0-b3df-80cfc7ac5a13&filters={Uri.EscapeDataString(filtersJson)}&limit={limit}";

            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API request failed with status: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<CompanyRegistryResponse>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<AvoinDataApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Success == true && apiResponse.Result?.Records?.Any() == true)
            {
                return apiResponse.Result.Records.Select(record => new CompanyRegistryResponse
                {
                    BusinessId = record.BusinessId,
                    Name = record.Name,
                    Industry = record.Industry,
                    Address = record.Address,
                    PostalCode = record.PostalCode,
                    City = record.City,
                    Phone = record.Phone,
                    Website = record.Website,
                    RegistrationDate = record.RegistrationDate,
                    Turnover = record.Turnover
                });
            }

            return Enumerable.Empty<CompanyRegistryResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching companies by turnover");
            throw;
        }
    }

    public async Task<IEnumerable<CompanyRegistryResponse>> GetCompaniesByIndustryAsync(string industry, int limit = 100)
    {
        try
        {
            _logger.LogInformation("Fetching companies in industry: {Industry}", industry);

            var filters = new { Industry = industry };
            var filtersJson = JsonSerializer.Serialize(filters);
            var endpoint = $"datastore_search?resource_id=c5b7877f-d8f8-46e0-b3df-80cfc7ac5a13&filters={Uri.EscapeDataString(filtersJson)}&limit={limit}";

            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API request failed with status: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<CompanyRegistryResponse>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<AvoinDataApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Success == true && apiResponse.Result?.Records?.Any() == true)
            {
                return apiResponse.Result.Records.Select(record => new CompanyRegistryResponse
                {
                    BusinessId = record.BusinessId,
                    Name = record.Name,
                    Industry = record.Industry,
                    Address = record.Address,
                    PostalCode = record.PostalCode,
                    City = record.City,
                    Phone = record.Phone,
                    Website = record.Website,
                    RegistrationDate = record.RegistrationDate,
                    Turnover = record.Turnover
                });
            }

            return Enumerable.Empty<CompanyRegistryResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies by industry: {Industry}", industry);
            throw;
        }
    }
}