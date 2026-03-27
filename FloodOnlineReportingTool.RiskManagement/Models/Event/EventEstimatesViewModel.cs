using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Models.Search;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;

public class EventEstimatesViewModel
{
    // Common search properties (always required)
    public string SearchReference { get; set; } = string.Empty;
    // These three are set on the details page but included in this model for reference
    // as they may need updating. For example, if properties flooded exceeds the internally flooded 
    // estimate we can offer to update the total on save".
    public int EstimatedTotalInternallyFlooded { get; set; } = 0;
    public int EstimatedTotalExternalOnly { get; set; } = 0;
    public int EstimatedDisruptedUtilities { get; set; } = 0;

    // Property estimates
    public int EstimatedResidentialAffected { get; set; } = 0;
    public string? EstimatedResidentialAffectedString { get; set; }
    public int EstimatedResidentialFlooded { get; set; } = 0;
    public string? EstimatedResidentialFloodedString { get; set; }
    public int EstimatedResidentialOther { get; set; } = 0;
    public string? EstimatedResidentialOtherString { get; set; }
    public int EstimatedCommercialAffected { get; set; } = 0;
    public string? EstimatedCommercialAffectedString { get; set; }
    public int EstimatedCommercialFlooded { get; set; } = 0;
    public string? EstimatedCommercialFloodedString { get; set; }
    public int EstimatedCommercialOther { get; set; } = 0;
    public string? EstimatedCommercialOtherString { get; set; }

    // Utility estimates
    public int EstimatedNoElectric { get; set; } = 0;
    public string? EstimatedNoElectricString { get; set; }
    public int EstimatedNoGas { get; set; } = 0;
    public string? EstimatedNoGasString { get; set; }
    public int EstimatedNoFoul { get; set; } = 0;
    public string? EstimatedNoFoulString { get; set; }
    public int EstimatedNoWaterSupply { get; set; } = 0;
    public string? EstimatedNoWaterSupplyString { get; set; }

    // Other properties
    public ConfidenceOptions? FloodExtentConfidence { get; set; }
}
