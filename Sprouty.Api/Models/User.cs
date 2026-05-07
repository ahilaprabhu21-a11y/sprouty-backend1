using System.ComponentModel.DataAnnotations;

namespace Sprouty.Api.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Headline { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(500)]
    public string AvatarUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string CoverUrl { get; set; } = string.Empty;

    /// <summary>Comma-separated category list e.g. "Art,Music,Code"</summary>
    [MaxLength(500)]
    public string Categories { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();
    public ICollection<InstitutionMember> InstitutionMemberships { get; set; } = new List<InstitutionMember>();
}
