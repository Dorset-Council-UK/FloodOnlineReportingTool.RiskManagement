using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodReport;

public partial class Details(
    ILogger<Details> logger,
    IReportStatusClient reportStatusClient,
    ICommonRepository commonRepository,
    IActionLogRepository actionLogRepository,
    ISettingsService settingsService,
    MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
{
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        new("", "Home"),
    ];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private bool _gettingToken;
    private bool _isLoading = true;
    private bool _notFound = false;
    private string ReportStatus { get; set; }
    private Guid? AcknowledgeAction { get; set; }

    // Shared EditContext for all action buttons on the page - errors from any action
    // are pushed here and rendered once by a single GdsErrorSummary.
    private readonly object _actionsModel = new();
    private EditContext? _actionsEditContext;
    private ValidationMessageStore? _actionsMessageStore;

    [Parameter]
    public required string Reference { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private SearchResultFloodReport? ViewModel { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _actionsEditContext = new EditContext(_actionsModel);
        _actionsMessageStore = new ValidationMessageStore(_actionsEditContext);

        // Don't get any data if you don't have permission
        AuthenticationState? authState;
        if (AuthenticationState is null)
        {
            logger.LogWarning("Attempt to access flood report details without authentication");
            return;
        }
        else
        {
            authState = await AuthenticationState;
            if (!authState.User.IsAuthenticated)
            {
                logger.LogWarning("Attempt to access flood report details without authentication");
                return;
            }
        }

        try
        {
            _gettingToken = false;

            // GetFloodReportDetail returns SearchResultFloodReport, fully mapped and ready for display.
            // SearchFloodReportByReference returns FloodReportWithSourceReferences (raw API shape) and
            // should not be used here as it does not contain the full data needed for display.
            var reportResult = await reportStatusClient.GetFloodReportDetail(Reference, true, _cts.Token);

            if (reportResult.IsSuccess && reportResult.ResultModel is not null)
            {
                var reportStatuses = await commonRepository.GetReportStatuses(_cts.Token);
                ViewModel = reportResult.ResultModel;
                ReportStatus = reportStatuses.Where(rs => rs.Id == ViewModel.RecordStatusId).Select(rs => rs.Text).FirstOrDefault() ?? "";

                AcknowledgeAction = await actionLogRepository.GetAcknowledgeActionByReport(ViewModel.Reference, _cts.Token);
                logger.LogInformation("Flood report detail loaded successfully for reference");
            }
            else
            {
                _notFound = true;
                logger.LogWarning("Flood report detail not found for reference");
            }
        }
        catch (Exception ex)
        {
            // Handles MsalUiRequiredException / MicrosoftIdentityWebChallengeUserException.
            // consentHandler.HandleException will trigger a redirect to re-authenticate
            // or prompt for incremental consent, then return the user to this page.
            _gettingToken = true;
            consentHandler.HandleException(ex);
        }
        _isLoading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (ViewModel is not null)
        {
            // add to recent work store (only once per page load)
            await settingsService.AddRecentWork($"{Reference}", "Flood Report", "", $"report/details/{Reference}");
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// Acknowledges that the current user has read the flood report.
    /// Any errors are pushed into the shared <see cref="_actionsMessageStore"/> so that
    /// <see cref="GdsErrorSummary"/> renders them using the standard GOV.UK pattern.
    /// </summary>
    private async Task AcknowledgeAsync()
    {
        _actionsMessageStore!.Clear();

        if (AcknowledgeAction is not Guid acknowledgeAction)
        {
            _actionsMessageStore.Add(new FieldIdentifier(_actionsModel, string.Empty),
                "Unable to acknowledge the report: no pending acknowledgement action was found.");
            _actionsEditContext!.NotifyValidationStateChanged();
            return;
        }

        var currentUserDisplayName = string.Empty;
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            currentUserDisplayName = authState.User.GetDisplayName();
        }

        if (string.IsNullOrEmpty(currentUserDisplayName))
        {
            _actionsMessageStore.Add(new FieldIdentifier(_actionsModel, string.Empty),
                "Unable to acknowledge the report: the current user could not be identified.");
            _actionsEditContext!.NotifyValidationStateChanged();
            return;
        }

        var acknowledged = await actionLogRepository.AcknowledgeReport(acknowledgeAction, currentUserDisplayName, _cts.Token);
        if (!acknowledged)
        {
            _actionsMessageStore.Add(new FieldIdentifier(_actionsModel, string.Empty),
                "Unable to acknowledge the report at this time. Please try again.");
            _actionsEditContext!.NotifyValidationStateChanged();
            return;
        }

        AcknowledgeAction = await actionLogRepository.GetAcknowledgeActionByReport(ViewModel!.Reference, _cts.Token);
    }
}