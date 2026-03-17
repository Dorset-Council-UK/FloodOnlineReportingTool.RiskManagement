using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using GdsBlazorComponents;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Extensions;

internal static class SearchMappingExtensions
{
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
            FloodPrimaryCauses = viewModel.ByFloodCause ?? [],
            FloodSecondaryCauses = [],
            UPRN = null,
            USRN = null,
            Postcode = viewModel.ByPlaceName ?? string.Empty,
            ReporterString = viewModel.ByPersonsName ?? string.Empty,
            Geom = viewModel.ByPolygon,
            ShowResultsAs = viewModel.ShowResultsAs,
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
            FloodPrimaryCauses = viewModel.ByFloodCause ?? [],
            FloodSecondaryCauses = [],
            UPRN = null,
            USRN = null,
            Postcode = viewModel.ByPlaceName ?? string.Empty,
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
            savedSearch.Reference = viewModel.ByReference;
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
            ByFloodCause = savedSearch.FloodPrimaryCauses.ToList(),
            ByPlaceName = savedSearch.Postcode,
            ByPersonsName = savedSearch.ReporterString,
            ByPolygon = savedSearch.Geom,
            ShowResultsAs = savedSearch.ShowResultsAs,
            Buffer = null
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
            ByFloodCause = savedSearch.FloodPrimaryCauses.ToList(),
            ByPlaceName = savedSearch.Postcode,
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