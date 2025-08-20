using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProspectFinderPro.Shared.Models;

[Table("ProductOwnershipAnalysis")]
public class ProductOwnershipAnalysis
{
    [Key]
    public long Id { get; set; }

    [Required]
    public long CompanyId { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal IndustryScore { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal PatentScore { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal ExportScore { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal WebsiteScore { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal CompanySizeScore { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal OverallConfidenceScore { get; set; }

    [MaxLength(1000)]
    public string? AnalysisReasoning { get; set; }

    [MaxLength(500)]
    public string? KeyIndicators { get; set; }

    [MaxLength(500)]
    public string? RiskFactors { get; set; }

    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(20)]
    public string? AlgorithmVersion { get; set; } = "1.0";

    // Navigation properties
    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; } = null!;
}