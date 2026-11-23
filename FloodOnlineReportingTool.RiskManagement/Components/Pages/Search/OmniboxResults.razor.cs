using FloodOnlineReportingTool.Database.Models.Enums;
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
    [Parameter]
    public string? Query { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private IReadOnlyList<OmniboxSearchResult> _results = [];
    private bool _isLoading = true;
    private string? _errorMessage;

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
            _results = await referenceSearchService.Search(Query, 50, _cts.Token);
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

    private static string GetResultUrl(OmniboxSearchResult result)
    {
        return result.ResultType switch
        {
            RecordTypes.Event => $"{EventPages.View.Url}/{Uri.EscapeDataString(result.Reference)}",
            RecordTypes.Search => $"{SearchPages.Results.Url}/{Uri.EscapeDataString(result.Reference)}",
            RecordTypes.Report => $"{ReportPages.Details.Url}/{Uri.EscapeDataString(result.Reference)}",
            _ => GeneralPages.Home.Url,
        };
    }

    private static string GetResultType(OmniboxSearchResult result)
    {
        return result.ResultType switch
        {
            RecordTypes.Event => "Flood event",
            RecordTypes.Search => "Saved search",
            RecordTypes.Report => "Flood report",
            _ => "Record",
        };
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _cts.Dispose();
    }
}
