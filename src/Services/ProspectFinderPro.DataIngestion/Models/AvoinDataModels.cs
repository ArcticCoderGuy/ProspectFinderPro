using System.Text.Json.Serialization;

namespace ProspectFinderPro.DataIngestion.Models;

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
    [JsonPropertyName("BusinessId")]
    public string BusinessId { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Industry")]
    public string? Industry { get; set; }

    [JsonPropertyName("Address")]
    public string? Address { get; set; }

    [JsonPropertyName("PostalCode")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("City")]
    public string? City { get; set; }

    [JsonPropertyName("Phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("Website")]
    public string? Website { get; set; }

    [JsonPropertyName("RegistrationDate")]
    public DateTime? RegistrationDate { get; set; }

    [JsonPropertyName("Turnover")]
    public decimal? Turnover { get; set; }

    [JsonPropertyName("EmployeeCount")]
    public int? EmployeeCount { get; set; }
}

public class CompanyRegistryResponse
{
    public string BusinessId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public decimal? Turnover { get; set; }
    public int? EmployeeCount { get; set; }
}

// PRH API Models
public class PrhApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("result")]
    public PrhResult? Result { get; set; }
}

public class PrhResult
{
    [JsonPropertyName("records")]
    public List<PrhRecord>? Records { get; set; }
}

public class PrhRecord
{
    [JsonPropertyName("businessId")]
    public string BusinessId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("companyForm")]
    public string? CompanyForm { get; set; }

    [JsonPropertyName("detailsUri")]
    public string? DetailsUri { get; set; }

    [JsonPropertyName("businessLines")]
    public List<BusinessLine>? BusinessLines { get; set; }

    [JsonPropertyName("addresses")]
    public List<CompanyAddress>? Addresses { get; set; }
}

public class BusinessLine
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string? Version { get; set; }
}

public class CompanyAddress
{
    [JsonPropertyName("careOf")]
    public string? CareOf { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("postCode")]
    public string? PostCode { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }
}