using FloodOnlineReportingTool.Database.DbContexts;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.AspNetCore.Builder;

internal static class HealthCheckExtensions
{
    private static readonly IEnumerable<string> tagsDatabase = ["database"];
    private static readonly IEnumerable<string> tagsLive = ["live"];

    extension(IHostApplicationBuilder builder)
    {
        internal IHostApplicationBuilder AddRiskManagementHealthChecks()
        {
            builder.Services
                .AddHealthChecks()
                .AddDbContextCheck<RiskManagementDbContext>(tags: tagsDatabase)
                // Add a default liveness check to ensure app is responsive
                .AddCheck("self", () => HealthCheckResult.Healthy(), tagsLive);

            return builder;
        }
    }

    extension(WebApplication app)
    {
        internal WebApplication MapRiskManagementHealthChecks()
        {
            app.MapHealthChecks("/health", new HealthCheckOptions()
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });

            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live"),
            });

            return app;
        }
    }
}
