using FloodOnlineReportingTool.RiskManagement.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;

namespace FloodOnlineReportingTool.RiskManagement.Components;

public partial class App(IOptions<RiskManagementOptions> _options, GdsBlazorComponents.IGdsJsInterop gdsJs)
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    // The Render Mode is InteractiveServer most of the time. Except on pages which are set as Static server-side rendering (static SSR)
    private IComponentRenderMode? PageRenderMode => HttpContext?.AcceptsInteractiveRouting() == true ? RenderMode.InteractiveServer : null;

    private readonly RiskManagementOptions _riskManagementOptions = _options.Value;
    private string _pathBase = "/";

    protected override void OnInitialized()
    {
        var pathBase = _riskManagementOptions.PathBase;
        if (pathBase != null)
        {
            _pathBase = $"/{pathBase}/";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await gdsJs.InitGds();
        }
    }
}
