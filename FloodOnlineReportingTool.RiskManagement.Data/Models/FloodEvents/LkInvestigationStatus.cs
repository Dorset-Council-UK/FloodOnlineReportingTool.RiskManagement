namespace FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;

public record LkInvestigationStatus
{
    public LkInvestigationStatus(Guid id, string text, int order)
    {
        Id = id;
        Text = text;
        Order = order;
    }

    public LkInvestigationStatus(Guid id, string text, string description, int order)
    {
        Id = id;
        Text = text;
        Description = description;
        Order = order;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string? Text { get; init; } // Was TypeName
    public string? Description { get; init; } // Was TypeDescription
    public int Order { get; init; } // Was OptionOrder
}
