using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.RiskManagement.Options;
using Microsoft.Identity.Abstractions;
using NetTopologySuite.IO.Converters;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public class AzureApiClient(
    IDownstreamApi downstreamApi,
    ILogger<AzureApiClient> logger) : IAzureApiClient
{

    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new GeoJsonConverterFactory() },
    };

    private async Task<PagedResult<T>?> GetSearchResults<T>(string relativePath, CancellationToken ct)
    {
        return await downstreamApi
            .CallApiForUserAsync<PagedResult<T>>(RiskManagementOptions.ReportStatusApi, options =>
            {
                options.RelativePath = relativePath;
                options.Deserializer = (content) =>
                {
                    if (content is null) return null;
                    var stream = content.ReadAsStream(ct);
                    return JsonSerializer.Deserialize<PagedResult<T>>(stream, _jsonSerializerOptions);
                };
            }, cancellationToken: ct);
    }

    public async Task<> GetAddressSummary(string epsg, string searchArea, CancellationToken ct)
    {

    }


}