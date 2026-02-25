namespace FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;

public record LkCommentType
{
    public LkCommentType(int id, string text, int order)
    {
        Id = id;
        Text = text;
        Order = order;
    }

    public LkCommentType(int id, string text, int order, string description)
    {
        Id = id;
        Text = text;
        Order = order;
        Description = description;
    }

    public int Id { get; init; }
    public string? Text { get; init; } // Was TypeName
    public string? Description { get; init; } // Was TypeDescription
    public int Order { get; init; } // Was OptionOrder
}
