using FloodOnlineReportingTool.Public.Options;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public class GeoServerTokenHandler(
    ITokenService tokenService,
    ILogger<GeoServerTokenHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await tokenService.GetGeoServerTokenAsync(cancellationToken);
        
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            logger.LogDebug("Added bearer token to GeoServer request");
        }
        else
        {
            logger.LogDebug("No token available - sending unauthenticated GeoServer request");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}