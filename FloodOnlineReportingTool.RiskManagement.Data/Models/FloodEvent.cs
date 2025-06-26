using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models
{
    public class FloodEvent
    {
        [GdsFieldErrorClass(GdsFieldTypes.Date)]
        public GdsDate? FloodStartDate { get; set; } = new();
        [GdsFieldErrorClass(GdsFieldTypes.Date)]
        public GdsDate? FloodEndDate { get; set; } = new();
        [GdsFieldErrorClass(GdsFieldTypes.Radio)]
        public string? FloodStatus { get; set; }
        [GdsFieldErrorClass(GdsFieldTypes.Radio)]
        public string? ResponsibleAuthority { get; set; }
        [GdsFieldErrorClass(GdsFieldTypes.Input)]
        public string? ResponsibleAuthorityOther { get; set; }
        [GdsFieldErrorClass(GdsFieldTypes.Input)]
        public string? FloodTitle { get; set; }
    }
}
