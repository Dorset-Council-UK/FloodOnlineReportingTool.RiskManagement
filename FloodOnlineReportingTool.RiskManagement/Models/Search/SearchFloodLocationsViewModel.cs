using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public class SearchFloodLocationsViewModel
{
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? PageNumberText { get; set; }
    public int? PageNumber { get; set; } = 1;

    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public int? PageSize { get; set; } = 10;

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? EastingText { get; set; }
    public double? Easting { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? NorthingText { get; set; }
    public double? Northing { get; set; } = 10;

    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public double? Buffer { get; set; } = 10;

    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public FloodLocationSearchType SearchType { get; set; } = FloodLocationSearchType.All;
}
