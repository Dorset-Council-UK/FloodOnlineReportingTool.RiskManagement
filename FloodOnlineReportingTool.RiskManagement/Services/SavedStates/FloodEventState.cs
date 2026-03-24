using FloodOnlineReportingTool.RiskManagement.Models.Event;

namespace FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
public class FloodEventState : IFloodEventState
{

    public EventDetailsViewModel? Details { get; private set; }
    public EventEstimatesViewModel? Estimates { get; private set; }
    public bool HasDetails => Details != null;
    public bool HasEstimates => Estimates != null;
    public void SetEventDetails(EventDetailsViewModel details)
    {
        Details = details;
    }

    public void SetEventEstimates(EventEstimatesViewModel estimates)
    {
        Estimates = estimates;
    }
}
