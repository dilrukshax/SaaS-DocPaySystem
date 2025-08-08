using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        UserDbContext context,
        ITokenService tokenService,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthenticationResult> LoginAsync(
        string email, 
        string password, 
        string? deviceInfo = null, 
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                return new AuthenticationResult(false, "Invalid credentials", null, null);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                var message = result.IsLockedOut ? "Account is locked out" :
                             result.IsNotAllowed ? "Account is not allowed to sign in" :
                             "Invalid credentials";
                return new AuthenticationResult(false, message, null, null);
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Generate tokens
            var tokens = await _tokenService.GenerateTokensAsync(user, cancellationToken);

            // Create session
            var session = new UserSession(
                user.Id,
                tokens.AccessToken,
                tokens.RefreshToken,
                deviceInfo,
                ipAddress,
                null, // UserAgent will be set by controller
                DateTime.UtcNow.AddDays(30)); // Refresh token expires in 30 days

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync(cancellationToken);

            // Get user details
            var userDto = await GetUserDetailsAsync(user, cancellationToken);

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return new AuthenticationResult(
                true, 
                "Login successful", 
                tokens, 
                userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for email {Email}", email);
            return new AuthenticationResult(false, "An error occurred during login", null, null);
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if tenant exists
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);
            if (tenant == null || !tenant.IsActive)
            {
                return new AuthenticationResult(false, "Invalid tenant", null, null);
            }

            // Check if user already exists in this tenant
            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.TenantId == request.TenantId, cancellationToken);
            if (existingUser != null)
            {
                return new AuthenticationResult(false, "User already exists", null, null);
            }

            // Create user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                TenantId = request.TenantId,
                EmailConfirmed = false, // Will be confirmed via email
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthenticationResult(false, $"Registration failed: {errors}", null, null);
            }

            // Create user profile
            var profile = new UserProfile
            {
                Department = request.Department,
                JobTitle = request.JobTitle,
                PhoneNumber = request.PhoneNumber,
                TimeZone = request.TimeZone ?? "UTC",
                Language = request.Language ?? "en",
                CreatedAt = DateTime.UtcNow
            };

            user.Profile = profile;
            await _context.SaveChangesAsync(cancellationToken);

            // Assign default role
            var defaultRole = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.TenantId == request.TenantId && r.Name == "User", cancellationToken);
            if (defaultRole != null)
            {
                await _userManager.AddToRoleAsync(user, defaultRole.Name!);
            }

            _logger.LogInformation("User {UserId} registered successfully", user.Id);

            // Generate tokens
            var tokens = await _tokenService.GenerateTokensAsync(user, cancellationToken);
            var userDto = await GetUserDetailsAsync(user, cancellationToken);

            return new AuthenticationResult(
                true,
                "Registration successful",
                tokens,
                userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for email {Email}", request.Email);
            return new AuthenticationResult(false, "An error occurred during registration", null, null);
        }
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _context.UserSessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken && s.IsActive, cancellationToken);

            if (session == null || session.ExpiresAt <= DateTime.UtcNow)
            {
                return new AuthenticationResult(false, "Invalid or expired refresh token", null, null);
            }

            var user = session.User;
            if (user == null || !user.IsActive)
            {
                return new AuthenticationResult(false, "User not found or inactive", null, null);
            }

            // Generate new tokens
            var tokens = await _tokenService.GenerateTokensAsync(user, cancellationToken);

            // Update session
            session.UpdateSession(tokens.AccessToken, tokens.RefreshToken, DateTime.UtcNow.AddDays(30));
            await _context.SaveChangesAsync(cancellationToken);

            var userDto = await GetUserDetailsAsync(user, cancellationToken);

            return new AuthenticationResult(
                true,
                "Token refreshed successfully",
                tokens,
                userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return new AuthenticationResult(false, "An error occurred during token refresh", null, null);
        }
    }

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken, cancellationToken);

            if (session != null)
            {
                session.Deactivate();
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return false;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _tokenService.ValidateTokenAsync(token, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            return false;
        }
    }

    private async Task<UserDto> GetUserDetailsAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await _context.Entry(user)
            .Reference(u => u.Profile)
            .LoadAsync(cancellationToken);

        await _context.Entry(user)
            .Collection(u => u.UserRoles)
            .Query()
            .Include(ur => ur.Role)
            .LoadAsync(cancellationToken);

        var roles = user.UserRoles?.Select(ur => ur.Role.Name).Where(name => name != null).Cast<string>().ToList() ?? new List<string>();

        return new UserDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.TenantId,
            roles,
            user.IsActive,
            user.LastLoginAt,
            user.CreatedAt,
            user.Profile != null ? new UserProfileDto(
                user.Profile.Department,
                user.Profile.JobTitle,
                user.Profile.PhoneNumber,
                user.Profile.TimeZone,
                user.Profile.Language,
                user.Profile.Preferences) : null);
    }
}
