namespace FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;

public record LkEventStatus
{
    public LkEventStatus(Guid id, string text, int order)
    {
        Id = id;
        Text = text;
        Order = order;
    }

    public LkEventStatus(Guid id, string text, int order, string description)
    {
        Id = id;
        Text = text;
        Order = order;
        Description = description;
    }

    public Guid Id { get; init; }
    public string? Text { get; init; } // Was TypeName
    public string? Description { get; init; } // Was TypeDescription
    public int Order { get; init; } // Was OptionOrder
}
