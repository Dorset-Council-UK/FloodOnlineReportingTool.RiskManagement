using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;

namespace FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;

public record SearchRedirect
{
    public Guid? Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public RecordTypes RedirectType { get; set; } = RecordTypes.Search;
}
