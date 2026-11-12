using FloodOnlineReportingTool.Database.Models.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class PublicMediaSubmissionConfiguration : IEntityTypeConfiguration<PublicMediaSubmission>
{
    public const string ReferenceIndexName = "IX_PublicMediaSubmissions_Reference";

    public void Configure(EntityTypeBuilder<PublicMediaSubmission> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.Reference).IsRequired().HasMaxLength(10);
        builder.HasIndex(s => s.Reference).IsUnique().HasDatabaseName(ReferenceIndexName);
        builder.Property(s => s.Name).HasMaxLength(PublicMediaSubmissionInput.MaxNameLength);
        builder.Property(s => s.EmailAddress).HasMaxLength(PublicMediaSubmissionInput.MaxEmailLength);
        builder.Property(s => s.SubmittedAtUtc).IsRequired();
        builder.Property(s => s.SharingAgreementAccepted).IsRequired();
        builder.HasMany(s => s.MediaItems)
            .WithOne(m => m.PublicMediaSubmission)
            .HasForeignKey(m => m.PublicMediaSubmissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
