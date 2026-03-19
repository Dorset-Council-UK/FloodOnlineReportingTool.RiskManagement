namespace FloodOnlineReportingTool.RiskManagement.Models.Event;
public interface IEvent
{
    // Common search properties (always required)
    public string SearchReference { get; set; }
    public string SearchTitle { get; set; }

    // Common event properties (always required)
    public Guid RecordStatus { get; set; }
    public Guid FloodStatus { get; set; }
    public Guid InvestigationStatus { get; set; }
    public int EstimatedVulnerable { get; set; }
    public int EstimatedUninhabitable { get; set; }
    public int EstimatedTotalExternalOnly { get; set; }
    public int EstimatedTotalInternallyFlooded { get; set; }
    public string? StatusNote { get; set; }
}
