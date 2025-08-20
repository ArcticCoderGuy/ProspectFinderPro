using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProspectFinderPro.Shared.Models;

[Table("Companies")]
public class Company
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string BusinessId { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(15,2)")]
    public decimal? Turnover { get; set; }

    [MaxLength(100)]
    public string? Industry { get; set; }

    public bool HasOwnProducts { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal? ProductConfidenceScore { get; set; }

    public DateTime? LastVerified { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    public int? EmployeeCount { get; set; }

    [MaxLength(10)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<FinancialData> FinancialHistory { get; set; } = new List<FinancialData>();
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public virtual ProductOwnershipAnalysis? ProductOwnershipAnalysis { get; set; }
}