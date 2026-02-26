using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class CommentTypeConfiguration : IEntityTypeConfiguration<CommentType>
{
    public void Configure(EntityTypeBuilder<CommentType> builder)
    {
        builder.ToTable("Lk_CommentTypes");
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("All possible comment types and their user friendly descriptions"));

        builder
            .HasData(InitialData.CommentTypeData());
    }
}