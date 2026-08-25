using Microsoft.AspNetCore.Components;
using FloodOnlineReportingTool.RiskManagement.Models.Report;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodReport;

public partial class SourceSummary
{

    [Parameter]
    public required ReportOverviewSourceItem SourceRecord { get; set; }

}