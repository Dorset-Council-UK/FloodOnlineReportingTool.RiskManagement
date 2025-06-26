using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages;

public partial class FloodEvent(
    ILogger<FloodEvent> logger,
    IGdsJsInterop gdsJs,
    NavigationManager navigationManager
)
{
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        new ("", "Home"),
    ];

    private Models.FloodEvent Model { get; set; } = default!;
    private EditContext _editContext = default!;
    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await gdsJs.InitGds();

            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task OnValidSubmit()
    {
        logger.LogInformation("Flood Event submitted successfuly");
        navigationManager.NavigateTo("");
        //navigationManager.NavigateToLocal(""); // extension method to navigate to the local URL
    }
}
