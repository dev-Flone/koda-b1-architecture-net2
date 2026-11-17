using System;
using System.ComponentModel.DataAnnotations;

namespace KodaStore.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required, StringLength(100, MinimumLength = 10), EmailAddress] public string Email { get; init; } = string.Empty;
    [Required, StringLength(20, MinimumLength = 8)] public string Password { get; init; } = string.Empty;
    [Required, StringLength(120, MinimumLength = 2)] public string PublicName { get; init; } = string.Empty;
}
