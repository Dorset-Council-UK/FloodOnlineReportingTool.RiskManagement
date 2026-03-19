using FloodOnlineReportingTool.Contracts.Shared;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;
public class CreateEventViewModel : IEvent
{
    // Common search properties (always required)
    public string SearchReference { get; set; } = string.Empty;
    public string SearchTitle { get; set; } = string.Empty;

    // Common event properties (always required)
    public Guid RecordStatus { get; set; } = EventStatusIds.RecoveryPhase;
    public Guid FloodStatus { get; set; } = EventStatusIds.PropertiesAffected;
    public Guid InvestigationStatus { get; set; } = Section19StatusIds.NoSection19;
    public int EstimatedVulnerable { get; set; } = 0;
    public int EstimatedUninhabitable { get; set; } = 0;
    public int EstimatedTotalExternalOnly { get; set; } = 0;
    public int EstimatedTotalInternallyFlooded { get; set; } = 0;
    public string? StatusNote { get; set; } = string.Empty;

    public string? EstimatedVulnerableString { get; set; }
    public string? EstimatedUninhabitableString { get; set; }
    public string? EstimatedTotalExternalOnlyString { get; set; }
    public string? EstimatedTotalInternallyFloodedString { get; set; }

    public bool TriggerInvestigation { get; set; } = true;
}
