using System.ComponentModel.DataAnnotations;

namespace Sprouty.Api.DTOs;

public record LoginRequest(
    [Required] string Email,
    [Required] string Password);

public record SignupRequest(
    [Required, MaxLength(100)] string Name,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [MaxLength(300)] string? Headline,
    [MaxLength(500)] string? Categories,
    [MaxLength(100)] string? Location);

public record AuthResponse(string Token, UserDto User);

public record UserDto(
    int Id,
    string Name,
    string Email,
    string Headline,
    string Location,
    string AvatarUrl,
    string CoverUrl,
    string[] Categories,
    DateTime CreatedAt);
