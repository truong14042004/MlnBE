using System.Security.Claims;

namespace DigitalDetox.Api.Services;

/// <summary>
/// Helpers to safely resolve the acting user id from the authenticated principal,
/// preventing IDOR (a client cannot read/write another user's data by guessing ids).
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the user id from a valid token (claim "sub" / NameIdentifier).
    /// If there is no authenticated user, falls back to "anon" (guest mode).
    /// Any client-supplied userId is intentionally ignored when a token is present.
    /// </summary>
    public static string ResolveUserId(this ClaimsPrincipal? user)
    {
        var id = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? user?.FindFirst("sub")?.Value;
        return string.IsNullOrWhiteSpace(id) ? "anon" : id;
    }
}
