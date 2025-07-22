using FloodOnlineReportingTool.RiskManagement.Data.Models;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models
{
    public class FloodEventDto
    {
        [GdsFieldErrorClass(GdsFieldTypes.Date)]
        public GdsDate? FloodStartDate { get; set; } = new();

        [GdsFieldErrorClass(GdsFieldTypes.Date)]
        public GdsDate? FloodEndDate { get; set; } = new();

        [GdsFieldErrorClass(GdsFieldTypes.Radio)]
        public string? FloodStatus { get; set; }

        [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
        public IList<Guid>? ResponsibleAuthorityIds { get; set; } = [];

        [GdsFieldErrorClass(GdsFieldTypes.Input)]
        public string? ResponsibleAuthorityOther { get; set; }

        [GdsFieldErrorClass(GdsFieldTypes.Input)]
        public string? FloodTitle { get; set; }
    }
}
