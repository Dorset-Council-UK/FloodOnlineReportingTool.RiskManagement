namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

/// <summary>
/// Represents the result of a search operation, including its state, results, and any errors.
/// </summary>
/// <typeparam name="T">The type of the search results.</typeparam>
public record SearchResult<T>(SearchState State, T Results, ICollection<string> Errors)
{
    /// <summary>Creates a search result indicating the search has not started.</summary>
    public static SearchResult<T> NotStarted() => new(SearchState.NotStarted, default!, []);

    /// <summary>Creates a search result indicating the search is in progress.</summary>
    public static SearchResult<T> Searching() => new(SearchState.Searching, default!, []);

    /// <summary>Creates a search result indicating results were found.</summary>
    public static SearchResult<T> ResultsFound(T results) => new(SearchState.ResultsFound, results, []);

    /// <summary>Creates a search result indicating no results were found.</summary>
    public static SearchResult<T> NoResults() => new(SearchState.NoResults, default!, []);

    /// <summary>Creates a search result indicating an error occurred.</summary>
    public static SearchResult<T> Error(ICollection<string> errors) => new(SearchState.Error, default!, errors);
}