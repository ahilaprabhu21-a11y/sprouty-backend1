using System.ComponentModel.DataAnnotations;

namespace Sprouty.Api.DTOs;

public record CreateChallengeRequest(
    [Required, MaxLength(150)] string Title,
    [Required, MaxLength(50)] string Category,
    [MaxLength(1000)] string? Description);

public record ChallengeDto(
    int Id,
    int UserId,
    string UserName,
    string Title,
    string Category,
    string Description,
    int DurationDays,
    DateTime StartDate,
    int CurrentDay,
    int CompletedDays,
    int StreakDays,
    int ProgressPercent,
    DateTime CreatedAt);

public record AddEntryRequest(
    [Required] int DayNumber,
    string? Note,
    string? MediaUrl,
    string? MediaType,
    bool ShareToFeed);

public record EntryDto(
    int Id,
    int ChallengeId,
    int DayNumber,
    string Note,
    string MediaUrl,
    string MediaType,
    bool SharedToFeed,
    DateTime CreatedAt);
