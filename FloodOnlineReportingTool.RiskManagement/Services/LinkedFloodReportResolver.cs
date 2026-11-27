using FloodOnlineReportingTool.Database.Models.SavedSearches;
using FloodOnlineReportingTool.Database.Repositories;

namespace FloodOnlineReportingTool.RiskManagement.Services;

internal sealed class LinkedFloodReportResolver(
    IReportStatusClient reportStatusClient
) : ILinkedFloodReportResolver
{
    private const int PageSize = 500;

    public async Task<IReadOnlyCollection<string>> GetLinkedFloodReportReferences(SavedSearch savedSearch, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(savedSearch);

        if (savedSearch.ResultsFrom is null
            || savedSearch.ResultsTo is null
            || savedSearch.FloodSources.Count == 0)
        {
            return [];
        }

        var firstPage = await reportStatusClient.SearchFloodReports(
            savedSearch.ResultsFrom,
            savedSearch.ResultsTo,
            savedSearch.FloodSources,
            savedSearch.UPRN,
            savedSearch.USRN,
            savedSearch.Postcode,
            savedSearch.Geom,
            1,
            PageSize,
            cancellationToken);

        var references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddReferences(firstPage.Results, references);

        for (var page = 2; page <= firstPage.TotalPages; page++)
        {
            var currentPage = await reportStatusClient.SearchFloodReports(
                savedSearch.ResultsFrom,
                savedSearch.ResultsTo,
                savedSearch.FloodSources,
                savedSearch.UPRN,
                savedSearch.USRN,
                savedSearch.Postcode,
                savedSearch.Geom,
                page,
                PageSize,
                cancellationToken);

            AddReferences(currentPage.Results, references);
        }

        return [.. references];
    }

    private static void AddReferences(IEnumerable<Contracts.Shared.Search.SearchResultFloodReport> reports, ISet<string> references)
    {
        foreach (var reference in reports.Select(r => r.Reference).Where(r => !string.IsNullOrWhiteSpace(r)))
        {
            references.Add(reference);
        }
    }
}
