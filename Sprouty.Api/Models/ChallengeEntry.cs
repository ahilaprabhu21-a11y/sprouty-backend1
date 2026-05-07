using System.ComponentModel.DataAnnotations;

namespace Sprouty.Api.Models;

public class ChallengeEntry
{
    public int Id { get; set; }
    public int ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }

    public int DayNumber { get; set; }

    [MaxLength(1000)]
    public string Note { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string MediaUrl { get; set; } = string.Empty;

    [MaxLength(20)]
    public string MediaType { get; set; } = "none";

    public bool SharedToFeed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
