namespace ProspectFinderPro.ApiGateway.Models;

public class Company
{
    public int Id { get; set; }
    public string BusinessId { get; set; } = default!;   // 1234567-8
    public string Name { get; set; } = default!;
    public decimal Turnover { get; set; }                // euroina
    public string Industry { get; set; } = "";
    public bool HasOwnProducts { get; set; }
    public double ProductConfidenceScore { get; set; }   // 0..1
    public string Location { get; set; } = "";
    public int EmployeeCount { get; set; }
}
