using System.ComponentModel.DataAnnotations;

namespace Sprouty.Api.Models;

public class Institution
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(500)]
    public string LogoUrl { get; set; } = string.Empty;

    public string Courses { get; set; } = "[]";
    public string Achievements { get; set; } = "[]";
    public string SuccessStories { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<InstitutionMember> Members { get; set; } = new List<InstitutionMember>();
}
