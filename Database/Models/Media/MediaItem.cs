namespace FloodOnlineReportingTool.Database.Models.Media
{
    public class MediaItem
    {
        public Guid Id { get; init; } = Guid.CreateVersion7();
        public DateTimeOffset? UploadDateUtc { get; set; }
        public required string URL { get; set; }
        public string? Title { get; set; }

        // Foreign key
        public Guid FloodEventId { get; set; }
    }
}
