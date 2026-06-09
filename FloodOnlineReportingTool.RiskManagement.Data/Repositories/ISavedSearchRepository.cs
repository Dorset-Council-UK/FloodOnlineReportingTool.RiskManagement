using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public interface ISavedSearchRepository
{
    /// <summary>
    /// Count the number of saved searches
    /// </summary>
    Task<int> Count(CancellationToken cancellationToken);

    /// <summary>
    /// Does the saved search exist using the ID
    /// </summary>
    Task<bool> Exists(Guid savedSearchId, CancellationToken cancellationToken);

    /// <summary>
    /// Does the saved search exist using the reference
    /// </summary>
    Task<bool> Exists(string reference, CancellationToken cancellationToken);

    /// <summary>
    /// Get a saved search using the ID
    /// </summary>
    Task<SavedSearch?> Get(Guid savedSearchId, CancellationToken cancellationToken);

    /// <summary>
    /// Get a saved search using the reference
    /// </summary>
    Task<SavedSearch?> Get(string reference, CancellationToken cancellationToken);

    /// <summary>
    /// Create a saved search
    /// </summary>
    /// <returns>A result pattern with the new saved search, or a list of errors.</returns>
    Task<Result<SavedSearch?>> Create(SavedSearch searchCriteria, CancellationToken cancellationToken);

    /// <summary>
    /// Update the saved search using the ID
    /// </summary>
    /// <returns>A result pattern with the updated saved search, or a list of errors.</returns>
    Task<Result<SavedSearch?>> Update(Guid savedSearchId, SavedSearch searchCriteria, CancellationToken cancellationToken);

    /// <summary>
    /// Update the saved search using the reference
    /// </summary>
    /// <returns>A result pattern with the updated saved search, or a list of errors.</returns>
    Task<Result<SavedSearch?>> Update(string reference, SavedSearch searchCriteria, CancellationToken cancellationToken);

    Task<Result<SearchRedirect?>> FindLocalByReference(string reference, CancellationToken cancellationToken);
}
