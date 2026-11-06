using FloodOnlineReportingTool.Database.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using DbFloodEvent = FloodOnlineReportingTool.Database.Models.FloodEvents.FloodEvent;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.RecentFloodEvents;

public partial class RecentFloodEventList(
    IFloodEventRepository floodEventRepository,
    ILogger<RecentFloodEventList> logger
) : IAsyncDisposable
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }
    private readonly CancellationTokenSource _cts = new();

    private IReadOnlyList<DbFloodEvent> FloodEventsInPast30Days { get; set; } = [];
    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        // Don't get any data if you don't have permission
        AuthenticationState? authState;
        if (AuthenticationState is null)
        {
            logger.LogWarning("Attempt to access recent flood event list without authentication");
            return;
        }
        else
        {
            authState = await AuthenticationState;
            if (!authState.User.IsAuthenticated)
            {
                logger.LogWarning("Attempt to access recent flood event list without authentication");
                return;
            }
        }

        try
        {
            FloodEventsInPast30Days = await floodEventRepository.GetFloodEventsFromPast30Days(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected during normal cancellation, no action required.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed loading flood events recorded in the past 30 days.");
            FloodEventsInPast30Days = [];
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (OperationCanceledException)
        {
            // Expected during normal cancellation, no action required.
        }
    }
}