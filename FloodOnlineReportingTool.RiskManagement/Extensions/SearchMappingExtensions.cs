using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using GdsBlazorComponents;
using System.Globalization;

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
            Organisations = sr.Organisations
        });
    }

    /// <summary>
    /// Groups a paged API result into one <see cref="FloodReportTableRow"/> per
    /// <see cref="SearchResultFloodReport"/>, preserving the source records collection
    /// for display in an expandable table.
    /// </summary>
    internal static PagedResult<FloodReportTableRow>? ToFloodReportTableRows(this PagedResult<SearchResultFloodReport>? source, IList<ReportStatus> reportStatuses)
    {
        if (source is null)
        {
            return null;
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

        return new PagedResult<FloodReportTableRow>(items, source.TotalCount, source.PageSize, source.CurrentPage, source.TotalPages);
    }


    /// <summary>
    /// Maps SearchOptionsViewModel directly to SavedSearch entity.
    /// </summary>
    internal static SavedSearch OptionsViewToSavedSearch(this SearchOptionsViewModel viewModel, string? searchTitle)
    {
        return new SavedSearch
        {
            SearchTitle = searchTitle ?? string.Empty,
            ResultsFrom = ConvertToDateTimeOffset(viewModel.FromDate),
            ResultsTo = ConvertToDateTimeOffset(viewModel.ToDate),
            FloodSources = viewModel.ByFloodSource ?? [],
            UPRN = null,
            USRN = null,
            Postcode = viewModel.ByPostcode ?? string.Empty,
            ReporterString = viewModel.ByPersonsName ?? string.Empty,
            Geom = viewModel.ByPolygon,
            PagingSize = null,
            SortBy = string.Empty,
            SortOrder = SortOptions.Ascending,
            LimitByOrganisation = [],
            LimitByStatus = []
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
            ResultsFrom = ConvertToDateTimeOffset(viewModel.FromDate),
            ResultsTo= ConvertToDateTimeOffset(viewModel.ToDate),
            FloodSources = viewModel.ByFloodSource ?? [],
            UPRN = null,
            USRN = null,
            Postcode = viewModel.ByPostcode ?? string.Empty,
            ReporterString = viewModel.ByPersonsName ?? string.Empty,
            Geom = viewModel.ByPolygon,
            PagingSize = null,
            SortBy = viewModel.SortBy,
            SortOrder = viewModel.SortOrder,
            LimitByOrganisation= viewModel.LimitByOrganisation ?? [],
            LimitByStatus = viewModel.LimitByStatus ?? [],
            ShowResultsAs = viewModel.ShowResultsAs
        };

        if (!string.IsNullOrWhiteSpace(viewModel.ByReference))
        {
            savedSearch.Reference = viewModel.ByReference.Trim().ToUpperInvariant();
        }

        return savedSearch;
    }

    /// <summary>
    /// Maps SavedSearch entity to SearchOptionsViewModel.
    /// </summary>
    internal static SearchOptionsViewModel SavedSearchToOptionsView(this SavedSearch savedSearch)
    {
        return new SearchOptionsViewModel
        {
            FromDate = ConvertToGdsDate(savedSearch.ResultsFrom),
            ToDate = ConvertToGdsDate(savedSearch.ResultsTo),
            ByFloodSource = savedSearch.FloodSources.ToList(),
            ByPostcode = savedSearch.Postcode,
            ByPersonsName = savedSearch.ReporterString,
            ByPolygon = savedSearch.Geom
        };
    }

    /// <summary>
    /// Maps SavedSearch entity to SearchResultsViewModel.
    /// </summary>
    internal static SearchResultsViewModel SavedSearchToResultsView(this SavedSearch savedSearch)
    {
        return new SearchResultsViewModel {
            Id = savedSearch.Id,
            SearchTitle = savedSearch.SearchTitle,
            ByReference = savedSearch.Reference,
            FromDate = ConvertToGdsDate(savedSearch.ResultsFrom),
            ToDate = ConvertToGdsDate(savedSearch.ResultsTo),
            ByFloodSource = savedSearch.FloodSources.ToList(),
            ByPostcode = savedSearch.Postcode,
            ByPersonsName = savedSearch.ReporterString,
            ByPolygon = savedSearch.Geom,
            ShowResultsAs= savedSearch.ShowResultsAs,
            PagingSize = savedSearch.PagingSize ?? 10,
            SortBy = savedSearch.SortBy,
            SortOrder = savedSearch.SortOrder,
            LimitByOrganisation = savedSearch.LimitByOrganisation,
            LimitByStatus = savedSearch.LimitByStatus,
        };
    }

    private static DateTimeOffset? ConvertToDateTimeOffset(GdsDate gdsDate)
    {
        if (int.TryParse(gdsDate.YearText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int year) &&
            int.TryParse(gdsDate.MonthText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int month) &&
            int.TryParse(gdsDate.DayText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int day))
        {
            try
            {
                return new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private static GdsDate ConvertToGdsDate(DateTimeOffset? dateTimeOffset)
    {
        if (dateTimeOffset == null) return new GdsDate();

        var dt = dateTimeOffset.Value;
        return new GdsDate
        {
            DayText = dt.Day.ToString(CultureInfo.InvariantCulture),
            MonthText = dt.Month.ToString(CultureInfo.InvariantCulture),
            YearText = dt.Year.ToString(CultureInfo.InvariantCulture)
        };
    }
}