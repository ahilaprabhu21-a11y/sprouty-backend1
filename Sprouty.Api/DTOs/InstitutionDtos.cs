using System.ComponentModel.DataAnnotations;

namespace Sprouty.Api.DTOs;

public record InstitutionDto(
    int Id,
    string Name,
    string Category,
    string Description,
    string Location,
    string LogoUrl,
    string[] Courses,
    string[] Achievements,
    string[] SuccessStories,
    int MemberCount,
    bool JoinedByMe);

public record CreateInstitutionRequest(
    [Required, MaxLength(150)] string Name,
    [Required, MaxLength(50)] string Category,
    [MaxLength(500)] string? Description,
    [MaxLength(100)] string? Location,
    string? LogoUrl,
    string[]? Courses,
    string[]? Achievements,
    string[]? SuccessStories);
