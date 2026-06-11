using System.Diagnostics.CodeAnalysis;

namespace FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;

internal record SavedSearchDuplicateCheck
{
    [MemberNotNullWhen(true, nameof(UpdatedBy), nameof(SavedSearchId), nameof(SavedSearchReference))]
    internal bool IsDuplicate { get; init; }

    internal DateTimeOffset? LastUpdated { get; init; }

    internal string? UpdatedBy { get; init; }

    internal Guid? SavedSearchId { get; init; }

    internal string? SavedSearchReference { get; init; }

    private SavedSearchDuplicateCheck() {}

    internal static SavedSearchDuplicateCheck Duplicate(SavedSearch savedSearch)
    {
        const string Unknown = "Unknown";

        return new()
        {
            IsDuplicate = true,
            LastUpdated = savedSearch.UpdatedAt, // can still be null sometimes (hopefully not)
            UpdatedBy = string.IsNullOrWhiteSpace(savedSearch.UpdatedBy) ? Unknown : savedSearch.UpdatedBy,
            SavedSearchId = savedSearch.Id,
            SavedSearchReference = string.IsNullOrWhiteSpace(savedSearch.Reference) ? Unknown : savedSearch.Reference,
        };
    }

    internal static SavedSearchDuplicateCheck NotDuplicate()
    {
        return new()
        {
            IsDuplicate = false,
            LastUpdated = null,
            UpdatedBy = null,
            SavedSearchId = null,
            SavedSearchReference = null,
        };
    }
}
