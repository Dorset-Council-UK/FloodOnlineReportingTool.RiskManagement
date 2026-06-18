using FloodOnlineReportingTool.RiskManagement.Converters;
using GdsBlazorComponents;
using NetTopologySuite.Geometries;
using System.Text.Json.Serialization;

namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public class SearchOptionsViewModel : ISearchOptions
{
    public string? SearchReference { get; set; }

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

    /// <summary>
    /// Is the saved search locked for editing.
    /// </summary>
    public bool IsSavedSearchLocked { get; set; }

    /// <summary>
    /// From the flood event, the user can override the lock and edit the search options.
    /// </summary>
    /// <remarks>This takes precidence over <see cref="IsSavedSearchLocked"/>.</remarks>
    public bool OverrideLock { get; set; }
}
