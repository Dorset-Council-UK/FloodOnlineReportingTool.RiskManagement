using FloodOnlineReportingTool.Database.Models.Lookups;
using FloodOnlineReportingTool.Database.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class FloodProblemConfiguration : IEntityTypeConfiguration<FloodProblem>
{
    public void Configure(EntityTypeBuilder<FloodProblem> builder)
    {
        builder.ToTable("Lk_FloodProblems");
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("All possible flood problems and their user friendly descriptions"));

        builder
            .HasData(InitialData.FloodProblemData());
    }
}