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
public class ChallengesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ChallengesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChallengeDto>>> MyChallenges()
    {
        var uid = User.RequireUserId();
        var list = await _db.Challenges
            .Include(c => c.User)
            .Include(c => c.Entries)
            .Where(c => c.UserId == uid)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return Ok(list.Select(c => c.ToDto()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> Get(int id)
    {
        var uid = User.RequireUserId();
        var c = await _db.Challenges
            .Include(x => x.User)
            .Include(x => x.Entries)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        if (c.UserId != uid) return Forbid();

        var entries = c.Entries.OrderByDescending(e => e.DayNumber).Select(e => e.ToDto());
        return Ok(new { challenge = c.ToDto(), entries });
    }

    [HttpPost]
    public async Task<ActionResult<ChallengeDto>> Create([FromBody] CreateChallengeRequest req)
    {
        var uid = User.RequireUserId();
        var c = new Challenge
        {
            UserId = uid,
            Title = req.Title,
            Category = req.Category,
            Description = req.Description ?? "",
            DurationDays = 21,
            StartDate = DateTime.UtcNow.Date
        };
        _db.Challenges.Add(c);
        await _db.SaveChangesAsync();
        await _db.Entry(c).Reference(x => x.User).LoadAsync();
        await _db.Entry(c).Collection(x => x.Entries).LoadAsync();
        return CreatedAtAction(nameof(Get), new { id = c.Id }, c.ToDto());
    }

    [HttpPost("{id}/entries")]
    public async Task<ActionResult<EntryDto>> AddEntry(int id, [FromBody] AddEntryRequest req)
    {
        var uid = User.RequireUserId();
        var c = await _db.Challenges.FindAsync(id);
        if (c == null) return NotFound();
        if (c.UserId != uid) return Forbid();

        if (req.DayNumber < 1 || req.DayNumber > c.DurationDays)
            return BadRequest(new { message = "Invalid day number" });

        var entry = new ChallengeEntry
        {
            ChallengeId = id,
            DayNumber = req.DayNumber,
            Note = req.Note ?? "",
            MediaUrl = req.MediaUrl ?? "",
            MediaType = string.IsNullOrEmpty(req.MediaType) ? "none" : req.MediaType,
            SharedToFeed = req.ShareToFeed
        };
        _db.ChallengeEntries.Add(entry);

        if (req.ShareToFeed)
        {
            var post = new Post
            {
                UserId = uid,
                Content = $"Day {req.DayNumber} of '{c.Title}' — {req.Note ?? ""}",
                MediaUrl = entry.MediaUrl,
                MediaType = entry.MediaType,
                Category = c.Category,
                ChallengeId = c.Id,
                IsFromChallenge = true
            };
            _db.Posts.Add(post);
        }

        await _db.SaveChangesAsync();
        return Ok(entry.ToDto());
    }
}
