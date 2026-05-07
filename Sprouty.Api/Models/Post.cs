using System.ComponentModel.DataAnnotations;

namespace Sprouty.Api.Models;

public class Post
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string MediaUrl { get; set; } = string.Empty;

    [MaxLength(20)]
    public string MediaType { get; set; } = "none"; // image | video | none

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    public int? ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }

    public bool IsFromChallenge { get; set; }
    public int ApplaudCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Applause> Applauses { get; set; } = new List<Applause>();
}
