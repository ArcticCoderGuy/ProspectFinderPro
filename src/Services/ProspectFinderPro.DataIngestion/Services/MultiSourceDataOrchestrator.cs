using Microsoft.EntityFrameworkCore;
using ProspectFinderPro.DataIngestion.Services.DataSourceClients;
using ProspectFinderPro.DataIngestion.Models.ApiResponses;
using ProspectFinderPro.Shared.Data;
using ProspectFinderPro.Shared.Models;

namespace ProspectFinderPro.DataIngestion.Services;

public class MultiSourceDataOrchestrator
{
    private readonly CompanyFactsClient _companyFactsClient;
    private readonly AvoinDataClient _avoinDataClient;
    private readonly YTJClient _ytjClient;
    private readonly VeroClient _veroClient;
    private readonly ProspectFinderDbContext _context;
    private readonly ILogger<MultiSourceDataOrchestrator> _logger;

    public MultiSourceDataOrchestrator(
        CompanyFactsClient companyFactsClient,
        AvoinDataClient avoinDataClient,
        YTJClient ytjClient,
        VeroClient veroClient,
        ProspectFinderDbContext context,
        ILogger<MultiSourceDataOrchestrator> logger)
    {
        _companyFactsClient = companyFactsClient;
        _avoinDataClient = avoinDataClient;
        _ytjClient = ytjClient;
        _veroClient = veroClient;
        _context = context;
        _logger = logger;
    }

    public async Task<Company> EnrichCompanyDataAsync(string businessId)
    {
        _logger.LogInformation("Starting comprehensive data enrichment for BusinessId: {BusinessId}", businessId);

        try
        {
            // Check if company exists
            var existingCompany = await _context.Companies
                .Include(c => c.FinancialHistory)
                .Include(c => c.ProductOwnershipAnalysis)
                .FirstOrDefaultAsync(c => c.BusinessId == businessId);

            var company = existingCompany ?? new Company { BusinessId = businessId };

            // Phase 1: Get basic company information from CompanyFacts.eu
            var companyFactsData = await _companyFactsClient.GetCompanyByBusinessIdAsync(businessId);
            if (companyFactsData != null)
            {
                await EnrichFromCompanyFactsAsync(company, companyFactsData);
                _logger.LogInformation("Enriched company {BusinessId} with CompanyFacts.eu data", businessId);
            }

            // Phase 2: Get detailed registry information from YTJ
            var ytjData = await _ytjClient.GetCompanyDetailsAsync(businessId);
            if (ytjData != null)
            {
                await EnrichFromYTJAsync(company, ytjData);
                _logger.LogInformation("Enriched company {BusinessId} with YTJ data", businessId);
            }

            // Phase 3: Get financial data from Avoindata.fi
            var financialData = await _avoinDataClient.GetCompanyFinancialDataAsync(businessId);
            if (financialData.Any())
            {
                await EnrichFinancialDataAsync(company, financialData);
                _logger.LogInformation("Enriched company {BusinessId} with financial data", businessId);
            }

            // Phase 4: Get export data from Vero.fi
            var exportData = await _veroClient.GetExportDataAsync(businessId);
            if (exportData != null)
            {
                await EnrichFromExportDataAsync(company, exportData);
                _logger.LogInformation("Enriched company {BusinessId} with export data", businessId);
            }

            // Phase 5: Calculate product ownership confidence score
            await CalculateProductOwnershipScore(company);

            // Save or update company
            if (existingCompany == null)
            {
                _context.Companies.Add(company);
            }
            else
            {
                company.UpdatedAt = DateTime.UtcNow;
            }

            company.LastVerified = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully enriched company: {CompanyName} (ID: {BusinessId}) with confidence score: {Score}", 
                company.Name, company.BusinessId, company.ProductConfidenceScore);

            return company;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enriching company data for BusinessId: {BusinessId}", businessId);
            throw;
        }
    }

    private async Task EnrichFromCompanyFactsAsync(Company company, CompanyFactsRecord companyFactsData)
    {
        company.Name = companyFactsData.CompanyName;

        if (companyFactsData.Addresses?.Any() == true)
        {
            var primaryAddress = companyFactsData.Addresses.First();
            company.Address = primaryAddress.Street;
            company.PostalCode = primaryAddress.PostalCode;
            company.City = primaryAddress.City;
            company.Location = $"{primaryAddress.City}, {primaryAddress.PostalCode}".Trim(' ', ',');
        }

        // Extract industry information from industry codes
        if (companyFactsData.IndustryCodes?.Any() == true)
        {
            // TODO: Map industry codes to readable names
            company.Industry = string.Join(", ", companyFactsData.IndustryCodes.Take(2));
        }
    }

    private async Task EnrichFromYTJAsync(Company company, YTJRecord ytjData)
    {
        // Update company name if not already set
        if (string.IsNullOrEmpty(company.Name))
        {
            company.Name = ytjData.Name;
        }

        // Set primary business line as industry
        if (ytjData.BusinessLines?.Any() == true)
        {
            var primaryBusinessLine = ytjData.BusinessLines
                .OrderBy(bl => bl.RegistrationDate)
                .FirstOrDefault(bl => bl.EndDate == null);
            
            if (primaryBusinessLine != null)
            {
                company.Industry = primaryBusinessLine.Name;
            }
        }

        // Extract contact information
        if (ytjData.ContactDetails?.Any() == true)
        {
            var phone = ytjData.ContactDetails.FirstOrDefault(cd => cd.Type.Contains("phone", StringComparison.OrdinalIgnoreCase));
            var website = ytjData.ContactDetails.FirstOrDefault(cd => cd.Type.Contains("website", StringComparison.OrdinalIgnoreCase));
            var email = ytjData.ContactDetails.FirstOrDefault(cd => cd.Type.Contains("email", StringComparison.OrdinalIgnoreCase));

            if (phone != null) company.Phone = phone.Value;
            if (website != null) company.Website = website.Value;
            if (email != null) company.Email = email.Value;
        }

        // Update address information
        if (ytjData.Addresses?.Any() == true)
        {
            var currentAddress = ytjData.Addresses
                .Where(a => a.EndDate == null)
                .OrderBy(a => a.Type)
                .FirstOrDefault();

            if (currentAddress != null && string.IsNullOrEmpty(company.Address))
            {
                company.Address = currentAddress.Street;
                company.PostalCode = currentAddress.PostCode;
                company.City = currentAddress.City;
                company.Location = $"{currentAddress.City}, {currentAddress.PostCode}".Trim(' ', ',');
            }
        }
    }

    private async Task EnrichFinancialDataAsync(Company company, IEnumerable<AvoinDataRecord> financialRecords)
    {
        foreach (var record in financialRecords.Where(r => r.Year.HasValue))
        {
            var existingFinancialData = await _context.FinancialData
                .FirstOrDefaultAsync(fd => fd.CompanyId == company.Id && fd.Year == record.Year!.Value);

            if (existingFinancialData == null)
            {
                var financialData = new FinancialData
                {
                    Company = company,
                    Year = record.Year!.Value,
                    Revenue = record.Turnover,
                    Profit = record.Profit,
                    Assets = record.Assets,
                    Liabilities = record.Liabilities,
                    Source = "AvoinData.fi",
                    CreatedAt = DateTime.UtcNow
                };

                company.FinancialHistory.Add(financialData);
            }
            else
            {
                // Update existing data
                existingFinancialData.Revenue = record.Turnover ?? existingFinancialData.Revenue;
                existingFinancialData.Profit = record.Profit ?? existingFinancialData.Profit;
                existingFinancialData.Assets = record.Assets ?? existingFinancialData.Assets;
                existingFinancialData.Liabilities = record.Liabilities ?? existingFinancialData.Liabilities;
                existingFinancialData.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Update company turnover with latest available data
        var latestFinancialData = financialRecords
            .Where(r => r.Year.HasValue && r.Turnover.HasValue)
            .OrderByDescending(r => r.Year)
            .FirstOrDefault();

        if (latestFinancialData != null)
        {
            company.Turnover = latestFinancialData.Turnover;
            company.EmployeeCount = latestFinancialData.EmployeeCount ?? company.EmployeeCount;
        }
    }

    private async Task EnrichFromExportDataAsync(Company company, VeroExportRecord exportData)
    {
        // Create or update products based on export data
        if (exportData.ExportProducts?.Any() == true)
        {
            foreach (var exportProduct in exportData.ExportProducts)
            {
                var existingProduct = company.Products
                    .FirstOrDefault(p => p.Name.Contains(exportProduct.ProductName, StringComparison.OrdinalIgnoreCase));

                if (existingProduct == null)
                {
                    var product = new Product
                    {
                        Company = company,
                        Name = exportProduct.ProductName,
                        Category = "Export Product",
                        ProductType = "Own Product",
                        IsMainProduct = exportProduct.ExportValueEur > (exportData.ExportValueEur * 0.3m), // >30% of total exports
                        ConfidenceScore = 0.9m, // High confidence for export products
                        Source = "Vero.fi Export Data",
                        CreatedAt = DateTime.UtcNow
                    };

                    company.Products.Add(product);
                }
            }
        }

        // Update company export information (we might need to add these fields to Company model)
        // For now, we'll use the export data to influence the product ownership score
    }

    private async Task CalculateProductOwnershipScore(Company company)
    {
        var analysis = company.ProductOwnershipAnalysis ?? new ProductOwnershipAnalysis { Company = company };

        // Industry Score (30% weight) - Based on NACE codes that typically indicate manufacturing
        analysis.IndustryScore = CalculateIndustryScore(company.Industry);

        // Export Score (25% weight) - Companies that export are more likely to have own products
        var exportData = await _veroClient.GetExportDataAsync(company.BusinessId);
        analysis.ExportScore = CalculateExportScore(exportData);

        // Company Size Score (20% weight) - Larger companies more likely to have own products
        analysis.CompanySizeScore = CalculateCompanySizeScore(company.Turnover, company.EmployeeCount);

        // Financial Health Score (15% weight) - Healthy companies more likely to invest in R&D
        analysis.WebsiteScore = CalculateFinancialHealthScore(company);

        // Patent Score (10% weight) - Would require patent database integration
        analysis.PatentScore = 0.5m; // Neutral for now

        // Calculate overall confidence score
        analysis.OverallConfidenceScore = 
            (analysis.IndustryScore * 0.30m) +
            (analysis.ExportScore * 0.25m) +
            (analysis.CompanySizeScore * 0.20m) +
            (analysis.WebsiteScore * 0.15m) +
            (analysis.PatentScore * 0.10m);

        // Update company flags
        company.HasOwnProducts = analysis.OverallConfidenceScore >= 0.6m;
        company.ProductConfidenceScore = analysis.OverallConfidenceScore;

        // Generate reasoning
        analysis.AnalysisReasoning = GenerateAnalysisReasoning(analysis);
        analysis.UpdatedAt = DateTime.UtcNow;

        if (company.ProductOwnershipAnalysis == null)
        {
            company.ProductOwnershipAnalysis = analysis;
        }
    }

    private decimal CalculateIndustryScore(string? industry)
    {
        if (string.IsNullOrEmpty(industry))
            return 0.5m;

        var manufacturingKeywords = new[]
        {
            "manufacturing", "production", "factory", "industrial", "machinery",
            "electronics", "automotive", "pharmaceutical", "chemical", "food processing",
            "textile", "metal", "plastic", "furniture", "equipment", "technology",
            "software development", "engineering", "biotechnology"
        };

        var industryLower = industry.ToLowerInvariant();
        var matchCount = manufacturingKeywords.Count(keyword => industryLower.Contains(keyword));

        return matchCount switch
        {
            >= 3 => 1.0m,
            2 => 0.8m,
            1 => 0.6m,
            _ => 0.3m
        };
    }

    private decimal CalculateExportScore(VeroExportRecord? exportData)
    {
        if (exportData == null || !exportData.ExportValueEur.HasValue || exportData.ExportValueEur <= 0)
            return 0.2m;

        // High export companies are more likely to have own products
        return exportData.ExportPercentageOfTurnover switch
        {
            >= 50m => 1.0m,
            >= 25m => 0.8m,
            >= 10m => 0.6m,
            >= 5m => 0.4m,
            _ => 0.3m
        };
    }

    private decimal CalculateCompanySizeScore(decimal? turnover, int? employees)
    {
        var turnoverScore = turnover switch
        {
            >= 50_000_000 => 1.0m,
            >= 20_000_000 => 0.9m,
            >= 10_000_000 => 0.8m,
            >= 5_000_000 => 0.6m,
            >= 2_000_000 => 0.4m,
            _ => 0.2m
        };

        var employeeScore = employees switch
        {
            >= 200 => 1.0m,
            >= 100 => 0.8m,
            >= 50 => 0.6m,
            >= 20 => 0.4m,
            >= 10 => 0.3m,
            _ => 0.2m
        };

        return (turnoverScore * 0.7m) + (employeeScore * 0.3m);
    }

    private decimal CalculateFinancialHealthScore(Company company)
    {
        if (!company.Turnover.HasValue)
            return 0.5m;

        var latestFinancial = company.FinancialHistory
            .OrderByDescending(fh => fh.Year)
            .FirstOrDefault();

        if (latestFinancial == null)
            return 0.5m;

        var profitMargin = latestFinancial.Profit / latestFinancial.Revenue;
        var debtRatio = latestFinancial.Liabilities / latestFinancial.Assets;

        var profitScore = profitMargin switch
        {
            >= 0.15m => 1.0m,
            >= 0.10m => 0.8m,
            >= 0.05m => 0.6m,
            >= 0.00m => 0.4m,
            _ => 0.2m
        };

        var debtScore = debtRatio switch
        {
            <= 0.3m => 1.0m,
            <= 0.5m => 0.8m,
            <= 0.7m => 0.6m,
            <= 0.9m => 0.4m,
            _ => 0.2m
        };

        return (profitScore * 0.6m) + (debtScore * 0.4m);
    }

    private string GenerateAnalysisReasoning(ProductOwnershipAnalysis analysis)
    {
        var reasons = new List<string>();

        if (analysis.IndustryScore >= 0.7m)
            reasons.Add("Industry profile suggests manufacturing/production activities");
        
        if (analysis.ExportScore >= 0.7m)
            reasons.Add("Significant export activity indicates own products");
        
        if (analysis.CompanySizeScore >= 0.7m)
            reasons.Add("Company size supports R&D and product development capabilities");
        
        if (analysis.WebsiteScore >= 0.7m)
            reasons.Add("Strong financial health enables product investment");

        if (analysis.OverallConfidenceScore >= 0.8m)
            reasons.Add("High confidence in product ownership");
        else if (analysis.OverallConfidenceScore >= 0.6m)
            reasons.Add("Moderate confidence in product ownership");
        else
            reasons.Add("Limited indicators of product ownership");

        return string.Join(". ", reasons) + ".";
    }
}