using FloodOnlineReportingTool.RiskManagement.Data.Models;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event
{
    public class Media
    {
        [GdsFieldErrorClass(GdsFieldTypes.FileUpload)]
        public IList<MediaItem> UploadedFiles { get; set; } = [];
    }
}
