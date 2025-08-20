using System.Text.Json.Serialization;

namespace ProspectFinderPro.DataIngestion.Models.ApiResponses;

public class VeroExportRecord
{
    [JsonPropertyName("business_id")]
    public string BusinessId { get; set; } = string.Empty;

    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [JsonPropertyName("export_value_eur")]
    public decimal? ExportValueEur { get; set; }

    [JsonPropertyName("export_countries")]
    public List<string>? ExportCountries { get; set; }

    [JsonPropertyName("export_products")]
    public List<VeroExportProduct>? ExportProducts { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("industry_code")]
    public string? IndustryCode { get; set; }

    [JsonPropertyName("has_own_products")]
    public bool? HasOwnProducts { get; set; }

    [JsonPropertyName("export_percentage_of_turnover")]
    public decimal? ExportPercentageOfTurnover { get; set; }
}

public class VeroExportProduct
{
    [JsonPropertyName("product_code")]
    public string ProductCode { get; set; } = string.Empty;

    [JsonPropertyName("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("export_value_eur")]
    public decimal? ExportValueEur { get; set; }

    [JsonPropertyName("destination_countries")]
    public List<string>? DestinationCountries { get; set; }
}

public class VeroTaxRecord
{
    [JsonPropertyName("business_id")]
    public string BusinessId { get; set; } = string.Empty;

    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [JsonPropertyName("tax_year")]
    public int TaxYear { get; set; }

    [JsonPropertyName("taxable_income")]
    public decimal? TaxableIncome { get; set; }

    [JsonPropertyName("corporate_tax")]
    public decimal? CorporateTax { get; set; }

    [JsonPropertyName("vat_turnover")]
    public decimal? VatTurnover { get; set; }

    [JsonPropertyName("vat_paid")]
    public decimal? VatPaid { get; set; }

    [JsonPropertyName("employees_tax_data")]
    public VeroEmployeeTaxData? EmployeesTaxData { get; set; }
}

public class VeroEmployeeTaxData
{
    [JsonPropertyName("total_employees")]
    public int? TotalEmployees { get; set; }

    [JsonPropertyName("total_salary_payments")]
    public decimal? TotalSalaryPayments { get; set; }

    [JsonPropertyName("average_salary")]
    public decimal? AverageSalary { get; set; }
}