using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;

public class ChangeEventViewModel : IEvent
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

    // Property estimates
    public int? EstimatedResidentialAffected { get; set; }
    public string? EstimatedResidentialAffectedString { get; set; }
    public int? EstimatedResidentialFlooded { get; set; }
    public string? EstimatedResidentialFloodedString { get; set; }
    public int? EstimatedResidentialOther { get; set; }
    public string? EstimatedResidentialOtherString { get; set; }
    public int? EstimatedCommercialAffected { get; set; }
    public string? EstimatedCommercialAffectedString { get; set; }
    public int? EstimatedCommercialFlooded { get; set; }
    public string? EstimatedCommercialFloodedString { get; set; }
    public int? EstimatedCommercialOther { get; set; }
    public string? EstimatedCommercialOtherString { get; set; }

    // Utility estimates
    public int? EstimatedNoElectric { get; set; }
    public string? EstimatedNoElectricString { get; set; }
    public int? EstimatedNoGas { get; set; }
    public string? EstimatedNoGasString { get; set; }
    public int? EstimatedNoFoul { get; set; }
    public string? EstimatedNoFoulString { get; set; }
    public int? EstimatedNoWaterSupply { get; set; }
    public string? EstimatedNoWaterSupplyString { get; set; }
    public int? EstimatedNoPhone { get; set; }
    public string? EstimatedNoPhoneString { get; set; }
    public int? EstimatedDisruptedUtilities { get; set; }
    public string? EstimatedDisruptedUtilitiesString { get; set; }

    // Other properties
    public bool TriggerInvestigation { get; set; } = true;
    public ConfidenceOptions? FloodExtentConfidence { get; set; }

}
