using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;

public record EventOverviewViewModel 
{
    public string SearchReference { get; init; } = string.Empty;
    public string SearchTitle { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string UpdatedBy { get; init; } = string.Empty;
    public DateTimeOffset FloodingStart { get; init; }
    public DateTimeOffset FloodingEnd { get; init; }

    public IReadOnlyCollection<string> PrimaryFloodCauses { get; init; } = [];
    public IReadOnlyCollection<string> SecondaryFloodCauses { get; init; } = [];
    public IReadOnlyCollection<string> ResponsibleOrganisations { get; init; } = [];

    public long? UPRN { get; init; } 
    public long? USRN { get; init; } 
    public MultiPolygon? Geom { get; init; } = null;

    // Common event properties (always required)
    public string RecordStatus { get; init; } = string.Empty;
    public string FloodStatus { get; init; } = string.Empty;
    public string InvestigationStatus { get; init; } = string.Empty;
    public int EstimatedVulnerable { get; init; } = 0;
    public int EstimatedUninhabitable { get; init; } = 0;
    public int EstimatedTotalExternalOnly { get; init; } = 0;
    public int EstimatedTotalInternallyFlooded { get; init; } = 0;
    public int EstimatedDisruptedUtilities { get; init; } = 0;

    // Property estimates
    public int? EstimatedResidentialAffected { get; init; }
    public int? EstimatedResidentialFlooded { get; init; }
    public int? EstimatedResidentialOther { get; init; }
    public int? EstimatedCommercialAffected { get; init; }
    public int? EstimatedCommercialFlooded { get; init; }
    public int? EstimatedCommercialOther { get; init; }

    // Utility estimates
    public int? EstimatedNoElectric { get; init; }
    public int? EstimatedNoGas { get; init; }
    public int? EstimatedNoFoul { get; init; }
    public int? EstimatedNoWaterSupply { get; init; }

    // Notes
    public string StatusNote { get; init; } = string.Empty;
}
