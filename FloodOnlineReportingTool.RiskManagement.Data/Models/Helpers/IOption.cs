namespace FloodOnlineReportingTool.Database.Models.Helpers;

public interface IOption
{
    public Guid Id { get; init; }
    public string? Text { get; init; } // Was TypeName
}

