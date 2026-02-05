using FloodOnlineReportingTool.RiskManagement.Models;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages;

public partial class FloodEvent(
    ILogger<FloodEvent> logger,
    NavigationManager navigationManager
) : IDisposable
{
    // Fields
    private EditContext? _editContext;
    private CancellationTokenSource? _cts;
    private IReadOnlyCollection<GdsBreadcrumb>? Breadcrumbs;
    private IReadOnlyCollection<GdsOptionItem<Guid>>? FloodAuthorityOptions;
    private IReadOnlyCollection<GdsOptionItem<string>>? FloodStatusOptions;

    // Properties
    private FloodEventViewModel? Model { get; set; }

    protected override void OnInitialized()
    {
        _cts ??= new();

        // Setup model and edit context
        Model ??= new();
        if (_editContext is null)
        {
            _editContext = new(Model);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        }

        Breadcrumbs ??= [
            new ("", "Home"),
        ];

        FloodAuthorityOptions ??= [
            new ("1234546464", "Lead Local Flood Authority (LLFA)", new Guid("81c82725-d700-445d-8eee-050f58df7fdd")),
            new ("1234565465", "Environment Agency", new Guid("e24a9ab5-08de-41ab-8f37-675dd35cbf00")),
            new ("365436453", "Other", new Guid(), isExclusive:true),
        ];

        FloodStatusOptions ??= [
            new ("12345", "Yes this is a flood event", "yes"),
            new ("12346", "No I'm just looking at the moment", "no"),
        ];
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnValidSubmit()
    {
        logger.LogInformation("Flood Event submitted successfuly");
        navigationManager.NavigateTo("");
    }
}
