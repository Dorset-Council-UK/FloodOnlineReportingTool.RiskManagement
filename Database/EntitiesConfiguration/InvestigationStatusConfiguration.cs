using FloodOnlineReportingTool.Database.Models.Lookups;
using FloodOnlineReportingTool.Database.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class InvestigationStatusConfiguration : IEntityTypeConfiguration<InvestigationStatus>
{
    public void Configure(EntityTypeBuilder<InvestigationStatus> builder)
    {
        builder.ToTable("Lk_InvestigationStatuses");
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("All possible investigation statuses and their user friendly descriptions"));

        builder
            .HasData(InitialData.InvestigationStatusData());
    }
}