using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models.Media
{
    public class MediaUpload
    {
        [GdsFieldErrorClass(GdsFieldTypes.FileUpload)]
        public IList<MediaItem> UploadedFiles { get; set; } = [];
    }
}
