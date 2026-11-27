using FloodOnlineReportingTool.Database.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;

namespace FloodOnlineReportingTool.RiskManagement.Extensions;

/// <summary>
/// Shared presentation helpers for <see cref="OmniboxSearchResult"/> so the home page
/// (single-result navigation) and the omnibox results page (multi-result listing) do
/// not duplicate the record-type-to-route/label mapping.
/// </summary>
internal static class OmniboxSearchResultExtensions
{
    extension(OmniboxSearchResult result)
    {

        internal string GetUrl()
        {
            return result.ResultType switch
            {
                RecordTypes.Event => $"{EventPages.View.Url}/{Uri.EscapeDataString(result.Reference)}",
                RecordTypes.Search => $"{SearchPages.Results.Url}/{Uri.EscapeDataString(result.Reference)}",
                RecordTypes.Report => $"{ReportPages.Details.Url}/{Uri.EscapeDataString(result.Reference)}",
                _ => GeneralPages.Home.Url,
            };
        }

        internal string GetTypeLabel()
        {
            return result.ResultType switch
            {
                RecordTypes.Event => "Flood event",
                RecordTypes.Search => "Saved search",
                RecordTypes.Report => "Flood report",
                _ => "Record",
            };
        }
    }
}
