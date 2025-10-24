using FloodOnlineReportingTool.RiskManagement.Services;
using FloodOnlineReportingTool.RiskManagement.Settings;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class ReportStatusApiExtensions
{
    internal static TBuilder AddReportStatusApi<TBuilder>(this TBuilder builder, ReportStatusApiSettings reportStatusApiSettings) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHttpClient(ReportStatusApiSettings.ClientName, client =>
        {
            client.BaseAddress = new Uri(reportStatusApiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(reportStatusApiSettings.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("User-Agent", reportStatusApiSettings.UserAgent);
        })
        .AddStandardResilienceHandler();

        // Register API service
        builder.Services.AddScoped<IReportStatusApiService, ReportStatusApiService>();

        return builder;
    }
}
