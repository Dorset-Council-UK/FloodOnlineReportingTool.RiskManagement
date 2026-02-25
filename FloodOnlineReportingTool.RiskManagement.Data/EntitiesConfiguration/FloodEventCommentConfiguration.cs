using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class FloodEventCommentConfiguration : IEntityTypeConfiguration<FloodEventComment>
{
    public void Configure(EntityTypeBuilder<FloodEventComment> builder)
    {
        // Store enums as integers in the database
        builder.Property(e => e.CommentType)
                .HasConversion<int>();
    }
}
