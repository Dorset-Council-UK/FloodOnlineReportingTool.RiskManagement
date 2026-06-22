using FloodOnlineReportingTool.RiskManagement.Converters;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
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
    /// <remarks>This takes precedence over <see cref="IsSavedSearchLocked"/>.</remarks>
    public bool OverrideLock { get; set; }

    /// <summary>
    /// Copy the values from a <see cref="SavedSearch"/> into this view model.
    /// </summary>
    /// <remarks>This copies the values, never creates a new SearchOptionsViewModel.</remarks>
    public void CopyFrom(SavedSearch savedSearch)
    {
        ArgumentNullException.ThrowIfNull(savedSearch);
        
        SearchReference = savedSearch.Reference;
        // ByReference
        FromDate.CopyFrom(savedSearch.ResultsFrom);
        ToDate.CopyFrom(savedSearch.ResultsTo);
        ByFloodSource = [.. savedSearch.FloodSources];
        ByPostcode = string.IsNullOrWhiteSpace(savedSearch.Postcode) ? null : savedSearch.Postcode;
        ByPersonsName = string.IsNullOrWhiteSpace(savedSearch.ReporterString) ? null : savedSearch.ReporterString;
        ByPolygon = savedSearch.Geom;
        IsSavedSearchLocked = savedSearch.IsLocked;
        // OverrideLock
    }

    /// <summary>
    /// Copy the values from another <see cref="SearchOptionsViewModel"/> into this view model.
    /// </summary>
    /// <remarks>This copies the values, never creates a new SearchOptionsViewModel.</remarks>
    public void CopyFrom(SearchOptionsViewModel other)
    {
        ArgumentNullException.ThrowIfNull(other);

        SearchReference = other.SearchReference;
        ByReference = other.ByReference;
        FromDate.CopyFrom(other.FromDate);
        ToDate.CopyFrom(other.ToDate);
        ByFloodSource = other.ByFloodSource;
        ByPostcode = other.ByPostcode;
        ByPersonsName = other.ByPersonsName;
        ByPolygon = other.ByPolygon;
        IsSavedSearchLocked = other.IsSavedSearchLocked;
        OverrideLock = other.OverrideLock;
    }
}
