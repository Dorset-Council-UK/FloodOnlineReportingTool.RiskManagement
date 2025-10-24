using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Services;

internal class ReportStatusApiService(ILogger<ReportStatusApiService> logger, IHttpClientFactory httpClientFactory) : IReportStatusApiService
{
    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

    public Task Test(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("ReportStatusApiService Test method called.");
        return Task.CompletedTask;
    }
}
