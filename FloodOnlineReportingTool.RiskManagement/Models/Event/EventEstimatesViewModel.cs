using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Enums;
using FloodOnlineReportingTool.Database.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;

public class EventEstimatesViewModel : EventBaseModel
{
    // Property estimates
    public int EstimatedResidentialAffected { get; set; } = 0;
    public string EstimatedResidentialAffectedString { get; set; } = "0";
    public int EstimatedResidentialFlooded { get; set; } = 0;
    public string EstimatedResidentialFloodedString { get; set; } = "0";
    public int EstimatedResidentialOther { get; set; } = 0;
    public string EstimatedResidentialOtherString { get; set; } = "0";
    public int EstimatedCommercialAffected { get; set; } = 0;
    public string EstimatedCommercialAffectedString { get; set; } = "0";
    public int EstimatedCommercialFlooded { get; set; } = 0;
    public string EstimatedCommercialFloodedString { get; set; } = "0";
    public int EstimatedCommercialOther { get; set; } = 0;
    public string EstimatedCommercialOtherString { get; set; } = "0";

    // Utility estimates
    public int EstimatedNoElectric { get; set; } = 0;
    public string EstimatedNoElectricString { get; set; } = "0";
    public int EstimatedNoGas { get; set; } = 0;
    public string EstimatedNoGasString { get; set; } = "0";
    public int EstimatedNoFoul { get; set; } = 0;
    public string EstimatedNoFoulString { get; set; } = "0";
    public int EstimatedNoWaterSupply { get; set; } = 0;
    public string EstimatedNoWaterSupplyString { get; set; } = "0";

    // Other properties
    public ConfidenceOptions? FloodExtentConfidence { get; set; }

    public EventEstimatesViewModel ToModel(SavedSearch model)
    {
        EventEstimatesViewModel viewModel = new()
        {
            FloodEventId = model.FloodEventId,
            SearchReference = model.Reference,
            SearchTitle = model.SearchTitle,
            RecordStatus = model.FloodEvent?.RecordStatus ?? EventStatusIds.RecoveryPhase,
            FloodStatus = model.FloodEvent?.FloodStatus ?? EventStatusIds.PropertiesAffected,
            InvestigationStatus = model.FloodEvent?.InvestigationStatus ?? Section19StatusIds.NoSection19,
            EstimatedTotalInternallyFlooded = model.FloodEvent?.EstimatedTotalInternallyFlooded ?? 0,
            EstimatedTotalExternalOnly = model.FloodEvent?.EstimatedTotalExternalOnly ?? 0,
            EstimatedDisruptedUtilities = model.FloodEvent?.EstimatedDisruptedUtilities ?? 0,
            EstimatedVulnerable = model.FloodEvent?.EstimatedVulnerable ?? 0,
            EstimatedUninhabitable = model.FloodEvent?.EstimatedUninhabitable ?? 0,
            EstimatedResidentialAffected = model.FloodEvent?.EstimatedResidentialAffected ?? 0,
            EstimatedResidentialAffectedString = (model.FloodEvent?.EstimatedResidentialAffected ?? 0).ToString(CultureInfo.InvariantCulture),
            EstimatedResidentialFlooded = model.FloodEvent?.EstimatedResidentialFlooded ?? 0,
            EstimatedResidentialFloodedString = (model.FloodEvent?.EstimatedResidentialFlooded ?? 0).ToString(CultureInfo.InvariantCulture),
            EstimatedResidentialOther = model.FloodEvent?.EstimatedResidentialOther ?? 0,
            EstimatedResidentialOtherString = (model.FloodEvent?.EstimatedResidentialOther ?? 0).ToString(CultureInfo.InvariantCulture),
            EstimatedCommercialAffected = model.FloodEvent?.EstimatedCommercialAffected ?? 0,
            EstimatedCommercialAffectedString = (model.FloodEvent?.EstimatedCommercialAffected ?? 0).ToString(CultureInfo.InvariantCulture),
            EstimatedCommercialFlooded = model.FloodEvent?.EstimatedCommercialFlooded ?? 0,
            EstimatedCommercialFloodedString = (model.FloodEvent?.EstimatedCommercialFlooded ?? 0).ToString(CultureInfo.InvariantCulture),
            EstimatedCommercialOther = model.FloodEvent?.EstimatedCommercialOther ?? 0,
            EstimatedCommercialOtherString = (model.FloodEvent?.EstimatedCommercialOther ?? 0).ToString(CultureInfo.InvariantCulture),
            EstimatedNoElectric = model.FloodEvent?.EstimatedNoElectric ?? 0,
            EstimatedNoElectricString = (model.FloodEvent?.EstimatedNoElectric ?? 0).ToString(CultureInfo.InvariantCulture),
            EstimatedNoGas = model.FloodEvent?.EstimatedNoGas ?? 0,
            EstimatedNoGasString = (model.FloodEvent?.EstimatedNoGas ?? 0).ToString(CultureInfo.InvariantCulture),
            EstimatedNoFoul = model.FloodEvent?.EstimatedNoFoul ?? 0,
            EstimatedNoFoulString = (model.FloodEvent?.EstimatedNoFoul ?? 0).ToString(CultureInfo.InvariantCulture),
            EstimatedNoWaterSupply = model.FloodEvent?.EstimatedNoWaterSupply ?? 0,
            EstimatedNoWaterSupplyString = (model.FloodEvent?.EstimatedNoWaterSupply ?? 0).ToString(CultureInfo.InvariantCulture),
            FloodExtentConfidence = model.FloodEvent?.FloodExtentConfidence ?? ConfidenceOptions.Unknown
        };

        return viewModel;
    }
}
