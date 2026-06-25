using GdsBlazorComponents;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public interface ISearchOptions
{
    public string? ByReference { get; set; }
    public GdsDate FromDate { get; set; }
    public GdsDate ToDate { get; set; }
    public IList<Guid> ByFloodSource { get; set; }
    public string? ByPostcode { get; set; }
    public string? ByPersonsName { get; set; }
    public MultiPolygon? ByPolygon { get; set; }
    bool IsSavedSearchLocked { get; set; }
    bool OverrideLock { get; set; }
}
