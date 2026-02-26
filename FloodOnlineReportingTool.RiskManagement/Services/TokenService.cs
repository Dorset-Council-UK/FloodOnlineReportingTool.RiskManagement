using FloodOnlineReportingTool.Public.Options;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public class TokenService(
    AuthenticationStateProvider authenticationStateProvider,
    ITokenAcquisition tokenAcquisition,
    IConfiguration configuration,
    ILogger<TokenService> logger) : ITokenService
{
    public async Task<string?> GetGeoServerTokenAsync(CancellationToken ct = default)
    {
        try
        {
            // Get user from Blazor's AuthenticationStateProvider (works in SignalR)
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                logger.LogDebug("No authenticated user - GeoServer request will be unauthenticated");
                return null;
            }

            var scopes = configuration
                .GetSection($"{DownstreamAPIOptions.SectionName}:{DownstreamAPIOptions.GeoServerApi}:Scopes")
                .Get<string[]>()
                ?? throw new InvalidOperationException("GeoServer scopes not configured");

            return await tokenAcquisition.GetAccessTokenForUserAsync(
                scopes,
                user: user as ClaimsPrincipal,
                tokenAcquisitionOptions: new TokenAcquisitionOptions
                {
                    CancellationToken = ct
                });
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            logger.LogWarning(ex, "User needs to consent to GeoServer scopes");
            return null; // Allow request to proceed without token
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to acquire GeoServer token");
            return null; // Allow request to proceed without token
        }
    }
}