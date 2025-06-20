using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages;

public partial class Error(ILogger<Error> logger)
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private string? RequestId { get; set; }
    private bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    private readonly Uri? _contactLink = new("https://dorset-self.achieveservice.com/service/flood-reporting-tool-feedback");
    private readonly string _contactText = "let us know.";

    protected override void OnInitialized()
    {
        logger.LogError("Error page initialized. RequestId: {RequestId}", RequestId);
        RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
    }
}
