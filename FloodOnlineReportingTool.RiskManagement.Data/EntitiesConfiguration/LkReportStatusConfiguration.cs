using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class LkReportStatusConfiguration : IEntityTypeConfiguration<LkReportStatus>
{
    public void Configure(EntityTypeBuilder<LkReportStatus> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("All possible report statuses and their user friendly descriptions"));

        builder
            .HasData(InitialData.ReportStatusData());
    }
}