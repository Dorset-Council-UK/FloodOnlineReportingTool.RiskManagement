using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class FloodStatusConfiguration : IEntityTypeConfiguration<FloodStatus>
{
    public void Configure(EntityTypeBuilder<FloodStatus> builder)
    {
        builder.ToTable("Lk_FloodStatuses");
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("All possible flood statuses and their user friendly descriptions"));

        builder
            .HasData(InitialData.FloodStatusData());
    }
}