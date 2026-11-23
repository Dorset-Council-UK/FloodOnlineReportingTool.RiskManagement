using FloodOnlineReportingTool.Database.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Models.Order;

namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

/// <summary>
/// Shared presentation helpers for <see cref="OmniboxSearchResult"/> so the home page
/// (single-result navigation) and the omnibox results page (multi-result listing) do
/// not duplicate the record-type-to-route/label mapping.
/// </summary>
public static class OmniboxSearchResultExtensions
{
    public static string GetUrl(this OmniboxSearchResult result)
    {
        return result.ResultType switch
        {
            RecordTypes.Event => $"{EventPages.View.Url}/{Uri.EscapeDataString(result.Reference)}",
            RecordTypes.Search => $"{SearchPages.Results.Url}/{Uri.EscapeDataString(result.Reference)}",
            RecordTypes.Report => $"{ReportPages.Details.Url}/{Uri.EscapeDataString(result.Reference)}",
            _ => GeneralPages.Home.Url,
        };
    }

    public static string GetTypeLabel(this OmniboxSearchResult result)
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
