using System.Diagnostics.CodeAnalysis;

namespace FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;

public record Result<T>
{
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; init; }

    public T? Value { get; init; }

    public ResultSeverity Severity { get; init; } = ResultSeverity.None;

    public ICollection<string> Errors { get; init; } = [];

    private Result() { }

    internal static Result<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value,
    };

    internal static Result<T> Failure(ICollection<string> errors) => Failure(ResultSeverity.Error, errors);

    internal static Result<T> Failure(ResultSeverity severity, ICollection<string> errors) => new()
    {
        IsSuccess = false,
        Value = default,
        Severity = severity,
        Errors = errors,
    };
}
