using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using GdsBlazorComponents;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public class SearchResultsViewModel : ISearchOptions
{
    // Saved search properties
    public Guid? Id { get; init; }
    public string SearchTitle { get; init; } = string.Empty;

    // Common search parameters
    public string? ByReference { get; set; }
    public GdsDate FromDate { get; set; } = new();
    public GdsDate ToDate { get; set; } = new();
    public IList<Guid> ByFloodSource { get; set; } = [];
    public string? ByPostcode { get; set; }
    public string? ByPersonsName { get; set; }
    public MultiPolygon? ByPolygon { get; set; }

    // Result properties
    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public ResultTypes ShowResultsAs { get; set; } = ResultTypes.List;

    // View specific parameters 
    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public int PagingSize { get; init; } = 10;
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string SortBy { get; init; } = string.Empty;
    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public SortOptions SortOrder { get; init; } = SortOptions.Ascending;
    // List of Contracts.Shared.OrganisationIds
    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public ICollection<Guid> LimitByOrganisation { get; init; } = [];
    // List of Contracts.Shared.RecordStatusIds
    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public ICollection<Guid> LimitByStatus { get; init; } = [];

    public int? PageNumber { get; set; }
}
