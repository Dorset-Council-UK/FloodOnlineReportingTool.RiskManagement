using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class EventStatusConfiguration : IEntityTypeConfiguration<EventStatus>
{
    public void Configure(EntityTypeBuilder<EventStatus> builder)
    {
        builder.ToTable("Lk_EventStatuses");
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("All possible event statuses and their user friendly descriptions"));

        builder
            .HasData(InitialData.EventStatusData());
    }
}