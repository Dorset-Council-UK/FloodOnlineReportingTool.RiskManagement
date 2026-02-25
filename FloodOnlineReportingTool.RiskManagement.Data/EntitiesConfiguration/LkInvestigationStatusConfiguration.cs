using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class LkInvestigationStatusConfiguration : IEntityTypeConfiguration<LkInvestigationStatus>
{
    public void Configure(EntityTypeBuilder<LkInvestigationStatus> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("All possible investigation statuses and their user friendly descriptions"));

        builder
            .HasData(InitialData.InvestigationStatusData());
    }
}