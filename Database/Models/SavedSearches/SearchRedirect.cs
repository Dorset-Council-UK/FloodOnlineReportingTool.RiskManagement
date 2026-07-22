using FloodOnlineReportingTool.Database.Models.Enums;

namespace FloodOnlineReportingTool.Database.Models.SavedSearches;

public record SearchRedirect
{
    public Guid? Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public RecordTypes RedirectType { get; set; } = RecordTypes.Search;
}
