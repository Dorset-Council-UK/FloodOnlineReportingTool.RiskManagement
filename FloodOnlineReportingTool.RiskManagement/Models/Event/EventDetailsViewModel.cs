using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;
public class EventDetailsViewModel : EventBaseModel
{
    public string EstimatedVulnerableString { get; set; } = "0";
    public string EstimatedUninhabitableString { get; set; } = "0";
    public string EstimatedTotalExternalOnlyString { get; set; } = "0";
    public string EstimatedTotalInternallyFloodedString { get; set; } = "0";
    public string EstimatedDisruptedUtilitiesString { get; set; } = "0";
    public string? StatusNote { get; set; }

    public EventDetailsViewModel ToModel(SavedSearch model)
    {
        EventDetailsViewModel viewModel = new()
        {
            SearchTitle = model.SearchTitle,
            RecordStatus = model.FloodEvent?.RecordStatus ?? EventStatusIds.RecoveryPhase,
            FloodStatus = model.FloodEvent?.FloodStatus ?? EventStatusIds.PropertiesAffected,
            InvestigationStatus = model.FloodEvent?.InvestigationStatus ?? Section19StatusIds.NoSection19,
            EstimatedTotalInternallyFlooded = model.FloodEvent?.EstimatedTotalInternallyFlooded ?? 0,
            EstimatedTotalInternallyFloodedString = (model.FloodEvent?.EstimatedTotalInternallyFlooded ?? 0).ToString(CultureInfo.InvariantCulture),
            EstimatedTotalExternalOnly = model.FloodEvent?.EstimatedTotalExternalOnly ?? 0,
            EstimatedTotalExternalOnlyString = (model.FloodEvent?.EstimatedTotalExternalOnly ?? 0).ToString(CultureInfo.InvariantCulture),
            EstimatedDisruptedUtilities = model.FloodEvent?.EstimatedDisruptedUtilities ?? 0,
            EstimatedDisruptedUtilitiesString = (model.FloodEvent?.EstimatedDisruptedUtilities ?? 0).ToString(CultureInfo.InvariantCulture),
            EstimatedVulnerable = model.FloodEvent?.EstimatedVulnerable ?? 0,
            EstimatedVulnerableString = (model.FloodEvent?.EstimatedVulnerable ?? 0).ToString(CultureInfo.InvariantCulture),
            EstimatedUninhabitable = model.FloodEvent?.EstimatedUninhabitable ?? 0,
            EstimatedUninhabitableString = (model.FloodEvent?.EstimatedUninhabitable ?? 0).ToString(CultureInfo.InvariantCulture),
            StatusNote = model.FloodEvent?.StatusNote ?? ""
        };

        return viewModel;
    }
}
