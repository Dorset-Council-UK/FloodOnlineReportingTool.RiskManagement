using System.Diagnostics.CodeAnalysis;

namespace FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;

public record Result<T>
{
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; init; }

    public T? Value { get; init; }

    public ICollection<string> Errors { get; init; } = [];

    private Result(bool isSuccess, T? value, ICollection<string> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    internal static Result<T> Success(T value) => new(isSuccess: true, value, []);

    internal static Result<T> Failure(ICollection<string> errors) => new(isSuccess: false, value: default, errors);
}
