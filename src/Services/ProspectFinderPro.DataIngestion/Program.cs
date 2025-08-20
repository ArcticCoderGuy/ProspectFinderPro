using Microsoft.EntityFrameworkCore;
using ProspectFinderPro.DataIngestion.Services;
using ProspectFinderPro.DataIngestion.Services.DataSourceClients;
using ProspectFinderPro.Shared.Data;
using Serilog;
using System.Text.Json;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS for web UI
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebUI", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Database
builder.Services.AddDbContext<ProspectFinderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HTTP Clients with Polly
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            Log.Warning("Retry {RetryCount} for {OperationKey} after {Delay}ms",
                retryCount, context.OperationKey, timespan.TotalMilliseconds);
        });

// HTTP Clients for different data sources
builder.Services.AddHttpClient<CompanyFactsClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CompanyFactsApi:BaseUrl"] ?? "https://companyfacts.eu");
    client.DefaultRequestHeaders.Add("User-Agent", "ProspectFinderPro/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient<AvoinDataClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AvoinDataApi:BaseUrl"] ?? "https://avoindata.fi/data/fi/api/3/action/");
    client.DefaultRequestHeaders.Add("User-Agent", "ProspectFinderPro/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient<YTJClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["YTJApi:BaseUrl"] ?? "https://avoindata.fi/data/fi/api/3/action/");
    client.DefaultRequestHeaders.Add("User-Agent", "ProspectFinderPro/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient<VeroClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["VeroApi:BaseUrl"] ?? "https://api.vero.fi/");
    client.DefaultRequestHeaders.Add("User-Agent", "ProspectFinderPro/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddPolicyHandler(retryPolicy);

// Services
builder.Services.AddScoped<CompanyFactsClient>();
builder.Services.AddScoped<AvoinDataClient>();
builder.Services.AddScoped<YTJClient>();
builder.Services.AddScoped<VeroClient>();
builder.Services.AddScoped<MultiSourceDataOrchestrator>();
builder.Services.AddScoped<CompanyDataProcessor>();
builder.Services.AddHostedService<DataIngestionBackgroundService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContext<ProspectFinderDbContext>()
    .AddHttpClient("AvoinData", client => client.BaseAddress = new Uri(builder.Configuration["AvoinDataApi:BaseUrl"]!));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

// Enable CORS
app.UseCors("AllowWebUI");

// Serve static files
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProspectFinderDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();