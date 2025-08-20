using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProspectFinderPro.Shared.Models;

[Table("Products")]
public class Product
{
    [Key]
    public long Id { get; set; }

    [Required]
    public long CompanyId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(50)]
    public string? ProductType { get; set; }

    public bool IsMainProduct { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal? ConfidenceScore { get; set; }

    [MaxLength(50)]
    public string? Source { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; } = null!;
}