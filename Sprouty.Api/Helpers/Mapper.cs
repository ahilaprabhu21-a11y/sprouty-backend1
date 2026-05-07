using Sprouty.Api.DTOs;
using Sprouty.Api.Models;
using System.Text.Json;

namespace Sprouty.Api.Helpers;

public static class Mapper
{
    public static UserDto ToDto(this User u) => new(
        u.Id, u.Name, u.Email, u.Headline ?? "", u.Location ?? "",
        u.AvatarUrl ?? "", u.CoverUrl ?? "",
        string.IsNullOrWhiteSpace(u.Categories)
            ? Array.Empty<string>()
            : u.Categories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        u.CreatedAt);

    public static PostDto ToDto(this Post p, int? currentUserId)
    {
        var applauded = currentUserId.HasValue && p.Applauses != null
            && p.Applauses.Any(a => a.UserId == currentUserId.Value);

        int? day = null;
        if (p.IsFromChallenge && p.ChallengeId.HasValue && p.Challenge != null)
        {
            day = (int)Math.Max(1, (p.CreatedAt - p.Challenge.StartDate).TotalDays + 1);
        }

        return new PostDto(
            p.Id, p.UserId,
            p.User?.Name ?? "",
            p.User?.AvatarUrl ?? "",
            p.Content, p.MediaUrl ?? "", p.MediaType ?? "none",
            p.Category, p.ChallengeId, p.IsFromChallenge, day,
            p.ApplaudCount, p.CommentCount, applauded, p.CreatedAt);
    }

    public static ChallengeDto ToDto(this Challenge c)
    {
        var totalDays = c.DurationDays;
        var start = DateTime.SpecifyKind(c.StartDate, DateTimeKind.Utc);
        var elapsed = (int)Math.Floor((DateTime.UtcNow - start).TotalDays) + 1;
        var currentDay = Math.Clamp(elapsed, 1, totalDays);

        var entries = c.Entries ?? new List<ChallengeEntry>();
        var completedDays = entries.Select(e => e.DayNumber).Distinct().Count();
        var progress = totalDays == 0 ? 0 : (int)((completedDays / (double)totalDays) * 100);

        var entryDays = entries.Select(e => e.DayNumber).ToHashSet();
        int streak = 0;
        for (int d = currentDay; d >= 1; d--)
        {
            if (entryDays.Contains(d)) streak++;
            else break;
        }

        return new ChallengeDto(
            c.Id, c.UserId, c.User?.Name ?? "",
            c.Title, c.Category, c.Description ?? "",
            c.DurationDays, c.StartDate, currentDay,
            completedDays, streak, progress, c.CreatedAt);
    }

    public static EntryDto ToDto(this ChallengeEntry e) => new(
        e.Id, e.ChallengeId, e.DayNumber, e.Note ?? "", e.MediaUrl ?? "",
        e.MediaType ?? "none", e.SharedToFeed, e.CreatedAt);

    public static InstitutionDto ToDto(this Institution i, int memberCount, bool joined) => new(
        i.Id, i.Name, i.Category, i.Description ?? "", i.Location ?? "", i.LogoUrl ?? "",
        Parse(i.Courses), Parse(i.Achievements), Parse(i.SuccessStories),
        memberCount, joined);

    private static string[] Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return Array.Empty<string>();
        try { return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>(); }
        catch { return Array.Empty<string>(); }
    }
}
