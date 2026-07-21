using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Enums;
using FloodOnlineReportingTool.Database.Models.SavedSearches;

namespace FloodOnlineReportingTool.Database.Models.FloodEvents;

public record FloodEvent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    // Always exactly one linked saved search.
    public Guid SavedSearchId { get; init; }
    public SavedSearch SavedSearch { get; init; } = null!;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Reference => SavedSearch.Reference; // Allows direct searching by saved search reference

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public ConditionalOptions MitigationTaken { get; init; } = ConditionalOptions.No;
    // Values from Contracts.Shared.EventStatusIds
    public Guid RecordStatus { get; init; } = EventStatusIds.RecoveryPhase;
    // Values from Contracts.Shared.EventStatusIds
    public Guid FloodStatus { get; init; } = EventStatusIds.PropertiesAffected;
    // Values from Contracts.Shared.Section19StatusIds
    public Guid InvestigationStatus { get; init; } = Section19StatusIds.NoSection19;

    // Event details
    public int EstimatedVulnerable { get; init; }
    public int EstimatedUninhabitable { get; init; }
    public int? EstimatedNoElectric { get; init; }
    public int? EstimatedNoGas { get; init; }
    public int? EstimatedNoFoul { get; init; }
    public int? EstimatedNoWaterSupply { get; init; }
    public int? EstimatedDisruptedUtilities { get; init; }

    // Property estimates
    public int? EstimatedResidentialAffected { get; init; }
    public int? EstimatedResidentialFlooded { get; init; }
    public int? EstimatedResidentialOther { get; init; }
    public int? EstimatedCommercialAffected { get; init; }
    public int? EstimatedCommercialFlooded { get; init; }
    public int? EstimatedCommercialOther { get; init; }
    public int EstimatedTotalExternalOnly { get; init; }
    public int EstimatedTotalInternallyFlooded { get; init; }

    // Notes
    public string StatusNote { get; init; } = string.Empty;

    // Later properties - not currently in use
    public ConfidenceOptions? FloodExtentConfidence { get; init; }

    /// <summary>
    /// All media items associated with the flood report.
    /// </summary>
    public ICollection<MediaItem> MediaItems { get; set; } = [];

}
