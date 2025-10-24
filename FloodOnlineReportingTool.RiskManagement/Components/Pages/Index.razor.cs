using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages;

public partial class Index(IGdsJsInterop gdsJs, IReportStatusApiService reportStatusApiService, NavigationManager navigationManager)
{
    public string Title { get; set; } = "Flood Reporting Admin";
    private bool _isLoading = true;
    private EditContext _editContext = default!;

    private Models.Searches.HomeSearch Model { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await gdsJs.InitGds();

            _isLoading = false;
            StateHasChanged();
        }
    }

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    private async Task OnValidSubmit()
    {
        await reportStatusApiService.Test();
        var tempGuid = Guid.Empty;
        navigationManager.NavigateTo($"flood-report/details/{Uri.EscapeDataString(tempGuid.ToString())}");
    }
}
