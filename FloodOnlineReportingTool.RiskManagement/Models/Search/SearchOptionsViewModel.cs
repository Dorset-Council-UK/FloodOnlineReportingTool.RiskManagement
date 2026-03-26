using FloodOnlineReportingTool.RiskManagement.Converters;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using GdsBlazorComponents;
using NetTopologySuite.Geometries;
using System.Text.Json.Serialization;

namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public class SearchOptionsViewModel : ISearchOptions
{
    // Common search parameters
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? ByReference { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Date)]
    public GdsDate FromDate { get; set; } = new();

    [GdsFieldErrorClass(GdsFieldTypes.Date)]
    public GdsDate ToDate { get; set; } = new();

    public IList<Guid> ByFloodCause { get; set; } = [];

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? ByPlaceName { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? ByPersonsName { get; set; }
    [JsonConverter(typeof(MultiPolygonJsonConverter))]
    public MultiPolygon? ByPolygon { get; set; }

    // Result properties
    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public ResultTypes ShowResultsAs { get; set; } = ResultTypes.List;


    // View specific parameters 

    // Only used to convert a point from the map to a polygon search. 
    // we will only use the geometry for searches rather than easting / northing. 
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? BufferText { get; set; }
    public int? Buffer { get; set; }

    public SearchOptionsViewModel ShallowCopy()
    {
        return (SearchOptionsViewModel)this.MemberwiseClone();
    }
}
