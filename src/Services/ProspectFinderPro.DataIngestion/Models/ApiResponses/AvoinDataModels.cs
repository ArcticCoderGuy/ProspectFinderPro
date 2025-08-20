using System.Text.Json.Serialization;

namespace ProspectFinderPro.DataIngestion.Models.ApiResponses;

public class AvoinDataApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("result")]
    public AvoinDataResult? Result { get; set; }
}

public class AvoinDataResult
{
    [JsonPropertyName("records")]
    public List<AvoinDataRecord>? Records { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class AvoinDataRecord
{
    [JsonPropertyName("business_id")]
    public string BusinessId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("turnover")]
    public decimal? Turnover { get; set; }

    [JsonPropertyName("profit")]
    public decimal? Profit { get; set; }

    [JsonPropertyName("assets")]
    public decimal? Assets { get; set; }

    [JsonPropertyName("liabilities")]
    public decimal? Liabilities { get; set; }

    [JsonPropertyName("employees")]
    public int? EmployeeCount { get; set; }

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("industry_code")]
    public string? IndustryCode { get; set; }

    [JsonPropertyName("industry_name")]
    public string? IndustryName { get; set; }

    [JsonPropertyName("municipality")]
    public string? Municipality { get; set; }

    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}