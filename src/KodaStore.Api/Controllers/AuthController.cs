using KodaStore.Application.DTOs.Auth;
using KodaStore.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace KodaStore.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthUserResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);

        if (result.Status == RegisterStatus.Created) return StatusCode(StatusCodes.Status201Created, result.User);

        if (result.Status == RegisterStatus.DuplicateEmail) return Conflict(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Email already registered",
            Detail = "Email is used by another user."
        });

        var problem = new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["registration"] = result.Error
        })
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Registration failed"
        };
        return BadRequest(problem);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);

        if (result is null || !result.Succeeded) return Unauthorized(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Invalid Access",
            Detail = "Wrong Email or Password"
        });

        var response = new LoginResponse("Account Verified", result.User!);

        return Ok(response);
    }

}