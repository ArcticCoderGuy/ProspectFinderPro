using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProspectFinderPro.DataIngestion.Services;
using ProspectFinderPro.DataIngestion.Services.DataSourceClients;
using ProspectFinderPro.Shared.Data;
using ProspectFinderPro.Shared.Models;

namespace ProspectFinderPro.DataIngestion.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ProspectFinderDbContext _context;
    private readonly MultiSourceDataOrchestrator _orchestrator;
    private readonly CompanyFactsClient _companyFactsClient;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(
        ProspectFinderDbContext context,
        MultiSourceDataOrchestrator orchestrator,
        CompanyFactsClient companyFactsClient,
        ILogger<CompaniesController> logger)
    {
        _context = context;
        _orchestrator = orchestrator;
        _companyFactsClient = companyFactsClient;
        _logger = logger;
    }

    /// <summary>
    /// Search companies with filters
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<CompanySearchResponse>> SearchCompanies(
        [FromQuery] decimal? minTurnover = null,
        [FromQuery] decimal? maxTurnover = null,
        [FromQuery] bool? hasOwnProducts = null,
        [FromQuery] string? industry = null,
        [FromQuery] string? location = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.Companies.AsQueryable();

            // Apply filters
            if (minTurnover.HasValue)
                query = query.Where(c => c.Turnover >= minTurnover.Value);

            if (maxTurnover.HasValue)
                query = query.Where(c => c.Turnover <= maxTurnover.Value);

            if (hasOwnProducts.HasValue)
                query = query.Where(c => c.HasOwnProducts == hasOwnProducts.Value);

            if (!string.IsNullOrEmpty(industry))
                query = query.Where(c => c.Industry != null && c.Industry.Contains(industry));

            if (!string.IsNullOrEmpty(location))
                query = query.Where(c => c.Location != null && c.Location.Contains(location));

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination and get results
            var companies = await query
                .OrderByDescending(c => c.ProductConfidenceScore)
                .ThenByDescending(c => c.Turnover)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.Products.Take(3))
                .Select(c => new CompanySearchResult
                {
                    BusinessId = c.BusinessId,
                    Name = c.Name,
                    Industry = c.Industry,
                    Turnover = c.Turnover,
                    Location = c.Location,
                    EmployeeCount = c.EmployeeCount,
                    HasOwnProducts = c.HasOwnProducts,
                    ProductConfidenceScore = c.ProductConfidenceScore,
                    Website = c.Website,
                    Phone = c.Phone,
                    MainProducts = c.Products.Where(p => p.IsMainProduct).Take(2).Select(p => p.Name).ToList(),
                    LastVerified = c.LastVerified
                })
                .ToListAsync();

            var response = new CompanySearchResponse
            {
                Companies = companies,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching companies");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get company details by business ID
    /// </summary>
    [HttpGet("{businessId}")]
    public async Task<ActionResult<CompanyDetailResponse>> GetCompanyDetails(string businessId)
    {
        try
        {
            var company = await _context.Companies
                .Include(c => c.Products)
                .Include(c => c.FinancialHistory.OrderByDescending(fh => fh.Year).Take(5))
                .Include(c => c.Contacts.Where(co => co.IsDecisionMaker))
                .Include(c => c.ProductOwnershipAnalysis)
                .FirstOrDefaultAsync(c => c.BusinessId == businessId);

            if (company == null)
            {
                return NotFound($"Company with BusinessId {businessId} not found");
            }

            var response = new CompanyDetailResponse
            {
                BusinessId = company.BusinessId,
                Name = company.Name,
                Industry = company.Industry,
                Turnover = company.Turnover,
                Location = company.Location,
                Address = company.Address,
                PostalCode = company.PostalCode,
                City = company.City,
                EmployeeCount = company.EmployeeCount,
                Website = company.Website,
                Phone = company.Phone,
                Email = company.Email,
                HasOwnProducts = company.HasOwnProducts,
                ProductConfidenceScore = company.ProductConfidenceScore,
                LastVerified = company.LastVerified,
                Products = company.Products.Select(p => new ProductInfo
                {
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    IsMainProduct = p.IsMainProduct,
                    ConfidenceScore = p.ConfidenceScore
                }).ToList(),
                FinancialHistory = company.FinancialHistory.Select(fh => new FinancialInfo
                {
                    Year = fh.Year,
                    Revenue = fh.Revenue,
                    Profit = fh.Profit,
                    FinancialHealthScore = fh.FinancialHealthScore
                }).ToList(),
                KeyContacts = company.Contacts.Select(c => new ContactInfo
                {
                    Name = $"{c.FirstName} {c.LastName}",
                    Position = c.Position,
                    Email = c.Email,
                    Phone = c.Phone
                }).ToList(),
                Analysis = company.ProductOwnershipAnalysis != null ? new OwnershipAnalysisInfo
                {
                    OverallConfidenceScore = company.ProductOwnershipAnalysis.OverallConfidenceScore,
                    IndustryScore = company.ProductOwnershipAnalysis.IndustryScore,
                    ExportScore = company.ProductOwnershipAnalysis.ExportScore,
                    CompanySizeScore = company.ProductOwnershipAnalysis.CompanySizeScore,
                    AnalysisReasoning = company.ProductOwnershipAnalysis.AnalysisReasoning,
                    AnalysisDate = company.ProductOwnershipAnalysis.AnalysisDate
                } : null
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company details for BusinessId: {BusinessId}", businessId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Enrich company data from all sources
    /// </summary>
    [HttpPost("{businessId}/enrich")]
    public async Task<ActionResult<CompanyDetailResponse>> EnrichCompanyData(string businessId)
    {
        try
        {
            _logger.LogInformation("Starting manual enrichment for BusinessId: {BusinessId}", businessId);

            var enrichedCompany = await _orchestrator.EnrichCompanyDataAsync(businessId);

            // Return the enriched company details
            return await GetCompanyDetails(businessId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enriching company data for BusinessId: {BusinessId}", businessId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search external companies (CompanyFacts.eu)
    /// </summary>
    [HttpGet("external/search")]
    public async Task<ActionResult> SearchExternalCompanies([FromQuery] string query, [FromQuery] int limit = 10)
    {
        try
        {
            var companies = await _companyFactsClient.SearchCompaniesAsync(query, "FI", limit);
            
            var results = companies.Select(c => new
            {
                businessId = c.BusinessId,
                name = c.CompanyName,
                tradeRegister = c.TradeRegister,
                addresses = c.Addresses?.Select(a => new
                {
                    street = a.Street,
                    postalCode = a.PostalCode,
                    city = a.City
                })
            });

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching external companies");
            return StatusCode(500, "Internal server error");
        }
    }
}

// Response models
public class CompanySearchResponse
{
    public List<CompanySearchResult> Companies { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class CompanySearchResult
{
    public string BusinessId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public decimal? Turnover { get; set; }
    public string? Location { get; set; }
    public int? EmployeeCount { get; set; }
    public bool HasOwnProducts { get; set; }
    public decimal? ProductConfidenceScore { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public List<string> MainProducts { get; set; } = new();
    public DateTime? LastVerified { get; set; }
}

public class CompanyDetailResponse
{
    public string BusinessId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public decimal? Turnover { get; set; }
    public string? Location { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public int? EmployeeCount { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool HasOwnProducts { get; set; }
    public decimal? ProductConfidenceScore { get; set; }
    public DateTime? LastVerified { get; set; }
    public List<ProductInfo> Products { get; set; } = new();
    public List<FinancialInfo> FinancialHistory { get; set; } = new();
    public List<ContactInfo> KeyContacts { get; set; } = new();
    public OwnershipAnalysisInfo? Analysis { get; set; }
}

public class ProductInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsMainProduct { get; set; }
    public decimal? ConfidenceScore { get; set; }
}

public class FinancialInfo
{
    public int Year { get; set; }
    public decimal? Revenue { get; set; }
    public decimal? Profit { get; set; }
    public decimal? FinancialHealthScore { get; set; }
}

public class ContactInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class OwnershipAnalysisInfo
{
    public decimal OverallConfidenceScore { get; set; }
    public decimal IndustryScore { get; set; }
    public decimal ExportScore { get; set; }
    public decimal CompanySizeScore { get; set; }
    public string? AnalysisReasoning { get; set; }
    public DateTime AnalysisDate { get; set; }
}