using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation($"Login attempt for email: {request.Email}");
        
        // Mock authentication - replace with real implementation
        var result = new AuthenticationResult
        {
            User = new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                FirstName = "Demo",
                LastName = "User",
                TenantId = "demo-tenant",
                Roles = new[] { "User" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            AccessToken = "mock-access-token-" + DateTime.UtcNow.Ticks,
            RefreshToken = "mock-refresh-token-" + DateTime.UtcNow.Ticks,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        return Ok(new ApiResponse<AuthenticationResult>
        {
            Success = true,
            Data = result,
            Message = "Login successful"
        });
    }

    /// <summary>
    /// User registration
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationResult>> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation($"Registration attempt for email: {request.Email}");
        
        // Mock registration - replace with real implementation
        var result = new AuthenticationResult
        {
            User = new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                TenantId = request.TenantId,
                Department = request.Department,
                JobTitle = request.JobTitle,
                PhoneNumber = request.PhoneNumber,
                TimeZone = request.TimeZone ?? "UTC+00:00",
                Language = request.Language ?? "en",
                Roles = new[] { "User" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            AccessToken = "mock-access-token-" + DateTime.UtcNow.Ticks,
            RefreshToken = "mock-refresh-token-" + DateTime.UtcNow.Ticks,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        return Ok(new ApiResponse<AuthenticationResult>
        {
            Success = true,
            Data = result,
            Message = "Registration successful"
        });
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthenticationResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Token refresh attempt");
        
        // Mock token refresh - replace with real implementation
        var result = new AuthenticationResult
        {
            User = new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Email = "demo@example.com",
                FirstName = "Demo",
                LastName = "User",
                TenantId = "demo-tenant",
                Roles = new[] { "User" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            AccessToken = "refreshed-access-token-" + DateTime.UtcNow.Ticks,
            RefreshToken = "refreshed-refresh-token-" + DateTime.UtcNow.Ticks,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        return Ok(new ApiResponse<AuthenticationResult>
        {
            Success = true,
            Data = result,
            Message = "Token refreshed successfully"
        });
    }

    /// <summary>
    /// User logout
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        _logger.LogInformation("Logout attempt");
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Logout successful"
        });
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        _logger.LogInformation("Get profile attempt");
        
        var user = new UserDto
        {
            Id = Guid.NewGuid().ToString(),
            Email = "demo@example.com",
            FirstName = "Demo",
            LastName = "User",
            TenantId = "demo-tenant",
            Roles = new[] { "User" },
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Data = user,
            Message = "Profile retrieved successfully"
        });
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        _logger.LogInformation("Update profile attempt");
        
        var user = new UserDto
        {
            Id = Guid.NewGuid().ToString(),
            Email = "demo@example.com",
            FirstName = request.FirstName,
            LastName = request.LastName,
            Department = request.Department,
            JobTitle = request.JobTitle,
            PhoneNumber = request.PhoneNumber,
            TimeZone = request.TimeZone,
            Language = request.Language,
            TenantId = "demo-tenant",
            Roles = new[] { "User" },
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Data = user,
            Message = "Profile updated successfully"
        });
    }

    /// <summary>
    /// Change password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        _logger.LogInformation("Change password attempt");
        
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Password changed successfully"
        });
    }
}

// DTOs
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class AuthenticationResult
{
    public UserDto User { get; set; } = new();
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TimeZone { get; set; }
    public string? Language { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
    public Dictionary<string, object>? Preferences { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
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
