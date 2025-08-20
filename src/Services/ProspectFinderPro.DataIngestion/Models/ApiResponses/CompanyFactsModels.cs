using System.Text.Json.Serialization;

namespace ProspectFinderPro.DataIngestion.Models.ApiResponses;

public class CompanyFactsRecord
{
    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [JsonPropertyName("business_id")]
    public string BusinessId { get; set; } = string.Empty;

    [JsonPropertyName("trade_register")]
    public string TradeRegister { get; set; } = string.Empty;

    [JsonPropertyName("auxiliary_names")]
    public List<string>? AuxiliaryNames { get; set; }

    [JsonPropertyName("parallel_names")]
    public List<string>? ParallelNames { get; set; }

    [JsonPropertyName("registration_date")]
    public DateTime? RegistrationDate { get; set; }

    [JsonPropertyName("company_form")]
    public string? CompanyForm { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("industry_codes")]
    public List<string>? IndustryCodes { get; set; }

    [JsonPropertyName("addresses")]
    public List<CompanyFactsAddress>? Addresses { get; set; }
}

public class CompanyFactsAddress
{
    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}