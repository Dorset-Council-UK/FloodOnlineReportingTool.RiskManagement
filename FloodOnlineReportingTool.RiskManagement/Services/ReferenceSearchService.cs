using FloodOnlineReportingTool.Database.Models.SavedSearches;
using FloodOnlineReportingTool.Database.Repositories;

namespace FloodOnlineReportingTool.RiskManagement.Services;

/// <summary>
/// Orchestrates a reference lookup across both local database entities and
/// downstream remote APIs. This service intentionally sits in the web/application
/// layer because it coordinates multiple infrastructure concerns (EF + HTTP).
/// </summary>
public class ReferenceSearchService(
    ILogger<ReferenceSearchService> logger,
    ISavedSearchRepository savedSearchRepository,
    IReportStatusClient reportStatusClient
) : IReferenceSearchService
{
    /// <summary>
    /// Searches all known record sources for a matching reference string and
    /// returns a <see cref="SearchRedirect"/> indicating the matched record type.
    /// Local DB is checked first to avoid unnecessary HTTP calls.
    /// </summary>
    public async Task<SearchRedirect?> FindByReference(string reference, CancellationToken ct)
    {
        logger.LogInformation("Searching all sources for reference");

        // Local first
        var findLocalResult = await savedSearchRepository.FindLocalByReference(reference, ct);
        if (findLocalResult.IsSuccess)
        {
            return findLocalResult.Value;
        }

        // Fall through to remote API if not found locally
        var searchRedirect = await reportStatusClient.FindFloodReportByReference(reference, ct);
        if (searchRedirect is not null)
        {
            return searchRedirect;
        }

        logger.LogWarning("Reference {Reference} was not found in any source", reference);
        return null;
    }
}