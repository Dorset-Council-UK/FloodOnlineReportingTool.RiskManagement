using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models
{
    public class FloodEventDto
    {
        public required int Id { get; set; }
        public DateTimeOffset? FloodStartDate { get; set; } = new();
        public DateTimeOffset? FloodEndDate { get; set; } = new();
        public string? FloodStatus { get; set; }
        public string? ResponsibleAuthority { get; set; }
        public string? ResponsibleAuthorityOther { get; set; }
        public string? FloodTitle { get; set; }
    }
}
