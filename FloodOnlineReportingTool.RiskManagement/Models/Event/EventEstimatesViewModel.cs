using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Models.Search;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;

public class EventEstimatesViewModel
{
    // Common search properties (always required)
    public string SearchReference { get; set; } = string.Empty;
    // These two are set on the details page but included in this model for reference
    // as they may need updating. For example, if properties flooded exceeds the internally flooded 
    // estimate we can offer to update the total on save".
    public int EstimatedTotalInternallyFlooded { get; set; } = 0;
    public int EstimatedTotalExternalOnly { get; set; } = 0;

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
    public int? EstimatedDisruptedUtilities { get; set; }
    public string? EstimatedDisruptedUtilitiesString { get; set; }

    // Other properties
    public ConfidenceOptions? FloodExtentConfidence { get; set; }

    public EventEstimatesViewModel ShallowCopy()
    {
        return (EventEstimatesViewModel)this.MemberwiseClone();
    }

}
