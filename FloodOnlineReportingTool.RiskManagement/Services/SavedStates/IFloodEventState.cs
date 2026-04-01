using FloodOnlineReportingTool.RiskManagement.Models.Event;

namespace FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
public interface IFloodEventState
{
    EventDetailsViewModel? Details { get; }
    EventEstimatesViewModel? Estimates { get; }
    bool HasDetails { get; }
    bool HasEstimates { get; }
    void SetEventDetails(EventDetailsViewModel? details);
    void SetEventEstimates(EventEstimatesViewModel? estimates);
}
