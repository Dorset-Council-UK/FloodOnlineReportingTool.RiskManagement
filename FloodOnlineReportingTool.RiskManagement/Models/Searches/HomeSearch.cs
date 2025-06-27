using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models.Searches
{
    public class HomeSearch
    {
        [GdsFieldErrorClass(GdsFieldTypes.Input)]
        public string? SearchText { get; set; }
    }
}
