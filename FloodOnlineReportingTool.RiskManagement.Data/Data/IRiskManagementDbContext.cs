using FloodOnlineReportingTool.RiskManagement.Data.Models;
using FloodOnlineReportingTool.RiskManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.RiskManagement.Data.Data
{
    public interface IRiskManagementDbContext
    {
        DbSet<FloodEventDto> FloodEvents { get; set; }
        DbSet<ResponsibleAuthority> ResponsibleAuthorities { get; set; }
    }
}
