using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Lookups;
using FloodOnlineReportingTool.Database.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class ReportStatusConfiguration : IEntityTypeConfiguration<ReportStatus>
{
    public void Configure(EntityTypeBuilder<ReportStatus> builder)
    {
        builder.ToTable("Lk_ReportStatuses");
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("All possible report statuses and their user friendly descriptions"));

        builder
            .HasData(InitialData.ReportStatusData());
    }
}