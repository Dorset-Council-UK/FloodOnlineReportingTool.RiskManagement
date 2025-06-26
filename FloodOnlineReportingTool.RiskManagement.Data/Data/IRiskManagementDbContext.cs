using FloodOnlineReportingTool.RiskManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.RiskManagement.Data.Data
{
    public interface IRiskManagementDbContext
    {
        DbSet<FloodEvent> FloodEvents { get; set; }
        DbSet<Example> Examples { get; set; }
    }
}
