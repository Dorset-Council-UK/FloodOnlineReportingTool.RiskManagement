using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models
{
    public class FloodEvent
    {
        public int? Id { get; set; }
        [GdsFieldErrorClass(GdsFieldTypes.Date)]
        public GdsDate? FloodStartDate { get; set; } = new();
        [GdsFieldErrorClass(GdsFieldTypes.Date)]
        public GdsDate? FloodEndDate { get; set; } = new();
        [GdsFieldErrorClass(GdsFieldTypes.Radio)]
        public string? FloodStatus { get; set; }
        [GdsFieldErrorClass(GdsFieldTypes.Radio)]
        public Guid? ResponsibleAuthority {  get; set; }
        public IReadOnlyCollection<GdsOptionItem<Guid>> ResponsibleAuthorityOptions { get; set; } = [];
        [GdsFieldErrorClass(GdsFieldTypes.Input)]
        public string? ResponsibleAuthorityOther { get; set; }
        [GdsFieldErrorClass(GdsFieldTypes.Input)]
        public string? FloodTitle { get; set; }
    }
}
