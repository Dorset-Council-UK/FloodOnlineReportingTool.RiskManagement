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

    public IList<Guid> ByFloodSource { get; set; } = [];

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? ByPostcode { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? ByPersonsName { get; set; }
    [JsonConverter(typeof(MultiPolygonJsonConverter))]
    public MultiPolygon? ByPolygon { get; set; }
}
