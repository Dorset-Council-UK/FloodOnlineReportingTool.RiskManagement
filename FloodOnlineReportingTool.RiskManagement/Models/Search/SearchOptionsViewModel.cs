using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public class SearchOptionsViewModel
{
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? ReportReference { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public FloodLocationSearchType? SearchType { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Date)]
    public GdsDate FromDate { get; set; } = new();

    [GdsFieldErrorClass(GdsFieldTypes.Date)]
    public GdsDate ToDate { get; set; } = new();

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? ByPostcode { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? ByName { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? EastingText { get; set; }

    public double? Easting { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? NorthingText { get; set; }

    public double? Northing { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? BufferText { get; set; }
    public int? Buffer { get; set; }

    public string? ByAreaWkt { get; set; }

    // Do we need a geometry property or is Well Known Text enough?

    // Other possible search criteria:
    // - By cause of flooding - select many
    // - By responsible organisation...or is this dealt with by using users properties?
    // - Internal / External flooding
}
