using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Commands;
using UserService.Application.DTOs;
using UserService.Application.Queries;
using Shared.Kernel.Constants;
using Shared.Kernel.Middleware;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password,
            request.DeviceInfo,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString());

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Message.ToErrorResponse());

        return Ok(result.ToSuccessResponse());
    }

    /// <summary>
    /// User registration
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationResult>> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.TenantId,
            request.Department,
            request.JobTitle,
            request.PhoneNumber,
            request.TimeZone,
            request.Language);

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Message.ToErrorResponse());

        return Ok(result.ToSuccessResponse());
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthenticationResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Message.ToErrorResponse());

        return Ok(result.ToSuccessResponse());
    }

    /// <summary>
    /// User logout
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        var command = new LogoutCommand(request.RefreshToken);
        var result = await _mediator.Send(command);
        
        return result ? Ok("Logout successful".ToSuccessResponse()) : BadRequest("Logout failed".ToErrorResponse());
    }

    /// <summary>
    /// Validate token
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<TokenValidationResult>> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        var query = new ValidateTokenQuery(request.Token);
        var result = await _mediator.Send(query);
        
        return Ok(result.ToSuccessResponse());
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var userId = HttpContext.GetUserId();
        if (!userId.HasValue)
            return BadRequest("Invalid user context");

        var query = new GetUserProfileQuery(userId.Value);
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();

        return Ok(result.ToSuccessResponse());
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = HttpContext.GetUserId();
        if (!userId.HasValue)
            return BadRequest("Invalid user context");

        var command = new UpdateUserProfileCommand(
            userId.Value,
            request.FirstName,
            request.LastName,
            request.Department,
            request.JobTitle,
            request.PhoneNumber,
            request.TimeZone,
            request.Language,
            request.Preferences);

        var result = await _mediator.Send(command);
        
        if (result == null)
            return NotFound();

        return Ok(result.ToSuccessResponse());
    }

    /// <summary>
    /// Change password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = HttpContext.GetUserId();
        if (!userId.HasValue)
            return BadRequest("Invalid user context");

        var command = new ChangePasswordCommand(userId.Value, request.CurrentPassword, request.NewPassword);
        var result = await _mediator.Send(command);
        
        return result ? Ok("Password changed successfully".ToSuccessResponse()) : BadRequest("Password change failed".ToErrorResponse());
    }
}

// Request DTOs
public record LoginRequest(string Email, string Password, string? DeviceInfo);
public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string TenantId,
    string? Department,
    string? JobTitle,
    string? PhoneNumber,
    string? TimeZone,
    string? Language);
public record RefreshTokenRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);
public record ValidateTokenRequest(string Token);
public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? Department,
    string? JobTitle,
    string? PhoneNumber,
    string? TimeZone,
    string? Language,
    Dictionary<string, object>? Preferences);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
