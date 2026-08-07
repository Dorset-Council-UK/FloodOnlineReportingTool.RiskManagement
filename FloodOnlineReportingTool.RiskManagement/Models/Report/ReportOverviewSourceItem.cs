using FloodOnlineReportingTool.Contracts.Shared;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Models.Report;

public class ReportOverviewSourceItem
{
    public required string Reference { get; init; } // This is the common reference key
    public required Uri ViewUri { get; init; }

    // ICreatedUpdated implementation
    public DateTimeOffset CreatedUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedUtc { get; set; }

    // Other properties
    public long? Uprn { get; init; }
    public long? Usrn { get; init; }
    public double Easting { get; init; }
    public double Northing { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public DateTimeOffset? ImpactStartUtc { get; init; }
    public int? ImpactDurationHours { get; init; }
    public bool IsOnGoing { get; init; }
    public bool IsUninhabitable { get; init; }
    public int? VulnerableCount { get; init; }
    public bool HasInvestigation { get; init; }
    public bool HasContacts { get; init; }
    public IReadOnlyCollection<string> ContactRecordTypes { get; set; } = [];
    public IReadOnlyCollection<string> FloodCauses { get; set; } = [];
    public IReadOnlyCollection<string> Organisations { get; set; } = [];
}
