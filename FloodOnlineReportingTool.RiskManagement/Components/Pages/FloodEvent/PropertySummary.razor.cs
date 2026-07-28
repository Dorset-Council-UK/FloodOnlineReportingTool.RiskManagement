using FloodOnlineReportingTool.RiskManagement.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class PropertySummary(
    ILogger<PropertySummary> logger)
{
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];
    private string Title => EventPages.PropertySummary.Title;

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }
    private bool _isLoading = true;
    [Parameter]
    public required string Reference { get; set; }

    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        Breadcrumbs = CreateBreadcrumbs();

        // Don't get any data if you don't have permission.
        AuthenticationState? authState;
        if (AuthenticationState is null)
        {
            logger.LogWarning("Attempt to access flood event property summary without authentication");
            return;
        }
        else
        {
            authState = await AuthenticationState;
            if (!authState.User.IsAuthenticated)
            {
                logger.LogWarning("Attempt to access flood event property summary without authentication");
                return;
            }
        }

        _isLoading = false;
    }

    private IReadOnlyCollection<GdsBreadcrumb> CreateBreadcrumbs()
    {
        return
        [
            GeneralPages.Home.ToGdsBreadcrumb(),
            new ($"{EventPages.View.Url}/{Reference}", $"Flood event {Reference}"),
            new ($"{EventPages.PropertySummary.Url}/{Reference}", "Property Summary")
        ];
    }
}