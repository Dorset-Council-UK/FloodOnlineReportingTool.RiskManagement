using FloodOnlineReportingTool.RiskManagement.Data.Models;
using FloodOnlineReportingTool.RiskManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.RiskManagement.Data.Data
{
    public interface IRiskManagementDbContext
    {
        DbSet<FloodEvent> FloodEvents { get; set; }
        DbSet<ResponsibleAuthority> ResponsibleAuthorities { get; set; }
        DbSet<FloodEventResponsibleAuthority> FloodEventResponsibleAuthorities { get; set; }
    }
}
