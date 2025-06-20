using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages;

public partial class Index(IGdsJsInterop gdsJs)
{
    private bool _isLoading = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await gdsJs.InitGds();

            _isLoading = false;
            StateHasChanged();
        }
    }
}
