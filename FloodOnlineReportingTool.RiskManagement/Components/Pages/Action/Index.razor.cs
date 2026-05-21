using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Action;

public partial class Index(
    ILogger<Index> logger,
    NavigationManager navigationManager)
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [Parameter]
    public string? ActionRef { get; set; }

    private bool _notPermitted = false;

    protected override async Task OnInitializedAsync()
    {
        // Don't get any data if you don't have permission
        if (AuthenticationState is null)
        {
            _notPermitted = true;
            logger.LogWarning("Attempt to access action logs without authentication");
        }
    }
}