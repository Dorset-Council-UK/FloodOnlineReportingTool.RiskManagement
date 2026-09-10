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
                switch (extension ?? "") // URL.Substring(URL.Length - 4).ToLower
                {
                    case ".gif":
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".bmp":
                    case ".tif":
                    case ".tiff":
                    case ".webp":
                        {
                            return MediaType.Image;
                        }

                    case ".wmv":
                    case ".mov":
                    case ".mp4":
                    case ".mpg":
                    case ".mpeg":
                    case ".qt":
                    case ".3gp":
                    case ".webm":
                    case ".ogg":
                        {
                            return MediaType.Video;
                        }

                    case ".pdf":
                        {
                            return MediaType.PDF_Document;
                        }

                    case ".doc":
                    case ".docx":
                        {
                            return MediaType.Word_Document;
                        }

                    case ".xls":
                    case ".xlsx":
                        {
                            return MediaType.Excel_Spreadsheet;
                        }

                    default:
                        {
                            return MediaType.Unknown;
                        }
                }
            }
        }
    }
}
