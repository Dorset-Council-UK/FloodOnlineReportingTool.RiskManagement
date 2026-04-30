using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
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

    public EventDetailsViewModel()
    {
    }

    public EventDetailsViewModel(SavedSearch savedSearch)
    {
        SearchReference = savedSearch.Reference;
        SearchTitle = savedSearch.SearchTitle;
        RecordStatus = savedSearch.FloodEvent?.RecordStatus ?? EventStatusIds.RecoveryPhase;
        FloodStatus = savedSearch.FloodEvent?.FloodStatus ?? EventStatusIds.PropertiesAffected;
        InvestigationStatus = savedSearch.FloodEvent?.InvestigationStatus ?? Section19StatusIds.NoSection19;
        EstimatedTotalInternallyFlooded = savedSearch.FloodEvent?.EstimatedTotalInternallyFlooded ?? 0;
        EstimatedTotalInternallyFloodedString = (savedSearch.FloodEvent?.EstimatedTotalInternallyFlooded ?? 0).ToString(CultureInfo.InvariantCulture);
        EstimatedTotalExternalOnly = savedSearch.FloodEvent?.EstimatedTotalExternalOnly ?? 0;
        EstimatedTotalExternalOnlyString = (savedSearch.FloodEvent?.EstimatedTotalExternalOnly ?? 0).ToString(CultureInfo.InvariantCulture);
        EstimatedDisruptedUtilities = savedSearch.FloodEvent?.EstimatedDisruptedUtilities ?? 0;
        EstimatedDisruptedUtilitiesString = (savedSearch.FloodEvent?.EstimatedDisruptedUtilities ?? 0).ToString(CultureInfo.InvariantCulture);
        EstimatedVulnerable = savedSearch.FloodEvent?.EstimatedVulnerable ?? 0;
        EstimatedVulnerableString = (savedSearch.FloodEvent?.EstimatedVulnerable ?? 0).ToString(CultureInfo.InvariantCulture);
        EstimatedUninhabitable = savedSearch.FloodEvent?.EstimatedUninhabitable ?? 0;
        EstimatedUninhabitableString = (savedSearch.FloodEvent?.EstimatedUninhabitable ?? 0).ToString(CultureInfo.InvariantCulture);
        StatusNote = savedSearch.FloodEvent?.StatusNote ?? "";
    }
}
