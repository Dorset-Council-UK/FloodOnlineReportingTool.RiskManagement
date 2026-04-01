using FloodOnlineReportingTool.RiskManagement.Data.Models.Helpers;

namespace FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;

public record InvestigationStatus : IOption
{
    public InvestigationStatus(Guid id, string text, int order)
    {
        Id = id;
        Text = text;
        Order = order;
    }

    public InvestigationStatus(Guid id, string text, string description, int order)
    {
        Id = id;
        Text = text;
        Description = description;
        Order = order;
    }

    public Guid Id { get; init; }
    public string? Text { get; init; } // Was TypeName
    public string? Description { get; init; } // Was TypeDescription
    public int Order { get; init; } // Was OptionOrder
}
