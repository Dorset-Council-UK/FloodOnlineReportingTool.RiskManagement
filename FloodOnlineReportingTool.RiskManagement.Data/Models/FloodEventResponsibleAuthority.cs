using FloodOnlineReportingTool.RiskManagement.Models;

namespace FloodOnlineReportingTool.RiskManagement.Data.Models
{
    public class FloodEventResponsibleAuthority
    {
        public FloodEvent? FloodEvent { get; set; }
        public ResponsibleAuthority? ResponsibleAuthority { get; set; }
        public int FloodEventId { get; set; }
        public Guid ResponsibleAuthorityId { get; set; }
    }
}
