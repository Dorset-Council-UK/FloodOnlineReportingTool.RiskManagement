using System.Diagnostics.CodeAnalysis;

namespace FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;

internal record SavedSearchDuplicateCheck
{
    [MemberNotNullWhen(true, [nameof(UpdatedBy), nameof(SavedSearchId), nameof(SavedSearchReference)])]
    internal bool IsDuplicate { get; init; }

    internal DateTimeOffset? LastUpdated { get; init; }

    internal string? UpdatedBy { get; init; }

    internal Guid? SavedSearchId { get; init; }

    internal string? SavedSearchReference { get; init; }

    private SavedSearchDuplicateCheck(bool isDuplicate)
    {
        IsDuplicate = isDuplicate;
    }

    private SavedSearchDuplicateCheck(DateTimeOffset? lastUpdated, string? updatedBy, Guid savedSearchId, string? savedSearchReference)
    {
        const string Unknown = "Unknown";

        IsDuplicate = true;
        LastUpdated = lastUpdated; // can still be null sometimes (hopefully not)
        UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? Unknown : updatedBy;
        SavedSearchId = savedSearchId;
        SavedSearchReference = string.IsNullOrWhiteSpace(savedSearchReference) ? Unknown : savedSearchReference;
    }

    internal static SavedSearchDuplicateCheck Duplicate(SavedSearch savedSearch) => new(savedSearch.UpdatedAt, savedSearch.UpdatedBy, savedSearch.Id, savedSearch.Reference);

    internal static SavedSearchDuplicateCheck NotDuplicate() => new(isDuplicate: false);
}
