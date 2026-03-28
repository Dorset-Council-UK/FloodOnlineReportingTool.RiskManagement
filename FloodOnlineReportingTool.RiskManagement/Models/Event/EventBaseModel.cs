using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;

public abstract class EventBaseModel
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
    public int EstimatedDisruptedUtilities { get; set; } = 0;
    public string? StatusNote { get; set; }

    public FloodEvent ToFloodEvent(EventDetailsViewModel details, EventEstimatesViewModel? estimates)
    {
        return new FloodEvent
        {
            MitigationTaken = ConditionalOptions.No,
            RecordStatus = details.RecordStatus,
            FloodStatus = details.FloodStatus,
            InvestigationStatus = details.InvestigationStatus,
            EstimatedVulnerable = details.EstimatedVulnerable,
            EstimatedUninhabitable = details.EstimatedUninhabitable,
            EstimatedTotalExternalOnly = details.EstimatedTotalExternalOnly,
            EstimatedTotalInternallyFlooded = details.EstimatedTotalInternallyFlooded,
            EstimatedDisruptedUtilities = details.EstimatedDisruptedUtilities,
            StatusNote = details.StatusNote ?? string.Empty,
            FloodExtentConfidence = estimates?.FloodExtentConfidence ?? ConfidenceOptions.Unknown,
            EstimatedNoElectric = estimates?.EstimatedNoElectric ?? 0,
            EstimatedNoGas = estimates?.EstimatedNoGas ?? 0,
            EstimatedNoFoul = estimates?.EstimatedNoFoul ?? 0,
            EstimatedNoWaterSupply = estimates?.EstimatedNoWaterSupply ?? 0,
            EstimatedResidentialAffected = estimates?.EstimatedResidentialAffected ?? 0,
            EstimatedResidentialFlooded = estimates?.EstimatedResidentialFlooded ?? 0,
            EstimatedResidentialOther = estimates?.EstimatedResidentialOther ?? 0,
            EstimatedCommercialAffected = estimates?.EstimatedCommercialAffected ?? 0,
            EstimatedCommercialFlooded = estimates?.EstimatedCommercialFlooded ?? 0,
            EstimatedCommercialOther = estimates?.EstimatedCommercialOther ?? 0
            
        };
    } 
}
