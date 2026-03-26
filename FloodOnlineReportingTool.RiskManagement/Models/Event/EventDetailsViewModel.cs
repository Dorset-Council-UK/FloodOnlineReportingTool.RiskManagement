using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.RiskManagement.Models.Search;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;
public class EventDetailsViewModel : IEvent
{
    // Common search properties (always required)
    public string SearchReference { get; set; } = string.Empty;
    public string SearchTitle { get; set; } = string.Empty;

    // Common event properties (always required)
    public Guid RecordStatus { get; set; } = EventStatusIds.RecoveryPhase;
    public Guid FloodStatus { get; set; } = EventStatusIds.PropertiesAffected;
    public Guid InvestigationStatus { get; set; } = Section19StatusIds.NoSection19;
    public int EstimatedVulnerable { get; set; } = 0;
    public string? EstimatedVulnerableString { get; set; }
    public int EstimatedUninhabitable { get; set; } = 0;
    public string? EstimatedUninhabitableString { get; set; }
    public int EstimatedTotalExternalOnly { get; set; } = 0;
    public string? EstimatedTotalExternalOnlyString { get; set; }
    public int EstimatedTotalInternallyFlooded { get; set; } = 0;
    public string? EstimatedTotalInternallyFloodedString { get; set; }
    public string? StatusNote { get; set; } = string.Empty;

    public EventDetailsViewModel ShallowCopy()
    {
        return (EventDetailsViewModel)this.MemberwiseClone();
    }
}
