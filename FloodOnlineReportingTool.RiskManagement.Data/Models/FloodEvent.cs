using FloodOnlineReportingTool.RiskManagement.Data.Models;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models
{
    public class FloodEvent
    {
        public int Id { get; set; }
        public DateTimeOffset? FloodStartDate { get; set; } = new();
        public DateTimeOffset? FloodEndDate { get; set; } = new();
        public string? FloodStatus { get; set; }
        public string? ResponsibleAuthorityOther { get; set; }
        public string? FloodTitle { get; set; }

        // Navigation properties
        public IList<FloodEventResponsibleAuthority>? ResponsibleAuthorites { get; set; }
    }
}
