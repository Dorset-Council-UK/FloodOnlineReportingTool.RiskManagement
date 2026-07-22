namespace FloodOnlineReportingTool.Database.Models.ResultModels;

public record GetResult<T>(bool IsSuccess, T? ResultModel, ICollection<string> Errors)
{
    public static GetResult<T> Success(T resultModel) => new(IsSuccess: true, resultModel, []);
    public static GetResult<T> Failure(ICollection<string> errors) => new(IsSuccess: false, ResultModel: default, errors);
}
