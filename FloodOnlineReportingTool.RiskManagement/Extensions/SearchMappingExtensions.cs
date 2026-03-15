using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Extensions;

internal static class SearchMappingExtensions
{
    /// <summary>
    /// Maps SearchOptionsViewModel directly to SavedSearch entity.
    /// </summary>
    internal static SavedSearch ToSavedSearch(this SearchOptionsViewModel viewModel, string searchTitle)
    {
        return new SavedSearch
        {
            SearchTitle = searchTitle,
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
    /// Maps SavedSearch entity to SearchOptionsViewModel.
    /// </summary>
    internal static SearchOptionsViewModel ToViewModel(this SavedSearch savedSearch)
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

    private static DateTimeOffset? ConvertToDateTimeOffset(GdsDate gdsDate)
    {
        if (int.TryParse(gdsDate.YearText, out int year) &&
            int.TryParse(gdsDate.MonthText, out int month) &&
            int.TryParse(gdsDate.DayText, out int day))
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
            DayText = dt.Day.ToString(),
            MonthText = dt.Month.ToString(),
            YearText = dt.Year.ToString()
        };
    }
}