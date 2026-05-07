using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sprouty.Api.Data;
using Sprouty.Api.DTOs;
using Sprouty.Api.Helpers;

namespace Sprouty.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var uid = User.RequireUserId();
        var u = await _db.Users.FindAsync(uid);
        if (u == null) return NotFound();
        return u.ToDto();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var me = User.RequireUserId();
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();

        var posts = await _db.Posts.CountAsync(p => p.UserId == id);
        var challenges = await _db.Challenges.CountAsync(c => c.UserId == id);
        var followers = await _db.Follows.CountAsync(f => f.FollowingId == id);
        var following = await _db.Follows.CountAsync(f => f.FollowerId == id);
        var iFollowThem = await _db.Follows.AnyAsync(f => f.FollowerId == me && f.FollowingId == id);

        return Ok(new
        {
            user = u.ToDto(),
            stats = new { posts, challenges, followers, following },
            iFollowThem
        });
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<UserDto>>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<UserDto>());
        var lower = q.Trim().ToLower();
        var users = await _db.Users
            .Where(u => u.Name.ToLower().Contains(lower) || u.Email.Contains(lower))
            .Take(20)
            .ToListAsync();
        return Ok(users.Select(u => u.ToDto()));
    }

    [HttpGet("suggested")]
    public async Task<ActionResult<IEnumerable<UserDto>>> Suggested()
    {
        var uid = User.RequireUserId();

        // Suggest users I'm not already following
        var alreadyFollowing = await _db.Follows
            .Where(f => f.FollowerId == uid)
            .Select(f => f.FollowingId)
            .ToListAsync();

        var users = await _db.Users
            .Where(u => u.Id != uid
                     && u.Email != "team@sprouty.app"
                     && !alreadyFollowing.Contains(u.Id))
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .ToListAsync();
        return Ok(users.Select(u => u.ToDto()));
    }
}
