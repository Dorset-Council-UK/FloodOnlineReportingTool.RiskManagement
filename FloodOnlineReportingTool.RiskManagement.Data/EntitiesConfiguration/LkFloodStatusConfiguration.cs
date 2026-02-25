using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class LkFloodStatusConfiguration : IEntityTypeConfiguration<LkFloodStatus>
{
    public void Configure(EntityTypeBuilder<LkFloodStatus> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("All possible flood statuses and their user friendly descriptions"));

        builder
            .HasData(InitialData.FloodStatusData());
    }
}