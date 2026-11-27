using FloodOnlineReportingTool.Database.Models.Enums;

namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public sealed record OmniboxSearchResult(
    Guid Id,
    string Reference,
    string Title,
    RecordTypes ResultType);
