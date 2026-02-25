using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class LkCommentTypeConfiguration : IEntityTypeConfiguration<LkCommentType>
{
    public void Configure(EntityTypeBuilder<LkCommentType> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("All possible comment types and their user friendly descriptions"));

        builder
            .HasData(InitialData.CommentTypeData());
    }
}