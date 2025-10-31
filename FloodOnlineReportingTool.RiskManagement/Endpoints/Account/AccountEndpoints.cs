using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FloodOnlineReportingTool.RiskManagement.Endpoints.Account;

internal static class AccountEndpoints
{
    internal static Results<ChallengeHttpResult, UnauthorizedHttpResult, ForbidHttpResult> Login()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/floodriskmanagement",
        };
        return TypedResults.Challenge(properties, [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
    }

    internal static Results<SignOutHttpResult, UnauthorizedHttpResult> Logout()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/floodriskmanagement/account/signed-out",
        };

        return TypedResults.SignOut(properties, [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
    }
}
