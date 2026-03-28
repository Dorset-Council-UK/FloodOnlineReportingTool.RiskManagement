namespace FloodOnlineReportingTool.RiskManagement.Models.Event;
public class EventDetailsViewModel : EventBaseModel
{
    public string EstimatedVulnerableString { get; set; } = "0";
    public string EstimatedUninhabitableString { get; set; } = "0";
    public string EstimatedTotalExternalOnlyString { get; set; } = "0";
    public string EstimatedTotalInternallyFloodedString { get; set; } = "0";
    public int EstimatedDisruptedUtilities { get; set; } = 0;
    public string EstimatedDisruptedUtilitiesString { get; set; } = "0";
}
