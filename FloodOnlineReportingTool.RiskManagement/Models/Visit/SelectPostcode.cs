using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models.Visit;
public class SelectPostcode
{

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? Postcode { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public bool? PostcodeKnown { get; set; }
}
