using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using GdsBlazorComponents;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public interface ISearchOptions
{
    public string? ByReference { get; set; }
    public GdsDate FromDate { get; set; }
    public GdsDate ToDate { get; set; }
    public IList<Guid> ByFloodSource { get; set; }
    public string? ByPlaceName { get; set; }
    public string? ByPersonsName { get; set; }
    public MultiPolygon? ByPolygon { get; set; }

    // Result properties
    public ResultTypes ShowResultsAs { get; set; }

    // Other possible search criteria:
    // - By responsible organisation...or is this dealt with by using users properties?
    // - Internal / External flooding 
}
