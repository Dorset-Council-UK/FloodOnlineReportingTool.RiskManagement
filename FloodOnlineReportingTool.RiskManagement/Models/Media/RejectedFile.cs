using Microsoft.AspNetCore.Components.Forms;
namespace FloodOnlineReportingTool.RiskManagement.Data.Models.Media;

public readonly record struct RejectedFile(IBrowserFile File, FileRejectionReason FileRejectionReason);