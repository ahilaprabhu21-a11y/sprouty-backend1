namespace Sprouty.Api.Models;

public class Follow
{
    public int Id { get; set; }

    /// <summary>The user who is following</summary>
    public int FollowerId { get; set; }
    public User? Follower { get; set; }

    /// <summary>The user being followed</summary>
    public int FollowingId { get; set; }
    public User? Following { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
