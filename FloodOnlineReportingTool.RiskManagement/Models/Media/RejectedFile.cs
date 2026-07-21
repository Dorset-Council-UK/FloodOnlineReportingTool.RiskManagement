using Microsoft.AspNetCore.Components.Forms;
namespace FloodOnlineReportingTool.Database.Models.Media;

public readonly record struct RejectedFile(IBrowserFile File, FileRejectionReason FileRejectionReason);