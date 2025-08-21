namespace ProspectFinderPro.WebApp.Models;

/// <summary>
/// Extended company detail model with additional information for dashboard view
/// </summary>
public record CompanyDetailModel(
    string BusinessId,
    string Name,
    decimal Turnover,
    string Industry,
    bool HasOwnProducts,
    double ProductConfidenceScore,
    int EmployeeCount,
    string Location,
    string DataSource,
    string Country,
    string? AdditionalInfo,
    string? PhoneNumber,
    string? Email,
    string? Website,
    string? Address,
    string? PostalCode,
    string? City,
    double? Latitude,
    double? Longitude
);