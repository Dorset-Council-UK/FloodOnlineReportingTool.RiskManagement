using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Settings;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Services;

// ideas:
// search locations /api/flood/location
// search properties /api/flood/property
// search roads /api/flood/road
// search flood reports /api/flood/reports
// search all?
// locations with unconfirmed flood reports
// properties with unconfirmed flood reports
// roads with unconfirmed flood reports
// unconfirmed flood reports
// start with existing search:
// these are flood report searches only
// these will definately be removed later
// /api/search/uprn?uprn=123456
// /api/search/coordinates?easting=11111&northing=22222
// /api/search/startdate?ImpactStartUtc=2025-10-27T11%3A42%3A59

internal class ReportStatusApiService(ILogger<ReportStatusApiService> logger, IHttpClientFactory httpClientFactory) : IReportStatusApiService
{
    private const string TryAgain = "Please try again and if the problem persists, contact support.";
    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

    public async Task<SearchResult> SearchUprn(long uprn, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("ReportStatusApiService SearchUprn called for UPRN {Uprn}", uprn);

        try
        {
            var url = string.Create(CultureInfo.InvariantCulture, $"/api/search/uprn?uprn={uprn}");

            var client = httpClientFactory.CreateClient(ReportStatusApiSettings.ClientName);
            var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var floodReports = await response.Content
                    .ReadFromJsonAsync<IReadOnlyCollection<TEMP_FloodReport>>(_jsonOptions, cancellationToken)
                    .ConfigureAwait(false);

                if (floodReports is null)
                {
                    logger.LogWarning("ReportStatusApiService SearchUprn returned null flood reports for UPRN {Uprn}, please investigate. The user will see an empty search result, no errors.", uprn);
                    return SearchResult.Success([]);
                }

                logger.LogInformation("ReportStatusApiService SearchUprn succeeded for UPRN {Uprn} with {Count} flood reports found.", uprn, floodReports.Count);
                return SearchResult.Success(floodReports);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                logger.LogWarning("ReportStatusApiService SearchUprn unauthorized access for UPRN {Uprn}", uprn);
                return SearchResult.Failure([
                    "You do not have permission to search for flood reports.",
                    TryAgain,
                ]);
            }

            logger.LogError("ReportStatusApiService SearchUprn failed with status code {StatusCode}", response.StatusCode);
            return SearchResult.Failure([
                "I am sorry but there was a problem searching for the UPRN.",
                TryAgain,
            ]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred in SearchUprn for UPRN {Uprn}", uprn);
            return SearchResult.Failure([
                "An unexpected error occurred while searching for the UPRN.",
                TryAgain,
            ]);
        }
    }
}
