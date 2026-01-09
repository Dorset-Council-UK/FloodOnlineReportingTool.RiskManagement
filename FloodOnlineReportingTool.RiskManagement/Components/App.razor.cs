using FloodOnlineReportingTool.RiskManagement.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;

namespace FloodOnlineReportingTool.RiskManagement.Components;

public partial class App(IOptions<RiskManagementSettings> _settings)
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    // The Render Mode is InteractiveServer most of the time. Except on pages which are set as Static server-side rendering (static SSR)
    private IComponentRenderMode? PageRenderMode => HttpContext?.AcceptsInteractiveRouting() == true ? RenderMode.InteractiveServer : null;

    private readonly RiskManagementSettings _riskSettings = _settings.Value;
    private string _pathBase = "/";

    protected override void OnInitialized()
    {
        var pathBase = _riskSettings.PathBase;
        if (pathBase != null)
        {
            _pathBase = $"/{pathBase}/";
        }
    }
}
