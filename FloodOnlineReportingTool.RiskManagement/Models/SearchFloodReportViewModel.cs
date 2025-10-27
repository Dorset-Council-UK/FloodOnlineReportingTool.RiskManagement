using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models;

public class SearchFloodReportViewModel
{
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? SearchText { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? UprnText { get; set; }
    public int? Uprn { get; set; }
}
