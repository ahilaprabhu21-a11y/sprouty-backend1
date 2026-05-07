using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sprouty.Api.Data;
using Sprouty.Api.DTOs;
using Sprouty.Api.Helpers;
using Sprouty.Api.Models;
using System.Text.Json;

namespace Sprouty.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InstitutionsController : ControllerBase
{
    private readonly AppDbContext _db;
    public InstitutionsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InstitutionDto>>> List([FromQuery] string? category)
    {
        var uid = User.RequireUserId();
        var query = _db.Institutions.AsQueryable();
        if (!string.IsNullOrEmpty(category) && category != "All")
            query = query.Where(i => i.Category == category);

        var list = await query.OrderBy(i => i.Name).ToListAsync();
        var memberCounts = await _db.InstitutionMembers
            .GroupBy(m => m.InstitutionId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count);
        var myMemberships = await _db.InstitutionMembers
            .Where(m => m.UserId == uid)
            .Select(m => m.InstitutionId)
            .ToListAsync();

        return Ok(list.Select(i => i.ToDto(
            memberCounts.GetValueOrDefault(i.Id, 0),
            myMemberships.Contains(i.Id))));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InstitutionDto>> Get(int id)
    {
        var uid = User.RequireUserId();
        var i = await _db.Institutions.FindAsync(id);
        if (i == null) return NotFound();
        var memberCount = await _db.InstitutionMembers.CountAsync(m => m.InstitutionId == id);
        var joined = await _db.InstitutionMembers.AnyAsync(m => m.InstitutionId == id && m.UserId == uid);
        return i.ToDto(memberCount, joined);
    }

    [HttpPost]
    public async Task<ActionResult<InstitutionDto>> Create([FromBody] CreateInstitutionRequest req)
    {
        var i = new Institution
        {
            Name = req.Name,
            Category = req.Category,
            Description = req.Description ?? "",
            Location = req.Location ?? "",
            LogoUrl = req.LogoUrl ?? "",
            Courses = JsonSerializer.Serialize(req.Courses ?? Array.Empty<string>()),
            Achievements = JsonSerializer.Serialize(req.Achievements ?? Array.Empty<string>()),
            SuccessStories = JsonSerializer.Serialize(req.SuccessStories ?? Array.Empty<string>())
        };
        _db.Institutions.Add(i);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = i.Id }, i.ToDto(0, false));
    }

    [HttpPost("{id}/join")]
    public async Task<ActionResult<object>> Join(int id)
    {
        var uid = User.RequireUserId();
        var inst = await _db.Institutions.FindAsync(id);
        if (inst == null) return NotFound();

        var existing = await _db.InstitutionMembers
            .FirstOrDefaultAsync(m => m.InstitutionId == id && m.UserId == uid);

        bool joined;
        if (existing != null)
        {
            _db.InstitutionMembers.Remove(existing);
            joined = false;
        }
        else
        {
            _db.InstitutionMembers.Add(new InstitutionMember { InstitutionId = id, UserId = uid });
            joined = true;
        }
        await _db.SaveChangesAsync();
        var count = await _db.InstitutionMembers.CountAsync(m => m.InstitutionId == id);
        return Ok(new { joined, memberCount = count });
    }
}
