using FloodOnlineReportingTool.Contracts.Shared;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Models.Report;

public record FloodReportSource
{
    public Guid Id { get; init; } = Guid.CreateVersion7(); // Primary key and not the same as in calling apps
    public required string Reference { get; init; } // This is the common reference key
    public required Uri ViewUri { get; init; }

    // ICreatedUpdated implementation
    public DateTimeOffset CreatedUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedUtc { get; set; }

    // Other properties
    public required Guid EligibilityCheckId { get; init; } // This is the ID from the sending system
    public long? Uprn { get; init; }
    public long? Usrn { get; init; }
    public required Point Geom { get; init; }
    public double Easting { get; init; }
    public double Northing { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public DateTimeOffset? ImpactStartUtc { get; init; }
    public int? ImpactDurationHours { get; init; }
    public bool IsOnGoing { get; init; }
    public bool IsUninhabitable { get; init; }
    public int? VulnerableCount { get; init; }
    public string? LocationDescription { get; init; }
    public bool HasInvestigation { get; init; }
    public bool HasContacts { get; init; }
    public IReadOnlyCollection<ContactRecordType> ContactRecordTypes { get; init; } = [];

    // Navigation properties using wrapper models instead of contract types directly
    public ICollection<FloodReportSourceFloodCause> FloodCauses { get; init; } = [];
    public ICollection<FloodReportSourceOrganisation> Organisations { get; init; } = [];
}
