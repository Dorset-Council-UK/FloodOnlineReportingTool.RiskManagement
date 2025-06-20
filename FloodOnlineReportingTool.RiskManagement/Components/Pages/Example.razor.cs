using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages;

public partial class Example(
    ILogger<Example> logger,
    IGdsJsInterop gdsJs,
    NavigationManager navigationManager
)
{
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        new ("", "Home"),
        new ("somewhere", "Somewhere else"),
    ];

    private Models.Example Model { get; set; } = default!;
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
        logger.LogInformation("Form submitted successfully with first name: '{FirstName}' some number: '{SomeNumber}'", Model.FirstName, Model.SomeNumber);
        navigationManager.NavigateTo("");
        //navigationManager.NavigateToLocal(""); // extension method to navigate to the local URL
    }
}
