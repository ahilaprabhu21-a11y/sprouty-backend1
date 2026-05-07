using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sprouty.Api.Data;
using Sprouty.Api.DTOs;
using Sprouty.Api.Helpers;
using Sprouty.Api.Models;

namespace Sprouty.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly AppDbContext _db;
    public PostsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> Feed(
        [FromQuery] string? category, [FromQuery] int? userId)
    {
        var uid = User.RequireUserId();

        // Specific user's posts → simple chronological
        if (userId.HasValue)
        {
            var ulist = await _db.Posts
                .Include(p => p.User).Include(p => p.Challenge).Include(p => p.Applauses)
                .Where(p => p.UserId == userId.Value)
                .OrderByDescending(p => p.CreatedAt)
                .Take(100)
                .ToListAsync();
            return Ok(ulist.Select(p => p.ToDto(uid)));
        }

        // Specific category tab → simple chronological in that category
        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            var clist = await _db.Posts
                .Include(p => p.User).Include(p => p.Challenge).Include(p => p.Applauses)
                .Where(p => p.Category == category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(100)
                .ToListAsync();
            return Ok(clist.Select(p => p.ToDto(uid)));
        }

        // ===== "All" tab: smart 3-tier feed =====
        // Tier 1: followed people + my own posts
        // Tier 2: posts in my preferred categories (excluding tier 1)
        // Tier 3: everything else

        // Who I follow (+ myself)
        var followedIds = await _db.Follows
            .Where(f => f.FollowerId == uid)
            .Select(f => f.FollowingId)
            .ToListAsync();
        followedIds.Add(uid);
        var tier1Set = followedIds.ToHashSet();

        // My categories (CSV → list)
        var meUser = await _db.Users.FindAsync(uid);
        var myCategories = (meUser?.Categories ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet();

        // Pull a generous window of recent posts and tier them in memory.
        // 300 is plenty for a personal feed; trimmed to 100 after sorting.
        var recent = await _db.Posts
            .Include(p => p.User).Include(p => p.Challenge).Include(p => p.Applauses)
            .OrderByDescending(p => p.CreatedAt)
            .Take(300)
            .ToListAsync();

        int TierOf(Post p)
        {
            if (tier1Set.Contains(p.UserId)) return 1;
            if (myCategories.Contains(p.Category ?? "")) return 2;
            return 3;
        }

        var ordered = recent
            .OrderBy(TierOf)
            .ThenByDescending(p => p.CreatedAt)
            .Take(100)
            .ToList();

        return Ok(ordered.Select(p => p.ToDto(uid)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> Get(int id)
    {
        var uid = User.RequireUserId();
        var p = await _db.Posts
            .Include(p => p.User)
            .Include(p => p.Challenge)
            .Include(p => p.Applauses)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (p == null) return NotFound();
        return p.ToDto(uid);
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> Create([FromBody] CreatePostRequest req)
    {
        var uid = User.RequireUserId();
        var post = new Post
        {
            UserId = uid,
            Content = req.Content,
            MediaUrl = req.MediaUrl ?? "",
            MediaType = string.IsNullOrEmpty(req.MediaType) ? "none" : req.MediaType,
            Category = req.Category,
            ChallengeId = req.ChallengeId,
            IsFromChallenge = req.IsFromChallenge
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();
        await _db.Entry(post).Reference(p => p.User).LoadAsync();

        return CreatedAtAction(nameof(Get), new { id = post.Id }, post.ToDto(uid));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var uid = User.RequireUserId();
        var post = await _db.Posts
            .Include(p => p.Comments)
            .Include(p => p.Applauses)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();
        if (post.UserId != uid) return Forbid();

        _db.Comments.RemoveRange(post.Comments);
        _db.Applauses.RemoveRange(post.Applauses);
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();

        return Ok(new { deleted = true });
    }

    [HttpPost("{id}/applaud")]
    public async Task<ActionResult<object>> Applaud(int id)
    {
        var uid = User.RequireUserId();
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();

        var existing = await _db.Applauses.FirstOrDefaultAsync(a => a.PostId == id && a.UserId == uid);
        bool nowApplauded;
        if (existing != null)
        {
            _db.Applauses.Remove(existing);
            post.ApplaudCount = Math.Max(0, post.ApplaudCount - 1);
            nowApplauded = false;
        }
        else
        {
            _db.Applauses.Add(new Applause { PostId = id, UserId = uid });
            post.ApplaudCount += 1;
            nowApplauded = true;
        }
        await _db.SaveChangesAsync();
        return Ok(new { applauded = nowApplauded, count = post.ApplaudCount });
    }

    [HttpPost("{id}/share")]
    public async Task<ActionResult<object>> Share(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();
        post.ApplaudCount += 1;
        await _db.SaveChangesAsync();
        return Ok(new { count = post.ApplaudCount });
    }

    [HttpGet("{id}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> Comments(int id)
    {
        var list = await _db.Comments
            .Include(c => c.User)
            .Where(c => c.PostId == id)
            .OrderByDescending(c => c.CreatedAt)
            .Take(100)
            .ToListAsync();

        return Ok(list.Select(c => new CommentDto(
            c.Id, c.UserId, c.User?.Name ?? "", c.User?.AvatarUrl ?? "",
            c.Content, c.CreatedAt)));
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(int id, [FromBody] CreateCommentRequest req)
    {
        var uid = User.RequireUserId();
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();

        var c = new Comment { PostId = id, UserId = uid, Content = req.Content };
        _db.Comments.Add(c);
        post.CommentCount += 1;
        await _db.SaveChangesAsync();
        await _db.Entry(c).Reference(x => x.User).LoadAsync();

        return Ok(new CommentDto(c.Id, c.UserId, c.User?.Name ?? "", c.User?.AvatarUrl ?? "", c.Content, c.CreatedAt));
    }
}
