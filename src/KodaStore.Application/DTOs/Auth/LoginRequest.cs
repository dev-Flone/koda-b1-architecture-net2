using System;
using System.ComponentModel.DataAnnotations;

namespace KodaStore.Application.DTOs.Auth;

public class LoginRequest
{
    [EmailAddress, Required] public string Email { get; init; } = string.Empty;
    [Required] public string Password { get; init; } = string.Empty;
}
