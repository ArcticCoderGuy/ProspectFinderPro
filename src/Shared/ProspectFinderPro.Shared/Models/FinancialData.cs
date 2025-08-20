using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProspectFinderPro.Shared.Models;

[Table("FinancialData")]
public class FinancialData
{
    [Key]
    public long Id { get; set; }

    [Required]
    public long CompanyId { get; set; }

    [Required]
    public int Year { get; set; }

    [Column(TypeName = "decimal(15,2)")]
    public decimal? Revenue { get; set; }

    [Column(TypeName = "decimal(15,2)")]
    public decimal? Profit { get; set; }

    [Column(TypeName = "decimal(15,2)")]
    public decimal? Assets { get; set; }

    [Column(TypeName = "decimal(15,2)")]
    public decimal? Liabilities { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal? FinancialHealthScore { get; set; }

    [MaxLength(20)]
    public string? Currency { get; set; } = "EUR";

    [MaxLength(50)]
    public string? Source { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; } = null!;
}