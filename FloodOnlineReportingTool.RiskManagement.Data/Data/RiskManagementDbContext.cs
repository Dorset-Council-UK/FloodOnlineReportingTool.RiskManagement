using FloodOnlineReportingTool.RiskManagement.Data.Models;
using FloodOnlineReportingTool.RiskManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.RiskManagement.Data.Data
{
    public class RiskManagementDbContext(DbContextOptions<RiskManagementDbContext> options) : DbContext(options), IRiskManagementDbContext
    {
        public DbSet<FloodEventDto> FloodEvents { get; set; }
        public DbSet<ResponsibleAuthority> ResponsibleAuthorities { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("riskmanagement");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RiskManagementDbContext).Assembly);
        }
    }
}
