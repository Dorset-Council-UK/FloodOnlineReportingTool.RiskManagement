using FloodOnlineReportingTool.RiskManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.RiskManagement.Data.Data
{
    public class RiskManagementDbContext(DbContextOptions<RiskManagementDbContext> options) : DbContext(options), IRiskManagementDbContext
    {
        public DbSet<FloodEventDto> FloodEvents { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("riskmanagement");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RiskManagementDbContext).Assembly);
        }
    }
}
