using GdsBlazorComponents;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Models;

public class SearchFloodReportViewModel
{
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? UprnText { get; set; }
    public long? Uprn => long.TryParse(UprnText, CultureInfo.InvariantCulture, out var value) ? value : null;
}
