using FloodOnlineReportingTool.Database.Models.Enums;
using FloodOnlineReportingTool.Database.Models.SavedSearches;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Search;

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
        var localResult = await FindLocalByReference(reference, ct);
        if (localResult is not null)
        {
            return localResult;
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

    public async Task<IReadOnlyList<OmniboxSearchResult>> Search(string query, int maxResults, CancellationToken ct)
    {
        var normalizedQuery = query.Trim();
        if (normalizedQuery.Length == 0 || maxResults <= 0)
        {
            return [];
        }

        var localReferenceResult = await FindLocalByReference(normalizedQuery.ToUpperInvariant(), ct);
        if (localReferenceResult is not null)
        {
            return [ToSearchResult(localReferenceResult)];
        }

        var reportResult = await reportStatusClient.FindFloodReportByReference(normalizedQuery, ct);
        if (reportResult is not null)
        {
            return [ToSearchResult(reportResult)];
        }

        var titleMatches = await savedSearchRepository.FindByTitle(normalizedQuery, maxResults, ct);
        if (titleMatches.Count > 0)
        {
            return titleMatches
                .Select(match => new OmniboxSearchResult(
                    match.FloodEventId ?? match.Id,
                    match.Reference,
                    match.SearchTitle,
                    match.FloodEventId.HasValue ? RecordTypes.Event : RecordTypes.Search))
                .DistinctBy(result => (result.ResultType, result.Id))
                .ToArray();
        }

        return [];
    }

    private async Task<SearchRedirect?> FindLocalByReference(string reference, CancellationToken ct)
    {
        var result = await savedSearchRepository.FindLocalByReference(reference, ct);
        return result.IsSuccess ? result.Value : null;
    }

    private static OmniboxSearchResult ToSearchResult(SearchRedirect redirect)
    {
        return new OmniboxSearchResult(
            redirect.Id ?? Guid.Empty,
            redirect.Reference,
            string.Empty,
            redirect.RedirectType);
    }
}