using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Services;

/// <summary>
/// Orchestrates a reference lookup across both local database entities and
/// downstream remote APIs. This service intentionally sits in the web/application
/// layer because it coordinates multiple infrastructure concerns (EF + HTTP).
/// </summary>
public class ReferenceSearchService(
    ILogger<ReferenceSearchService> logger,
    ICommonRepository commonRepository,
    IReportStatusClient reportStatusClient) : IReferenceSearchService
{
    /// <summary>
    /// Searches all known record sources for a matching reference string and
    /// returns a <see cref="SearchRedirect"/> indicating the matched record type.
    /// Local DB is checked first to avoid unnecessary HTTP calls.
    /// </summary>
    public async Task<GetResult<SearchRedirect>> FindByReference(string reference, CancellationToken ct)
    {
        logger.LogInformation("Searching all sources for reference");

        // Local first
        var localResult = await commonRepository.FindLocalByReference(reference, ct);
        if (localResult.IsSuccess)
        {
            return localResult;
        }

        // Fall through to remote API if not found locally
        var reportResult = await reportStatusClient.FindFloodReportByReference(reference, ct);
        if (reportResult.IsSuccess)
        {
            return GetResult<SearchRedirect>.Success(new SearchRedirect
            {
                Id = reportResult.ResultModel?.Id ?? null,
                Reference = reference,
                RedirectType = RecordTypes.Report
            });
        }

        logger.LogWarning("Reference {Reference} was not found in any source", reference);
        return GetResult<SearchRedirect>.Failure([$"No record found for reference '{reference}'."]);
    }
}