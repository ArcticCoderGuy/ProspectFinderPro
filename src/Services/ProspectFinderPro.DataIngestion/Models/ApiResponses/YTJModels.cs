using System.Text.Json.Serialization;

namespace ProspectFinderPro.DataIngestion.Models.ApiResponses;

public class YTJApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("result")]
    public YTJResult? Result { get; set; }
}

public class YTJResult
{
    [JsonPropertyName("records")]
    public List<YTJRecord>? Records { get; set; }
}

public class YTJRecord
{
    [JsonPropertyName("business_id")]
    public string BusinessId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("company_form")]
    public string? CompanyForm { get; set; }

    [JsonPropertyName("registration_date")]
    public DateTime? RegistrationDate { get; set; }

    [JsonPropertyName("business_lines")]
    public List<YTJBusinessLine>? BusinessLines { get; set; }

    [JsonPropertyName("addresses")]
    public List<YTJAddress>? Addresses { get; set; }

    [JsonPropertyName("contact_details")]
    public List<YTJContactDetail>? ContactDetails { get; set; }

    [JsonPropertyName("auxiliary_names")]
    public List<string>? AuxiliaryNames { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("liquidation_date")]
    public DateTime? LiquidationDate { get; set; }
}

public class YTJBusinessLine
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("registration_date")]
    public DateTime? RegistrationDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateTime? EndDate { get; set; }
}

public class YTJAddress
{
    [JsonPropertyName("care_of")]
    public string? CareOf { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("post_code")]
    public string? PostCode { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("registration_date")]
    public DateTime? RegistrationDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateTime? EndDate { get; set; }
}

public class YTJContactDetail
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("registration_date")]
    public DateTime? RegistrationDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateTime? EndDate { get; set; }
}