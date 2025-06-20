using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.AspNetCore.Builder;

internal static class RiskManagementHealthExtensions
{
    private static readonly IEnumerable<string> tagsLive = ["live"];

    public static IServiceCollection AddRiskManagementHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), tagsLive);

        return services;
    }

    public static void MapRiskManagementHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions()
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        });

        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live"),
        });
    }
}
