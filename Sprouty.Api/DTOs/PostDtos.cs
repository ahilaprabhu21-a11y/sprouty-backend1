using System.ComponentModel.DataAnnotations;

namespace Sprouty.Api.DTOs;

public record CreatePostRequest(
    [Required] string Content,
    string? MediaUrl,
    string? MediaType,
    [Required] string Category,
    int? ChallengeId,
    bool IsFromChallenge);

public record PostDto(
    int Id,
    int UserId,
    string UserName,
    string UserAvatarUrl,
    string Content,
    string MediaUrl,
    string MediaType,
    string Category,
    int? ChallengeId,
    bool IsFromChallenge,
    int? ChallengeDayNumber,
    int ApplaudCount,
    int CommentCount,
    bool ApplaudedByMe,
    DateTime CreatedAt);

public record CommentDto(
    int Id,
    int UserId,
    string UserName,
    string UserAvatarUrl,
    string Content,
    DateTime CreatedAt);

public record CreateCommentRequest([Required] string Content);
