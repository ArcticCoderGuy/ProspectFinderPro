using Microsoft.EntityFrameworkCore;
using ProspectFinderPro.DataIngestion.Models;
using ProspectFinderPro.Shared.Data;
using ProspectFinderPro.Shared.Models;

namespace ProspectFinderPro.DataIngestion.Services;

public class CompanyDataProcessor
{
    private readonly ProspectFinderDbContext _context;
    private readonly ILogger<CompanyDataProcessor> _logger;

    public CompanyDataProcessor(ProspectFinderDbContext context, ILogger<CompanyDataProcessor> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Company> ProcessCompanyDataAsync(CompanyRegistryResponse companyData, PrhRecord? prhData = null)
    {
        try
        {
            _logger.LogInformation("Processing company data for BusinessId: {BusinessId}", companyData.BusinessId);

            // Check if company already exists
            var existingCompany = await _context.Companies
                .FirstOrDefaultAsync(c => c.BusinessId == companyData.BusinessId);

            if (existingCompany != null)
            {
                // Update existing company
                await UpdateCompanyDataAsync(existingCompany, companyData, prhData);
                return existingCompany;
            }

            // Create new company
            var company = new Company
            {
                BusinessId = companyData.BusinessId,
                Name = CleanCompanyName(companyData.Name),
                Turnover = companyData.Turnover,
                Industry = DetermineIndustry(companyData.Industry, prhData?.BusinessLines),
                Location = $"{companyData.City}, {companyData.PostalCode}".Trim(' ', ','),
                EmployeeCount = companyData.EmployeeCount,
                PostalCode = companyData.PostalCode,
                City = companyData.City,
                Address = companyData.Address,
                Website = CleanWebsiteUrl(companyData.Website),
                Phone = CleanPhoneNumber(companyData.Phone),
                HasOwnProducts = false, // Will be determined by ML algorithm
                ProductConfidenceScore = null,
                LastVerified = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully processed company: {CompanyName} (ID: {BusinessId})", company.Name, company.BusinessId);
            return company;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing company data for BusinessId: {BusinessId}", companyData.BusinessId);
            throw;
        }
    }

    private async Task UpdateCompanyDataAsync(Company company, CompanyRegistryResponse companyData, PrhRecord? prhData)
    {
        company.Name = CleanCompanyName(companyData.Name);
        company.Turnover = companyData.Turnover;
        company.Industry = DetermineIndustry(companyData.Industry, prhData?.BusinessLines);
        company.Location = $"{companyData.City}, {companyData.PostalCode}".Trim(' ', ',');
        company.EmployeeCount = companyData.EmployeeCount;
        company.PostalCode = companyData.PostalCode;
        company.City = companyData.City;
        company.Address = companyData.Address;
        company.Website = CleanWebsiteUrl(companyData.Website);
        company.Phone = CleanPhoneNumber(companyData.Phone);
        company.LastVerified = DateTime.UtcNow;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<int> ProcessBatchCompaniesAsync(IEnumerable<CompanyRegistryResponse> companies)
    {
        var processedCount = 0;
        const int batchSize = 50;

        var batches = companies
            .Select((company, index) => new { Company = company, Index = index })
            .GroupBy(x => x.Index / batchSize)
            .Select(g => g.Select(x => x.Company));

        foreach (var batch in batches)
        {
            try
            {
                foreach (var companyData in batch)
                {
                    await ProcessCompanyDataAsync(companyData);
                    processedCount++;
                }

                _logger.LogInformation("Processed batch of {BatchSize} companies. Total processed: {ProcessedCount}", 
                    batch.Count(), processedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch of companies");
            }
        }

        return processedCount;
    }

    public decimal CalculateFinancialHealthScore(Company company, IEnumerable<FinancialData>? financialHistory = null)
    {
        var score = 0.5m; // Default neutral score

        try
        {
            // Turnover score (40% weight)
            if (company.Turnover.HasValue)
            {
                var turnoverScore = company.Turnover.Value switch
                {
                    >= 10_000_000 => 1.0m,
                    >= 5_000_000 => 0.8m,
                    >= 1_000_000 => 0.6m,
                    >= 500_000 => 0.4m,
                    _ => 0.2m
                };
                score += turnoverScore * 0.4m;
            }

            // Employee count score (20% weight)
            if (company.EmployeeCount.HasValue)
            {
                var employeeScore = company.EmployeeCount.Value switch
                {
                    >= 50 => 1.0m,
                    >= 20 => 0.8m,
                    >= 10 => 0.6m,
                    >= 5 => 0.4m,
                    _ => 0.2m
                };
                score += employeeScore * 0.2m;
            }

            // Financial trend score (30% weight)
            if (financialHistory?.Any() == true)
            {
                var orderedHistory = financialHistory.OrderBy(f => f.Year).ToList();
                if (orderedHistory.Count >= 2)
                {
                    var latest = orderedHistory.Last();
                    var previous = orderedHistory[^2];

                    if (latest.Revenue.HasValue && previous.Revenue.HasValue && previous.Revenue > 0)
                    {
                        var growthRate = (latest.Revenue.Value - previous.Revenue.Value) / previous.Revenue.Value;
                        var trendScore = growthRate switch
                        {
                            >= 0.2m => 1.0m,  // 20%+ growth
                            >= 0.1m => 0.8m,  // 10%+ growth
                            >= 0.05m => 0.6m, // 5%+ growth
                            >= 0 => 0.5m,     // Stable
                            >= -0.1m => 0.3m, // Minor decline
                            _ => 0.1m          // Major decline
                        };
                        score += trendScore * 0.3m;
                    }
                }
            }

            // Data completeness score (10% weight)
            var completenessScore = 0m;
            if (!string.IsNullOrEmpty(company.Website)) completenessScore += 0.25m;
            if (!string.IsNullOrEmpty(company.Phone)) completenessScore += 0.25m;
            if (!string.IsNullOrEmpty(company.Email)) completenessScore += 0.25m;
            if (!string.IsNullOrEmpty(company.Industry)) completenessScore += 0.25m;

            score += completenessScore * 0.1m;

            return Math.Max(0, Math.Min(1, score));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating financial health score for company: {CompanyId}", company.Id);
            return 0.5m; // Return neutral score on error
        }
    }

    private string CleanCompanyName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Remove common company suffixes for standardization
        var suffixes = new[] { " OY", " OYJ", " AB", " LTD", " LIMITED", " INC", " CORP", " LLC" };
        var cleanName = name.Trim().ToUpperInvariant();

        foreach (var suffix in suffixes)
        {
            if (cleanName.EndsWith(suffix))
            {
                cleanName = cleanName[..^suffix.Length].Trim();
                break;
            }
        }

        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cleanName.ToLowerInvariant());
    }

    private string? DetermineIndustry(string? avoinDataIndustry, IEnumerable<BusinessLine>? businessLines)
    {
        // Prefer more detailed industry information from PRH
        if (businessLines?.Any() == true)
        {
            var primaryBusinessLine = businessLines.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(primaryBusinessLine?.Name))
            {
                return primaryBusinessLine.Name;
            }
        }

        return avoinDataIndustry;
    }

    private string? CleanWebsiteUrl(string? website)
    {
        if (string.IsNullOrWhiteSpace(website))
            return null;

        var cleanUrl = website.Trim().ToLowerInvariant();
        
        if (!cleanUrl.StartsWith("http://") && !cleanUrl.StartsWith("https://"))
        {
            cleanUrl = "https://" + cleanUrl;
        }

        if (Uri.TryCreate(cleanUrl, UriKind.Absolute, out var uri))
        {
            return uri.ToString();
        }

        return null;
    }

    private string? CleanPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        // Remove common separators and spaces
        var cleanPhone = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

        // Validate Finnish phone number format
        if (cleanPhone.StartsWith("358") && cleanPhone.Length >= 10)
        {
            return "+" + cleanPhone;
        }
        else if (cleanPhone.StartsWith("0") && cleanPhone.Length >= 9)
        {
            return "+358" + cleanPhone[1..];
        }

        return cleanPhone.Length >= 7 ? cleanPhone : null;
    }
}