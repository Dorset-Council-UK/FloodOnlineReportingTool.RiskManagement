using FloodOnlineReportingTool.Database.Models.SavedSearches;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface ILinkedFloodReportResolver
{
    Task<IReadOnlyCollection<string>> GetLinkedFloodReportReferences(SavedSearch savedSearch, CancellationToken cancellationToken);
}
