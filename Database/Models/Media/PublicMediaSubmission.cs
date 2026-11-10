using System.Security.Cryptography;

namespace FloodOnlineReportingTool.Database.Models.Media;

public class PublicMediaSubmission
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Reference { get; set; } = Convert.ToHexString(RandomNumberGenerator.GetBytes(5)).ToUpperInvariant();
    public string? Name { get; set; }
    public string? EmailAddress { get; set; }
    public DateTimeOffset SubmittedAtUtc { get; set; }
    public bool SharingAgreementAccepted { get; set; }
    public ICollection<MediaItem> MediaItems { get; set; } = [];
}
