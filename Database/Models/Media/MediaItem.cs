using System.Globalization;

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

        public MediaType Format
        {
            get
            {
                var extension = System.IO.Path.GetExtension(URL.Split('?', 2)[0]).ToLower(CultureInfo.InvariantCulture);
                return (extension ?? "") switch // URL.Substring(URL.Length - 4).ToLower
                {
                    ".gif" or
                    ".jpg" or
                    ".jpeg" or
                    ".png" or
                    ".bmp" or
                    ".tif" or
                    ".tiff" or
                    ".webp"
                        => MediaType.Image,

                    ".wmv" or
                    ".mov" or
                    ".mp4" or
                    ".mpg" or
                    ".mpeg" or
                    ".qt" or
                    ".3gp" or
                    ".webm" or
                    ".ogg"
                        => MediaType.Video,

                    ".pdf"
                        => MediaType.PDF_Document,

                    ".doc" or
                    ".docx"
                        => MediaType.Word_Document,

                    ".xls" or
                    ".xlsx"
                        => MediaType.Excel_Spreadsheet,

                    _ => MediaType.Unknown,
                };
            }
        }
    }
}
