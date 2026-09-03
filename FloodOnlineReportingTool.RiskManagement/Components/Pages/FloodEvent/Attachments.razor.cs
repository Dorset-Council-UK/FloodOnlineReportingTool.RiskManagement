using FloodOnlineReportingTool.RiskManagement.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class Attachments(
    ILogger<Attachments> logger,
    NavigationManager navigationManager)
{
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];
    private string Title => EventPages.Attachments.Title;

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }
    private bool _isLoading = true;
    [Parameter]
    public required string Reference { get; set; }
    private Database.Models.FloodEvents.FloodEvent? FloodEvent { get; set; }

    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        Breadcrumbs = CreateBreadcrumbs();

        // Don't get any data if you don't have permission.
        AuthenticationState? authState;
        if (AuthenticationState is null)
        {
            logger.LogWarning("Attempt to access flood event attachments without authentication");
            return;
        }
        else
        {
            authState = await AuthenticationState;
            if (!authState.User.IsAuthenticated)
            {
                logger.LogWarning("Attempt to access flood event attachments without authentication");
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
            new ($"{EventPages.Attachments.Url}/{Reference}", "Attachments")
        ];
    }

    private void GoToUpload()
    {
        if (Reference is null)
        {
            navigationManager.NavigateTo($"{EventPages.Attachments.Url}/{Reference}");
        }

        navigationManager.NavigateTo($"{EventPages.Upload.Url}/{Reference}/true");
    }
}