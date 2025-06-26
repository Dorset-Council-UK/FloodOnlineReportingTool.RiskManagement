using FloodOnlineReportingTool.RiskManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.RiskManagement.Data.Data
{
    public interface IRiskManagementDbContext
    {
        DbSet<FloodEventDto> FloodEvents { get; set; }
    }
}
