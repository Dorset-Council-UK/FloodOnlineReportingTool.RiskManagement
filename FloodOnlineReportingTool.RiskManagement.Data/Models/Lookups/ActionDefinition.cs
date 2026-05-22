using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;

namespace FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;

public record ActionDefinition
{
    public ActionDefinition(Guid id, string name, string description, ActionTypes actionType)
    {
        Id = id; 
        Name = name; 
        Description = description; 
        ActionType = actionType;
    }

    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public ActionTypes ActionType { get; init; }
}
