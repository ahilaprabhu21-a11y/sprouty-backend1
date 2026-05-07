using System.ComponentModel.DataAnnotations;

namespace Sprouty.Api.Models;

public class Challenge
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public int DurationDays { get; set; } = 21;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChallengeEntry> Entries { get; set; } = new List<ChallengeEntry>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
