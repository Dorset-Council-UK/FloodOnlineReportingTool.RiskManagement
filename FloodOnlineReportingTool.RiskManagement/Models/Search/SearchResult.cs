namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public record SearchResult<T>(bool Succeeded, T Results, ICollection<string> Errors)
{
    internal static SearchResult<T> Success(T results) => new(Succeeded: true, results, []);

    internal static SearchResult<T> Failure(ICollection<string> errors) => new(Succeeded: false, default!, errors);
}