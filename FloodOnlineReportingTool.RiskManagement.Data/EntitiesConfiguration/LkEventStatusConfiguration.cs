using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class LkEventStatusConfiguration : IEntityTypeConfiguration<LkEventStatus>
{
    public void Configure(EntityTypeBuilder<LkEventStatus> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("All possible event statuses and their user friendly descriptions"));

        builder
            .HasData(InitialData.EventStatusData());
    }
}