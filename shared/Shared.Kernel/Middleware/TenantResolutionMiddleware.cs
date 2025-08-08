using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Shared.Kernel.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract tenant ID from JWT claims
        var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            context.Items["TenantId"] = tenantId;
        }

        // Extract user ID from JWT claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            context.Items["UserId"] = userId;
        }

        // Extract user roles
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        context.Items["UserRoles"] = roles;

        await _next(context);
    }
}

public static class HttpContextExtensions
{
    public static Guid? GetTenantId(this HttpContext context)
    {
        return context.Items["TenantId"] as Guid?;
    }

    public static Guid? GetUserId(this HttpContext context)
    {
        return context.Items["UserId"] as Guid?;
    }

    public static List<string> GetUserRoles(this HttpContext context)
    {
        return (context.Items["UserRoles"] as List<string>) ?? new List<string>();
    }

    public static bool HasRole(this HttpContext context, string role)
    {
        var roles = context.GetUserRoles();
        return roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
