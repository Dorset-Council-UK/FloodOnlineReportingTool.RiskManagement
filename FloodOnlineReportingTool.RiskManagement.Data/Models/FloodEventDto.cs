using FloodOnlineReportingTool.RiskManagement.Data.Models;

namespace FloodOnlineReportingTool.RiskManagement.Models
{
    public class FloodEventDto
    {
        public DateTimeOffset? FloodStartDate { get; set; } = new();
        public DateTimeOffset? FloodEndDate { get; set; } = new();
        public string? FloodStatus { get; set; }
        public Guid? ResponsibleAuthorityId { get; set; }
        public string? ResponsibleAuthorityOther { get; set; }
        public string? FloodTitle { get; set; }

        // Navigation properties
        public ResponsibleAuthority? ResponsibleAuthority { get; set; }
    }
}
