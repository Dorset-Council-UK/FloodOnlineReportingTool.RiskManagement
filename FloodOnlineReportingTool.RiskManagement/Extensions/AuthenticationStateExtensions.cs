using FloodOnlineReportingTool.RiskManagement.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace Microsoft.AspNetCore.Components.Authorization;

internal static class AuthenticationStateExtensions
{
    extension(Task<AuthenticationState>? authenticationStateTask)
    {
        internal Task<bool> TryIsReaderAsync(IAuthorizationService authorizationService)
        {
            return authenticationStateTask.TryIsAuthenticatedAndAuthorizedAsync(authorizationService, PolicyNames.Reader);
        }

        internal Task<bool> TryIsWriterAsync(IAuthorizationService authorizationService)
        {
            return authenticationStateTask.TryIsAuthenticatedAndAuthorizedAsync(authorizationService, PolicyNames.Writer);
        }

        internal Task<bool> TryIsAdminAsync(IAuthorizationService authorizationService)
        {
            return authenticationStateTask.TryIsAuthenticatedAndAuthorizedAsync(authorizationService, PolicyNames.Admin);
        }

        internal async Task<bool> TryIsAuthenticatedAndAuthorizedAsync(IAuthorizationService authorizationService, string policyName)
        {
            ArgumentNullException.ThrowIfNull(authorizationService);

            if (string.IsNullOrWhiteSpace(policyName) || authenticationStateTask is null)
            {
                return false;
            }

            try
            {
                var authState = await authenticationStateTask;
                if (authState.User.Identity?.IsAuthenticated is not true)
                {
                    return false;
                }

                var authorizationResult = await authorizationService.AuthorizeAsync(authState.User, policyName);
                return authorizationResult.Succeeded;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}
