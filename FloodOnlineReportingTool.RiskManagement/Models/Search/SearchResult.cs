namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public record SearchResult<T>(bool Succeeded, T Items, ICollection<string> Errors)
{
    internal static SearchResult<T> Success(T items) => new(Succeeded: true, items, []);

    internal static SearchResult<T> Failure(ICollection<string> errors) => new(Succeeded: false, default!, errors);
}