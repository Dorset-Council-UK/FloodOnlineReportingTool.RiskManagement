using Microsoft.Identity.Web;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Security.Claims;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class ClaimsPrincipalExtensions
{
    private const string Unknown = "Unknown";

    /// <summary>
    /// Find the display name from these claims preferred_username, DefaultNameClaimType, name
    /// </summary>
    internal static string DisplayName(this ClaimsPrincipal? claimsPrincipal) =>
        claimsPrincipal?.GetDisplayName() ?? Unknown;

    /// <summary>
    /// Find the email address from these claims emailaddress, emails
    /// </summary>
    internal static string Email(this ClaimsPrincipal? claimsPrincipal)
    {
        var emailaddress = claimsPrincipal?.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrWhiteSpace(emailaddress))
        {
            return emailaddress;
        }

        return claimsPrincipal?.FindFirstValue("emails") ?? Unknown;
    }

    /// <summary>
    /// Find the identity provider from these claims http://schemas.microsoft.com/identity/claims/identityprovider, http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider
    /// </summary>
    internal static string IdentityProvider(this ClaimsPrincipal? claimsPrincipal)
    {
        var identityprovider = claimsPrincipal?.FindFirstValue("http://schemas.microsoft.com/identity/claims/identityprovider");
        if (!string.IsNullOrWhiteSpace(identityprovider))
        {
            return identityprovider;
        }

        var accesscontrolservice = claimsPrincipal?.FindFirstValue("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider");
        return !string.IsNullOrWhiteSpace(accesscontrolservice) ? accesscontrolservice : Unknown;
    }

    internal static bool IsAuthenticated(this ClaimsPrincipal? claimsPrincipal) =>
        claimsPrincipal?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Find the user id from the NameIdentifier claim which is http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
    /// </summary>
    internal static string UserId (this ClaimsPrincipal? claimsPrincipal) =>
        claimsPrincipal?.GetNameIdentifierId() ?? Unknown;
}
