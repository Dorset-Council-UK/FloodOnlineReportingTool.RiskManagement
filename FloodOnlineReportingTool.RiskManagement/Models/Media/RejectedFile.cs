using Microsoft.AspNetCore.Components.Forms;
namespace FloodOnlineReportingTool.RiskManagement.Models.Media;

public readonly record struct RejectedFile(IBrowserFile File, FileRejectionReason FileRejectionReason);