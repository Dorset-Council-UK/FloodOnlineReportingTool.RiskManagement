using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Messaging;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.RiskManagement.Data.DbContexts;

public class RiskManagementDbContext(DbContextOptions<RiskManagementDbContext> options) : DbContext(options)
{
    public DbSet<SavedSearch> SavedSearches { get; set; }
    public DbSet<FloodEvent> FloodEvents { get; set; }
    public DbSet<FloodEventComment> FloodEventComments { get; set; }
    public DbSet<ActionLog> ActionLogs { get; set; }
    public DbSet<EventLog> EventLogs { get; set; }

    public DbSet<ActionDefinition> ActionDefinitions { get; set; }
    public DbSet<CommentType> CommentTypes { get; set; }
    public DbSet<EventStatus> EventStatuses { get; set; }
    public DbSet<FloodStatus> FloodStatuses { get; set; }
    public DbSet<InvestigationStatus> InvestigationStatuses { get; set; }
    public DbSet<FloodProblem> FloodProblems { get; set; }
    public DbSet<ReportStatus> ReportStatuses { get; set; }
    public DbSet<DownstreamApiLayer> DownstreamApiLayers { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaNames.RiskManagement);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RiskManagementDbContext).Assembly);
    }
}
