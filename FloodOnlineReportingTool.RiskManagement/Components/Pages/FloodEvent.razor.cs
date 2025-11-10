using FloodOnlineReportingTool.RiskManagement.Models;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages;

public partial class FloodEvent(
    ILogger<FloodEvent> logger,
    IGdsJsInterop gdsJs,
    NavigationManager navigationManager
) {
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        new ("", "Home"),
    ];

    private FloodEventViewModel Model { get; set; } = default!;
    private EditContext _editContext = default!;
    private bool _isLoading = true;
    private readonly CancellationTokenSource _cts = new();
    private readonly IReadOnlyCollection<GdsOptionItem<string>> FloodStatusOptions = [
        new ("12345", "Yes this is a flood event", "yes"),
        new ("12346", "No I'm just looking at the moment", "no")
        ];
    private IReadOnlyCollection<GdsOptionItem<Guid>> FloodAuthorityOptions = [];

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());

        FloodAuthorityOptions = [
            new ("1234546464", "Lead Local Flood Authority (LLFA)", new Guid("81c82725-d700-445d-8eee-050f58df7fdd")),
            new ("1234565465", "Environment Agency", new Guid("e24a9ab5-08de-41ab-8f37-675dd35cbf00")),
            new ("365436453", "Other", new Guid(), isExclusive:true)
        ];
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

    private void OnValidSubmit()
    {
        logger.LogInformation("Flood Event submitted successfuly");
        navigationManager.NavigateTo("");
    }
}
