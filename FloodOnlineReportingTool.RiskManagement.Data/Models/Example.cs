using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models;

public class Example
{
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? FirstName { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public int? SomeNumber { get; set; }
}