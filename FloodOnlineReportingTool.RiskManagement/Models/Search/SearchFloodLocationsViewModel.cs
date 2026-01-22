using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public class SearchFloodLocationsViewModel
{
    public int? PageNumber { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? EastingText { get; set; }

    public double? Easting { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? NorthingText { get; set; }

    public double? Northing { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public double? Buffer { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public FloodLocationSearchType? SearchType { get; set; }
}
