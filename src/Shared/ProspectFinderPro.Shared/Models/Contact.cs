using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProspectFinderPro.Shared.Models;

[Table("Contacts")]
public class Contact
{
    [Key]
    public long Id { get; set; }

    [Required]
    public long CompanyId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Position { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? LinkedInProfile { get; set; }

    public bool IsDecisionMaker { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal? ContactQualityScore { get; set; }

    [MaxLength(50)]
    public string? Source { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; } = null!;
}