using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class HealthCheckExtensions
{
    private static readonly IEnumerable<string> tagsDatabase = ["database"];
    private static readonly IEnumerable<string> tagsLive = ["live"];

    internal static TBuilder AddRiskManagementHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services
            .AddHealthChecks()
            .AddDbContextCheck<RiskManagementDbContext>(tags: tagsDatabase)
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), tagsLive);

        return builder;
    }

    internal static void MapRiskManagementHealthChecks(this WebApplication app)
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
