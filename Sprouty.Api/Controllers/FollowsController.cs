using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sprouty.Api.Data;
using Sprouty.Api.Helpers;
using Sprouty.Api.Models;

namespace Sprouty.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FollowsController : ControllerBase
{
    private readonly AppDbContext _db;
    public FollowsController(AppDbContext db) => _db = db;

    /// <summary>Follow a user</summary>
    [HttpPost("{userId}")]
    public async Task<ActionResult<object>> Follow(int userId)
    {
        var me = User.RequireUserId();
        if (me == userId) return BadRequest(new { message = "You can't follow yourself" });

        var target = await _db.Users.FindAsync(userId);
        if (target == null) return NotFound();

        var exists = await _db.Follows.AnyAsync(f => f.FollowerId == me && f.FollowingId == userId);
        if (!exists)
        {
            _db.Follows.Add(new Follow { FollowerId = me, FollowingId = userId });
            await _db.SaveChangesAsync();
        }

        var followers = await _db.Follows.CountAsync(f => f.FollowingId == userId);
        return Ok(new { following = true, followers });
    }

    /// <summary>Unfollow a user (you stop following them)</summary>
    [HttpDelete("{userId}")]
    public async Task<ActionResult<object>> Unfollow(int userId)
    {
        var me = User.RequireUserId();
        var follow = await _db.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == me && f.FollowingId == userId);
        if (follow != null)
        {
            _db.Follows.Remove(follow);
            await _db.SaveChangesAsync();
        }
        var followers = await _db.Follows.CountAsync(f => f.FollowingId == userId);
        return Ok(new { following = false, followers });
    }

    /// <summary>Remove a follower (kick them off your followers list)</summary>
    [HttpDelete("remove-follower/{userId}")]
    public async Task<ActionResult<object>> RemoveFollower(int userId)
    {
        var me = User.RequireUserId();
        var follow = await _db.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == userId && f.FollowingId == me);
        if (follow != null)
        {
            _db.Follows.Remove(follow);
            await _db.SaveChangesAsync();
        }
        return Ok(new { removed = true });
    }

    /// <summary>List all users following the given userId</summary>
    [HttpGet("{userId}/followers")]
    public async Task<ActionResult<IEnumerable<object>>> GetFollowers(int userId)
    {
        var me = User.RequireUserId();
        var rows = await _db.Follows
            .Where(f => f.FollowingId == userId)
            .Include(f => f.Follower)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var iFollowSet = await _db.Follows
            .Where(f => f.FollowerId == me)
            .Select(f => f.FollowingId)
            .ToListAsync();

        return Ok(rows.Select(f => new
        {
            id = f.FollowerId,
            name = f.Follower!.Name,
            email = f.Follower.Email,
            headline = f.Follower.Headline,
            avatarUrl = f.Follower.AvatarUrl,
            iFollowThem = iFollowSet.Contains(f.FollowerId),
            isMyFollower = userId == me
        }));
    }

    /// <summary>List all users the given userId is following</summary>
    [HttpGet("{userId}/following")]
    public async Task<ActionResult<IEnumerable<object>>> GetFollowing(int userId)
    {
        var me = User.RequireUserId();
        var rows = await _db.Follows
            .Where(f => f.FollowerId == userId)
            .Include(f => f.Following)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var iFollowSet = await _db.Follows
            .Where(f => f.FollowerId == me)
            .Select(f => f.FollowingId)
            .ToListAsync();

        return Ok(rows.Select(f => new
        {
            id = f.FollowingId,
            name = f.Following!.Name,
            email = f.Following.Email,
            headline = f.Following.Headline,
            avatarUrl = f.Following.AvatarUrl,
            iFollowThem = iFollowSet.Contains(f.FollowingId),
            canUnfollow = userId == me
        }));
    }
}
