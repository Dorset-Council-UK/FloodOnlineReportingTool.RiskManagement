using FloodOnlineReportingTool.RiskManagement.Models.Search;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;

internal static class SavedSearchExtensions
{
    extension(SavedSearch savedSearch)
    {
        /// <summary>
        /// Converts the SavedSearch entity to a SearchOptionsViewModel
        /// </summary>
        internal SearchOptionsViewModel ToSearchOptionsViewModel()
        {
            return new SearchOptionsViewModel
            {
                FromDate = savedSearch.ResultsFrom.HasValue ? new GdsDate(savedSearch.ResultsFrom.Value) : new GdsDate(),
                ToDate = savedSearch.ResultsTo.HasValue ? new GdsDate(savedSearch.ResultsTo.Value) : new GdsDate(),
                ByFloodSource = [.. savedSearch.FloodSources],
                ByPostcode = savedSearch.Postcode,
                ByPersonsName = savedSearch.ReporterString,
                ByPolygon = savedSearch.Geom,
                IsSavedSearchLocked = savedSearch.IsLocked,
            };
        }

        /// <summary>
        /// Converts the SavedSearch entity to a SearchResultsViewModel
        /// </summary>
        internal SearchResultsViewModel ToSearchResultsViewModel()
        {
            return new SearchResultsViewModel
            {
                Id = savedSearch.Id,
                SearchTitle = savedSearch.SearchTitle,
                ByReference = savedSearch.Reference,
                FromDate = savedSearch.ResultsFrom.HasValue ? new GdsDate(savedSearch.ResultsFrom.Value) : new GdsDate(),
                ToDate = savedSearch.ResultsTo.HasValue ? new GdsDate(savedSearch.ResultsTo.Value) : new GdsDate(),
                ByFloodSource = [.. savedSearch.FloodSources],
                ByPostcode = savedSearch.Postcode,
                ByPersonsName = savedSearch.ReporterString,
                ByPolygon = savedSearch.Geom,
                ShowResultsAs = savedSearch.ShowResultsAs,
                PagingSize = savedSearch.PagingSize ?? 10,
                SortBy = savedSearch.SortBy,
                SortOrder = savedSearch.SortOrder,
                LimitByOrganisation = savedSearch.LimitByOrganisation,
                LimitByStatus = savedSearch.LimitByStatus,
                IsSavedSearchLocked = savedSearch.IsLocked,
            };
        }
    }
}
