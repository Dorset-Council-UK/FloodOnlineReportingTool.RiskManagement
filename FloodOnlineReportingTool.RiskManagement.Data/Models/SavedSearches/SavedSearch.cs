using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
/// <summary>
/// This class allows us to save all the search parameters used to perform a 
/// flood report search. This excludes searching by report ID as that only returns one report. 
/// 
/// The purpose of saving a search is to allow a team to all look at the same results but
/// ensure that those results are up to date with the latest reports. 
/// Once a significance criteria is reached the user should convert the search into a flood 
/// event record. 
/// </summary>
public record SavedSearch
{
    // Saved search properties
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string SearchTitle { get; init; } = string.Empty;

    // Search parameters

    public DateTimeOffset? ResultsFrom { get; init; }
    public DateTimeOffset? ResultsTo { get; init; }
    // List of Contracts.Shared.PrimaryCauseIds
    public ICollection<Guid> FloodPrimaryCauses { get; init; } = [];
    // List of Contracts.Shared.SecondaryCauseIds
    public ICollection<Guid> FloodSecondaryCauses { get; init; } = [];
    public long? UPRN { get; init; } 
    public long? USRN { get; init; }
    public string Postcode { get; init; } = string.Empty;
    public string ReporterString { get; init; } = string.Empty;
    public MultiPolygon? Geom { get; init; }
    // Also known as buffer (in metres) around the search geometry (foreign key)
    public double? SearchRadius { get; init; }

    // Result properties
    public int? PagingSize { get; init; }
    public string SortBy { get; init; } = string.Empty;
    public SortOptions SortOrder { get; init; } = SortOptions.Ascending;
    // List of Contracts.Shared.OrganisationIds
    public ICollection<Guid> LimitByOrganisation { get; init; } = [];
    // List of Contracts.Shared.RecordStatusIds
    public ICollection<Guid> LimitByStatus { get; init; } = [];
    public ResultTypes ShowResultsAs { get; init; } = ResultTypes.Table;


    // Foreign key
    public Guid? FloodEventId { get; init; }
    public FloodEvent FloodEvent { get; init; }

}
