using FloodOnlineReportingTool.Database.Models.Actions;
using FloodOnlineReportingTool.Database.Models.FloodEvents;
using FloodOnlineReportingTool.Database.Models.Imports;
using FloodOnlineReportingTool.Database.Models.Lookups;
using FloodOnlineReportingTool.Database.Models.Messaging;
using FloodOnlineReportingTool.Database.Models.SavedSearches;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.Database.DbContexts;

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
    public DbSet<MediaItem> MediaItems { get; set; }

    public virtual DbSet<FailedImport> FailedImports { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaNames.RiskManagement);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RiskManagementDbContext).Assembly);
    }
}
