using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Models.Export;

public sealed class FloodEventExportDto
{
    public string Reference { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public DateTimeOffset FloodingStart { get; init; }
    public DateTimeOffset FloodingEnd { get; init; }

    public IReadOnlyCollection<string> PrimaryFloodCauses { get; init; } = [];
    public IReadOnlyCollection<string> SecondaryFloodCauses { get; init; } = [];
    public IReadOnlyCollection<string> ResponsibleOrganisations { get; init; } = [];

    public long? UPRN { get; init; }
    public long? USRN { get; init; }
    public MultiPolygon Geom { get; init; } = null!;

    public string RecordStatus { get; init; } = string.Empty;
    public string FloodStatus { get; init; } = string.Empty;
    public string InvestigationStatus { get; init; } = string.Empty;
    public int EstimatedVulnerable { get; init; } = 0;
    public int EstimatedUninhabitable { get; init; } = 0;
    public int EstimatedTotalExternalOnly { get; init; } = 0;
    public int EstimatedTotalInternallyFlooded { get; init; } = 0;
    public int EstimatedDisruptedUtilities { get; init; } = 0;

    public int? EstimatedResidentialAffected { get; init; }
    public int? EstimatedResidentialFlooded { get; init; }
    public int? EstimatedResidentialOther { get; init; }
    public int? EstimatedCommercialAffected { get; init; }
    public int? EstimatedCommercialFlooded { get; init; }
    public int? EstimatedCommercialOther { get; init; }

    public int? EstimatedNoElectric { get; init; }
    public int? EstimatedNoGas { get; init; }
    public int? EstimatedNoFoul { get; init; }
    public int? EstimatedNoWaterSupply { get; init; }

    public string StatusNote { get; init; } = string.Empty;
}
