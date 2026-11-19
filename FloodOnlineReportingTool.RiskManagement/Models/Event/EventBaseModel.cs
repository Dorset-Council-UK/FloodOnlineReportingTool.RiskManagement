using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Enums;
using FloodOnlineReportingTool.Database.Models.FloodEvents;
using FloodOnlineReportingTool.Database.Models.SavedSearches;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;

public abstract class EventBaseModel
{
    // Common search properties (always required)
    public string SearchReference { get; set; } = string.Empty;
    public string SearchTitle { get; set; } = string.Empty;
    public Guid? FloodEventId { get; set; }

    // Common event properties (always required)
    public Guid RecordStatus { get; set; } = EventStatusIds.RecoveryPhase;
    public Guid FloodStatus { get; set; } = EventStatusIds.PropertiesAffected;
    public Guid InvestigationStatus { get; set; } = Section19StatusIds.NoSection19;
    public int EstimatedVulnerable { get; set; } = 0;
    public int EstimatedUninhabitable { get; set; } = 0;
    public int EstimatedTotalExternalOnly { get; set; } = 0;
    public int EstimatedTotalInternallyFlooded { get; set; } = 0;
    public int EstimatedDisruptedUtilities { get; set; } = 0;

    /// <summary>
    /// Maps event view models to a <see cref="FloodEvent"/> and binds it to its owning saved search.
    /// </summary>
    /// <param name="savedSearchId">The parent saved search ID used as the required foreign key.</param>
    /// <param name="details">The event details model.</param>
    /// <param name="estimates">The optional event estimates model.</param>
    /// <returns>A fully populated <see cref="FloodEvent"/> ready for persistence.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="savedSearchId"/> is <see cref="Guid.Empty"/>.</exception>
    public FloodEvent ToFloodEvent(Guid savedSearchId, EventDetailsViewModel details, EventEstimatesViewModel? estimates)
    {
        if (savedSearchId == Guid.Empty)
        {
            throw new ArgumentException("SavedSearchId must be a non-empty GUID.", nameof(savedSearchId));
        }

        return new FloodEvent
        {
            SavedSearchId = savedSearchId,
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
