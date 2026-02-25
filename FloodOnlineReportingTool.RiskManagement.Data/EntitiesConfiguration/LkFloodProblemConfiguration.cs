using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class LkFloodProblemConfiguration : IEntityTypeConfiguration<LkFloodProblem>
{
    public void Configure(EntityTypeBuilder<LkFloodProblem> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("All possible flood problems and their user friendly descriptions"));

        builder
            .HasData(InitialData.FloodProblemData());
    }
}