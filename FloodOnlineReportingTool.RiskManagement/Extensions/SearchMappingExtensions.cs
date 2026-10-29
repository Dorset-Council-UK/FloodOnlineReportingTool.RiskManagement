using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.Database.Models.Enums;
using FloodOnlineReportingTool.Database.Models.Lookups;
using FloodOnlineReportingTool.Database.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using System.Net;
using System.Text;

namespace FloodOnlineReportingTool.RiskManagement.Extensions;

internal static class SearchMappingExtensions
{
    /// <summary>
    /// Creates a PagedResult from API return.
    /// </summary>
    internal static PagedResult<FloodResultItem> ToFloodResultItem(this PagedResult<SearchResultFloodReport>? source, IList<ReportStatus> reportStatuses)
    {
        if (source is null)
        {
            return new PagedResult<FloodResultItem>([], 0, 0, 0, 0);
        }

        var items = source.Results
            .SelectMany(r => r.ToFloodResultItems(reportStatuses))
            .ToList();

        return new PagedResult<FloodResultItem>(items, source.TotalCount, source.PageSize, source.CurrentPage, source.TotalPages);
    }

    /// <summary>
    /// Flattens a single <see cref="SearchResultFloodReport"/> into one <see cref="FloodResultItem"/>
    /// per <see cref="SearchResultFloodReportSource"/>, with <c>Id</c> and <c>RecordStatus</c>
    /// inherited from the parent report.
    /// </summary>
    internal static IEnumerable<FloodResultItem> ToFloodResultItems(this SearchResultFloodReport source, IList<ReportStatus> reportStatuses)
    {
        var statusName = reportStatuses
            .FirstOrDefault(s => s.Id == source.RecordStatusId)?.Text ?? string.Empty;

        return source.SourceRecords.Select(sr => new FloodResultItem
        {
            Id = source.Id,
            RecordStatus = statusName,
            SourceId = sr.Id,
            Reference = sr.Reference,
            ViewUri = sr.ViewUri,
            CreatedUtc = sr.CreatedUtc,
            UpdatedUtc = sr.UpdatedUtc,
            Geom = sr.Geom,
            Easting = sr.Easting,
            Northing = sr.Northing,
            Latitude = sr.Latitude,
            Longitude = sr.Longitude,
            ImpactStartUtc = sr.ImpactStartUtc,
            Causes = sr.Causes,
            Organisations = sr.Organisations,
        });
    }

    /// <summary>
    /// Groups a paged API result into one <see cref="FloodReportTableRow"/> per
    /// <see cref="SearchResultFloodReport"/>, preserving the source records collection
    /// for display in an expandable table.
    /// </summary>
    internal static SearchFloodReportPagedResult<FloodReportTableRow> ToFloodReportTableRows(this SearchFloodReportPagedResult<SearchResultFloodReport>? source, IList<ReportStatus> reportStatuses)
    {
        if (source is null)
        {
            return new SearchFloodReportPagedResult<FloodReportTableRow>([], 0, 0, 0, 0, 0);
        }

        var statusById = reportStatuses
            .GroupBy(s => s.Id)
            .ToDictionary(g => g.Key, g => g.First().Text);

        var items = source.Results
            .Select(r =>
            {
                statusById.TryGetValue(r.RecordStatusId, out var statusName);

                return new FloodReportTableRow
                {
                    Id = r.Id,
                    Reference = r.Reference,
                    RecordStatus = statusName ?? string.Empty,
                    SourceRecords = r.SourceRecords,
                };
            })
            .ToList();

        return new SearchFloodReportPagedResult<FloodReportTableRow>(items, source.TotalCount, source.TotalFloodSourceCount, source.PageSize, source.CurrentPage, source.TotalPages);
    }


    /// <summary>
    /// Maps SearchOptionsViewModel directly to SavedSearch entity.
    /// </summary>
    internal static SavedSearch OptionsViewToSavedSearch(this SearchOptionsViewModel viewModel, string? searchTitle)
    {
        var savedSearch = new SavedSearch
        {
            // Id
            // CreatedAt
            // UpdatedAt
            // UpdatedBy
            SearchTitle = searchTitle ?? string.Empty,
            ResultsFrom = viewModel.FromDate.DateUtc,
            ResultsTo = viewModel.ToDate.DateUtc,
            FloodSources = viewModel.ByFloodSource,
            UPRN = null,
            USRN = null,
            Postcode = viewModel.ByPostcode ?? string.Empty,
            ReporterString = viewModel.ByPersonsName ?? string.Empty,
            Geom = viewModel.ByPolygon,
            PagingSize = null,
            SortBy = string.Empty,
            SortOrder = SortOptions.Ascending,
            LimitByOrganisation = [],
            LimitByStatus = [],
        };

        if (!string.IsNullOrWhiteSpace(viewModel.SearchReference))
        {
            savedSearch.Reference = viewModel.SearchReference.Trim().ToUpperInvariant();
        }

        return savedSearch;
    }

    /// <summary>
    /// Maps SearchResultsViewModel directly to SavedSearch entity.
    /// </summary>
    internal static SavedSearch ResultsViewToSavedSearch(this SearchResultsViewModel viewModel)
    {
        var savedSearch = new SavedSearch
        {
            SearchTitle = viewModel.SearchTitle,
            ResultsFrom = viewModel.FromDate.DateUtc,
            ResultsTo= viewModel.ToDate.DateUtc,
            FloodSources = viewModel.ByFloodSource,
            UPRN = null,
            USRN = null,
            Postcode = viewModel.ByPostcode ?? string.Empty,
            ReporterString = viewModel.ByPersonsName ?? string.Empty,
            Geom = viewModel.ByPolygon,
            PagingSize = null,
            SortBy = viewModel.SortBy,
            SortOrder = viewModel.SortOrder,
            LimitByOrganisation= viewModel.LimitByOrganisation,
            LimitByStatus = viewModel.LimitByStatus,
            ShowResultsAs = viewModel.ShowResultsAs,
        };

        if (!string.IsNullOrWhiteSpace(viewModel.SearchReference))
        {
            savedSearch.Reference = viewModel.SearchReference.Trim().ToUpperInvariant();
        }

        return savedSearch;
    }


    internal static List<MapMarkerData>? ToMarkers(this IEnumerable<FloodResultItem> floodResults)
    {
        return [.. floodResults
            .Where(r => r.Latitude != 0 || r.Longitude != 0)
            .Select(r => new MapMarkerData(
                r.Id.ToString(),
                r.Latitude,
                r.Longitude,
                r.BuildPopupHtml())
            )];
    }

    internal static string BuildPopupHtml(this FloodResultItem report)
    {
        var sb = new StringBuilder();
        sb.Append($"<div class=\"govuk-body-s govuk-!-margin-bottom-0\"><strong>{WebUtility.HtmlEncode(report.Reference)}</strong><br/>");
        sb.Append($"<strong>Report status:</strong> {WebUtility.HtmlEncode(report.RecordStatus)}<br/>");

        if (report.ImpactStartUtc is not null)
        {
            sb.Append($"<strong>Flooding started:</strong> {report.ImpactStartUtc.Value:d MMMM yyyy}<br/>");
        }
        var causes = report.Causes.Count > 0
            ? string.Join(", ", report.Causes.Select(s => WebUtility.HtmlEncode(s.Name)))
            : "No flooding causes identified";
        sb.Append($"<strong>Causes:</strong> {causes}<br/>");
        sb.Append($"<strong>Created:</strong> {report.CreatedUtc:d MMMM yyyy}<br/>");

        if (report.UpdatedUtc is not null)
        {
            sb.Append($"<strong>Updated:</strong> {report.UpdatedUtc.Value:d MMMM yyyy}<br/>");
        }

        if (report.ViewUri.IsAbsoluteUri
            && (string.Equals(report.ViewUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(report.ViewUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            sb.Append($"<a href=\"{WebUtility.HtmlEncode(report.ViewUri.ToString())}\" target=\"_blank\" rel=\"noopener noreferrer\" class=\"govuk-link\">View details</a>");
        }
        sb.Append("</div>");

        return sb.ToString();
    }
}