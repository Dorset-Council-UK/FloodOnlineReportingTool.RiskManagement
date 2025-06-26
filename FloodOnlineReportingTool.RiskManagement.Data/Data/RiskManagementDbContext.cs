using FloodOnlineReportingTool.RiskManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.RiskManagement.Data.Data
{
    public class RiskManagementDbContext(DbContextOptions<RiskManagementDbContext> options) : DbContext(options), IRiskManagementDbContext
    {
        public DbSet<FloodEvent> FloodEvents { get; set; }
        public DbSet<Example> Examples { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("riskmanagement");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RiskManagementDbContext).Assembly);
        }
    }
}
