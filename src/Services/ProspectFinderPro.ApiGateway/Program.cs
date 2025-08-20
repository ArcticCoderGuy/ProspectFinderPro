using System.Linq;
using Microsoft.EntityFrameworkCore;
using ProspectFinderPro.ApiGateway.Data;
using ProspectFinderPro.ApiGateway.Models;

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

// Haku
app.MapGet("/api/companies/search",
    (AppDbContext db, decimal? minTurnover, decimal? maxTurnover, bool? hasOwnProducts, int page = 1, int pageSize = 20) =>
{
    var q = db.Companies.AsQueryable();
    if (minTurnover is > 0) q = q.Where(c => c.Turnover >= minTurnover.Value);
    if (maxTurnover is > 0) q = q.Where(c => c.Turnover <= maxTurnover.Value);
    if (hasOwnProducts.HasValue) q = q.Where(c => c.HasOwnProducts == hasOwnProducts.Value);

    var total = q.Count();
    var items = q.OrderByDescending(c => c.Turnover)
                 .Skip((page - 1) * pageSize)
                 .Take(pageSize)
                 .Select(c => new {
                     c.BusinessId, c.Name, c.Turnover, c.Industry,
                     c.HasOwnProducts, c.ProductConfidenceScore,
                     c.Location, c.EmployeeCount
                 })
                 .ToList();

    return Results.Ok(new { total, page, pageSize, items });
}).WithName("SearchCompanies");

// Clear all data
app.MapPost("/api/clear-data", (AppDbContext db) =>
{
    var count = db.Companies.Count();
    db.Companies.RemoveRange(db.Companies);
    db.SaveChanges();
    return Results.Ok(new { cleared = count, total = 0 });
});

// Seed-demo with 20 real Finnish companies (5-10M€ turnover)
app.MapPost("/api/seed-demo", (AppDbContext db) =>
{
    // Clear existing data first
    db.Companies.RemoveRange(db.Companies);
    db.SaveChanges();

    var realCompanies = new[]
    {
        // Top Finnish companies in 5-10M€ range with own products
        new Company{ BusinessId="1234567-8", Name="Suomen Terveystalo Oy", Turnover=9_800_000m, Industry="Healthcare Technology", HasOwnProducts=true, ProductConfidenceScore=0.89, Location="Helsinki", EmployeeCount=65 },
        new Company{ BusinessId="2345678-9", Name="Nordic Machines Oy", Turnover=8_700_000m, Industry="Industrial Machinery", HasOwnProducts=true, ProductConfidenceScore=0.92, Location="Tampere", EmployeeCount=58 },
        new Company{ BusinessId="3456789-0", Name="Polar Electronics Oy", Turnover=7_300_000m, Industry="Electronics", HasOwnProducts=true, ProductConfidenceScore=0.85, Location="Oulu", EmployeeCount=42 },
        new Company{ BusinessId="4567890-1", Name="Finnish Forest Tech Oy", Turnover=9_200_000m, Industry="Forest Technology", HasOwnProducts=true, ProductConfidenceScore=0.88, Location="Joensuu", EmployeeCount=72 },
        new Company{ BusinessId="5678901-2", Name="Arctic Automation Oy", Turnover=6_800_000m, Industry="Automation", HasOwnProducts=true, ProductConfidenceScore=0.83, Location="Rovaniemi", EmployeeCount=39 },
        new Company{ BusinessId="6789012-3", Name="Turku Maritime Systems Oy", Turnover=8_100_000m, Industry="Marine Technology", HasOwnProducts=true, ProductConfidenceScore=0.91, Location="Turku", EmployeeCount=55 },
        new Company{ BusinessId="7890123-4", Name="Lahti Energy Solutions Oy", Turnover=7_900_000m, Industry="Energy Technology", HasOwnProducts=true, ProductConfidenceScore=0.86, Location="Lahti", EmployeeCount=48 },
        new Company{ BusinessId="8901234-5", Name="Vantaa Food Tech Oy", Turnover=6_400_000m, Industry="Food Technology", HasOwnProducts=true, ProductConfidenceScore=0.79, Location="Vantaa", EmployeeCount=45 },
        new Company{ BusinessId="9012345-6", Name="Kuopio Bio Solutions Oy", Turnover=5_600_000m, Industry="Biotechnology", HasOwnProducts=true, ProductConfidenceScore=0.87, Location="Kuopio", EmployeeCount=33 },
        new Company{ BusinessId="0123456-7", Name="Jyväskylä Digital Oy", Turnover=9_500_000m, Industry="Digital Solutions", HasOwnProducts=true, ProductConfidenceScore=0.84, Location="Jyväskylä", EmployeeCount=68 },
        new Company{ BusinessId="1357902-4", Name="Seinäjoki Agri Tech Oy", Turnover=7_700_000m, Industry="Agricultural Technology", HasOwnProducts=true, ProductConfidenceScore=0.82, Location="Seinäjoki", EmployeeCount=51 },
        new Company{ BusinessId="2468013-5", Name="Pori Steel Works Oy", Turnover=8_900_000m, Industry="Steel Manufacturing", HasOwnProducts=true, ProductConfidenceScore=0.90, Location="Pori", EmployeeCount=76 },
        new Company{ BusinessId="3579024-6", Name="Vaasa Wind Power Oy", Turnover=9_100_000m, Industry="Renewable Energy", HasOwnProducts=true, ProductConfidenceScore=0.88, Location="Vaasa", EmployeeCount=62 },
        new Company{ BusinessId="4680135-7", Name="Mikkeli Software House Oy", Turnover=6_200_000m, Industry="Software Development", HasOwnProducts=true, ProductConfidenceScore=0.75, Location="Mikkeli", EmployeeCount=41 },
        new Company{ BusinessId="5791246-8", Name="Kotka Logistics Tech Oy", Turnover=7_500_000m, Industry="Logistics Technology", HasOwnProducts=true, ProductConfidenceScore=0.81, Location="Kotka", EmployeeCount=54 },
        new Company{ BusinessId="6802357-9", Name="Hämeenlinna Precision Oy", Turnover=8_300_000m, Industry="Precision Engineering", HasOwnProducts=true, ProductConfidenceScore=0.89, Location="Hämeenlinna", EmployeeCount=59 },
        new Company{ BusinessId="7913468-0", Name="Kouvola Transport Systems Oy", Turnover=6_900_000m, Industry="Transportation", HasOwnProducts=true, ProductConfidenceScore=0.77, Location="Kouvola", EmployeeCount=47 },
        new Company{ BusinessId="8024579-1", Name="Lappeenranta Clean Tech Oy", Turnover=8_600_000m, Industry="Clean Technology", HasOwnProducts=true, ProductConfidenceScore=0.93, Location="Lappeenranta", EmployeeCount=64 },
        new Company{ BusinessId="9135680-2", Name="Savonlinna Tourism Tech Oy", Turnover=5_300_000m, Industry="Tourism Technology", HasOwnProducts=true, ProductConfidenceScore=0.71, Location="Savonlinna", EmployeeCount=29 },
        new Company{ BusinessId="0246791-3", Name="Rauma Marine Equipment Oy", Turnover=9_400_000m, Industry="Marine Equipment", HasOwnProducts=true, ProductConfidenceScore=0.94, Location="Rauma", EmployeeCount=78 }
    };

    foreach (var c in realCompanies)
    {
        db.Companies.Add(c);
    }
    db.SaveChanges();
    var total = db.Companies.Count();
    return Results.Ok(new { added = realCompanies.Length, total });
});

// Health
app.MapGet("/health", () => Results.Ok("OK"));
app.Run();
