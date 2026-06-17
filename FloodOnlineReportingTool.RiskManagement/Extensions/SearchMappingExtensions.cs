using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Models.Search;

namespace FloodOnlineReportingTool.RiskManagement.Extensions;

internal static class SearchMappingExtensions
{
    /// <summary>
    /// Creates a PagedResult from API return.
    /// </summary>
    internal static PagedResult<FloodResultItem>? ToFloodResultItem(this PagedResult<SearchResultFloodReport>? source, IList<ReportStatus> reportStatuses)
    {
        if (source is null)
        {
            return null;
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
    /// Maps SearchOptionsViewModel directly to SavedSearch entity.
    /// </summary>
    internal static SavedSearch OptionsViewToSavedSearch(this SearchOptionsViewModel viewModel, string? searchTitle)
    {
        return new SavedSearch
        {
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

        if (!string.IsNullOrWhiteSpace(viewModel.ByReference))
        {
            savedSearch.Reference = viewModel.ByReference.Trim().ToUpperInvariant();
        }

        return savedSearch;
    }
}