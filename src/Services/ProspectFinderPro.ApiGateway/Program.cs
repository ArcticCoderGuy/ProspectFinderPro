using System.Linq;
using Microsoft.EntityFrameworkCore;
using ProspectFinderPro.ApiGateway.Data;
using ProspectFinderPro.ApiGateway.Models;
using ProspectFinderPro.ApiGateway.Services;

var builder = WebApplication.CreateBuilder(args);

// Bindaa varmasti porttiin 8080 kontissa
builder.WebHost.UseUrls("http://0.0.0.0:8080");

// Yhteys merkkijono: compose-ympäristöstä tai fallback
var cs = builder.Configuration.GetConnectionString("DefaultConnection")
         ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
         ?? "Server=sqlserver,1433;Database=ProspectFinderPro;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true";

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(cs));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HTTP client for external API calls
builder.Services.AddHttpClient();

// Register data services for real company data
builder.Services.AddScoped<YTJDataService>();
builder.Services.AddScoped<CompanyFactsService>();
builder.Services.AddScoped<StatisticsFinlandService>();
builder.Services.AddScoped<AvoinDataService>();
builder.Services.AddScoped<UnifiedDataService>();

const string CorsPolicy = "pfp";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

var app = builder.Build();
app.UseCors(CorsPolicy);
app.UseSwagger();
app.UseSwaggerUI();

// Migraatio (idempotentti)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Real company search using UnifiedDataService
app.MapGet("/api/companies/search-real",
    async (UnifiedDataService unifiedService, string dataSource = "avoindata", 
           decimal? minTurnover = 1000000, decimal? maxTurnover = 50000000, 
           bool? hasOwnProducts = null, string? sortBy = "turnover", 
           bool sortDesc = true, int page = 1, int pageSize = 20) =>
{
    try 
    {
        // Get real companies from external sources
        var allCompanies = await unifiedService.SearchCompaniesAsync(
            dataSource, 
            (long)(minTurnover ?? 1000000), 
            (long)(maxTurnover ?? 50000000), 
            hasOwnProducts);

        // Apply sorting
        var sortedCompanies = sortBy?.ToLowerInvariant() switch
        {
            "location" => sortDesc ? allCompanies.OrderByDescending(c => c.Location) : allCompanies.OrderBy(c => c.Location),
            "employees" or "employeecount" => sortDesc ? allCompanies.OrderByDescending(c => c.EmployeeCount) : allCompanies.OrderBy(c => c.EmployeeCount),
            "name" => sortDesc ? allCompanies.OrderByDescending(c => c.Name) : allCompanies.OrderBy(c => c.Name),
            "industry" => sortDesc ? allCompanies.OrderByDescending(c => c.Industry) : allCompanies.OrderBy(c => c.Industry),
            "turnover" => sortDesc ? allCompanies.OrderByDescending(c => c.Turnover) : allCompanies.OrderBy(c => c.Turnover),
            _ => allCompanies.OrderByDescending(c => c.Turnover) // Default sorting by turnover desc
        };

        // Apply pagination
        var total = sortedCompanies.Count();
        var items = sortedCompanies
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new {
                businessId = c.BusinessId, 
                name = c.Name, 
                turnover = c.Turnover, 
                industry = c.Industry,
                hasOwnProducts = c.HasOwnProducts, 
                productConfidenceScore = c.ProductConfidenceScore,
                employeeCount = c.EmployeeCount, 
                location = c.Location
            })
            .ToList();

        return Results.Ok(new { total, page, pageSize, items, sortBy, sortDesc, dataSource });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to search real companies: {ex.Message}");
    }
}).WithName("SearchRealCompanies");

// Demo search (keeps backward compatibility)
app.MapGet("/api/companies/search",
    (AppDbContext db, decimal? minTurnover, decimal? maxTurnover, bool? hasOwnProducts, 
     string? sortBy, bool sortDesc = false, int page = 1, int pageSize = 20) =>
{
    var q = db.Companies.AsQueryable();
    if (minTurnover is > 0) q = q.Where(c => c.Turnover >= minTurnover.Value);
    if (maxTurnover is > 0) q = q.Where(c => c.Turnover <= maxTurnover.Value);
    if (hasOwnProducts.HasValue) q = q.Where(c => c.HasOwnProducts == hasOwnProducts.Value);

    // Apply sorting
    q = sortBy?.ToLowerInvariant() switch
    {
        "location" => sortDesc ? q.OrderByDescending(c => c.Location) : q.OrderBy(c => c.Location),
        "employees" or "employeecount" => sortDesc ? q.OrderByDescending(c => c.EmployeeCount) : q.OrderBy(c => c.EmployeeCount),
        "name" => sortDesc ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name),
        "industry" => sortDesc ? q.OrderByDescending(c => c.Industry) : q.OrderBy(c => c.Industry),
        "turnover" => sortDesc ? q.OrderByDescending(c => c.Turnover) : q.OrderBy(c => c.Turnover),
        _ => q.OrderByDescending(c => c.Turnover) // Default sorting by turnover desc
    };

    var total = q.Count();
    var items = q.Skip((page - 1) * pageSize)
                 .Take(pageSize)
                 .Select(c => new {
                     c.BusinessId, c.Name, c.Turnover, c.Industry,
                     c.HasOwnProducts, c.ProductConfidenceScore,
                     c.Location, c.EmployeeCount
                 })
                 .ToList();

    return Results.Ok(new { total, page, pageSize, items, sortBy, sortDesc });
}).WithName("SearchCompanies");

// Clear all data
app.MapPost("/api/clear-data", (AppDbContext db) =>
{
    var count = db.Companies.Count();
    db.Companies.RemoveRange(db.Companies);
    db.SaveChanges();
    return Results.Ok(new { cleared = count, total = 0 });
});

// Seed with thousands of Finnish companies
app.MapPost("/api/seed-demo", (AppDbContext db) =>
{
    // Clear existing data first
    db.Companies.RemoveRange(db.Companies);
    db.SaveChanges();

    var cities = new[] 
    {
        "Helsinki", "Espoo", "Tampere", "Turku", "Oulu", "Jyväskylä", "Lahti", "Kuopio",
        "Vantaa", "Joensuu", "Lappeenranta", "Hämeenlinna", "Vaasa", "Pori", "Kotka", "Mikkeli",
        "Salo", "Kouvola", "Seinäjoki", "Savonlinna", "Rauma", "Rovaniemi", "Kajaani", "Iisalmi",
        "Pietarsaari", "Raahe", "Tornio", "Imatra", "Valkeakoski", "Hamina", "Forssa", "Lohja"
    };

    var industries = new[]
    {
        "Technology", "Manufacturing", "Healthcare", "Energy", "Construction", "Food & Beverage",
        "Automotive", "Electronics", "Software Development", "Biotechnology", "Clean Technology",
        "Marine Technology", "Forest Technology", "Agricultural Technology", "Tourism Technology",
        "Logistics Technology", "Digital Solutions", "Industrial Machinery", "Precision Engineering",
        "Renewable Energy", "Automation", "Transportation", "Steel Manufacturing", "Chemical Industry"
    };

    var companyTypes = new[] { "Oy", "Oyj", "Ab", "Ky", "T:mi" };
    var businessPrefixes = new[] 
    {
        "Nordic", "Finnish", "Arctic", "Baltic", "Suomen", "Polar", "Northern", "Fenno", "Scandi", 
        "Euro", "Turbo", "Mega", "Super", "Ultra", "Pro", "Max", "Tech", "Digi", "Smart", "Green"
    };

    var companies = new List<Company>();
    var random = new Random(42); // Fixed seed for reproducible results

    // Generate 2500 companies
    for (int i = 0; i < 2500; i++)
    {
        var city = cities[random.Next(cities.Length)];
        var industry = industries[random.Next(industries.Length)];
        var companyType = companyTypes[random.Next(companyTypes.Length)];
        var prefix = businessPrefixes[random.Next(businessPrefixes.Length)];
        
        var businessId = $"{random.Next(1000000, 9999999)}-{random.Next(1, 9)}";
        var name = $"{prefix} {city} {industry} {companyType}";
        
        // Generate realistic turnover (500K - 50M EUR)
        var turnover = (decimal)(random.NextDouble() * 49_500_000 + 500_000);
        
        // Estimate employees based on turnover with some randomness
        var baseEmployees = (int)(turnover / 250_000); // 250k per employee average
        var employeeCount = Math.Max(1, baseEmployees + random.Next(-5, 15));
        
        var hasOwnProducts = random.NextDouble() > 0.4; // 60% have own products
        var confidenceScore = random.NextDouble() * 0.4 + 0.6; // 0.6-1.0 range

        companies.Add(new Company
        {
            BusinessId = businessId,
            Name = name,
            Turnover = Math.Round(turnover, 2),
            Industry = industry,
            HasOwnProducts = hasOwnProducts,
            ProductConfidenceScore = Math.Round(confidenceScore, 2),
            Location = city,
            EmployeeCount = employeeCount
        });
    }

    // Add all companies in batches for better performance
    var batchSize = 100;
    for (int i = 0; i < companies.Count; i += batchSize)
    {
        var batch = companies.Skip(i).Take(batchSize);
        db.Companies.AddRange(batch);
    }
    
    db.SaveChanges();
    var total = db.Companies.Count();
    return Results.Ok(new { added = companies.Count, total });
});

// Health
app.MapGet("/health", () => Results.Ok("OK"));
app.Run();
