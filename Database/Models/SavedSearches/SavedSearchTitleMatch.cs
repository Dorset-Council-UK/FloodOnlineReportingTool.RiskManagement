namespace FloodOnlineReportingTool.Database.Models.SavedSearches;

public sealed record SavedSearchTitleMatch(
    Guid Id,
    string Reference,
    string SearchTitle,
    Guid? FloodEventId);
