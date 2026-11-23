using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Identity.Web;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class OmniboxResults(
    ILogger<OmniboxResults> logger,
    IReferenceSearchService referenceSearchService,
    MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler,
    NavigationManager navigationManager) : IAsyncDisposable
{
    private const int MaxResults = 50;

    [Parameter]
    public string? Query { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private IReadOnlyList<OmniboxSearchResult> _results = [];
    private bool _isLoading = true;
    private string? _errorMessage;

    private bool ResultLimitReached => _results.Count >= MaxResults;

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            navigationManager.NavigateTo(GeneralPages.Home.Url);
            return;
        }

        _isLoading = true;
        _errorMessage = null;

        try
        {
            _results = await referenceSearchService.Search(Query, MaxResults, _cts.Token);
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            logger.LogWarning(ex, "Consent required while loading omnibox search results");
            consentHandler.HandleException(ex);
        }
        catch (OperationCanceledException) when (_cts.IsCancellationRequested)
        {
            // Expected when leaving the results page.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while loading omnibox search results");
            _errorMessage = "There was a problem loading search results. Try again.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _cts.Dispose();
    }
}

