using DbFloodEvent = FloodOnlineReportingTool.Database.Models.FloodEvents.FloodEvent;
using FloodOnlineReportingTool.Database.Repositories;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.RecentFloodEvents;

public partial class RecentFloodEventList(
    IFloodEventRepository floodEventRepository,
    ILogger<RecentFloodEventList> logger
) : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();

    private IReadOnlyList<DbFloodEvent> FloodEventsInPast30Days { get; set; } = [];
    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
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