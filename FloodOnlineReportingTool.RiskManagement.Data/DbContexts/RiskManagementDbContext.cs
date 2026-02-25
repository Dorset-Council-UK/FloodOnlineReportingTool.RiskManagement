using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.RiskManagement.Data.DbContexts;

public class RiskManagementDbContext(DbContextOptions<RiskManagementDbContext> options) : DbContext(options)
{
    public DbSet<SavedSearch> SavedSearches { get; set; }
    public DbSet<FloodEvent> FloodEvents { get; set; }
    public DbSet<FloodEventComment> FloodEventComments { get; set; }

    public DbSet<LkCommentType> LkCommentTypes { get; set; }
    public DbSet<LkEventStatus> LkEventStatuses { get; set; }
    public DbSet<LkFloodStatus> LkFloodStatuses { get; set; }
    public DbSet<LkInvestigationStatus> LkInvestigationStatuses { get; set; }
    public DbSet<LkFloodProblem> LkFloodProblems { get; set; }
    public DbSet<LkReportStatus> LkReportStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaNames.RiskManagement);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RiskManagementDbContext).Assembly);
    }
}
