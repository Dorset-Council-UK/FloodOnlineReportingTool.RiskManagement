using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.RiskManagement.Converters;
using FloodOnlineReportingTool.RiskManagement.Extensions;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Report;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Identity.Web;
using Microsoft.JSInterop;
using NetTopologySuite.Geometries;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodReport;

public partial class Details(
    ILogger<Details> logger,
    IReportStatusClient reportStatusClient,
    ICommonRepository commonRepository,
    IActionLogRepository actionLogRepository,
    ISettingsService settingsService,
    NavigationManager navigationManager,
    IJSRuntime js,
    MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
{
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [Parameter]
    public required string Reference { get; set; }

    private bool _gettingToken;
    private bool _isLoading = true;
    private bool _notFound = false;
    private string ReportStatus { get; set; } = string.Empty;
    private Guid? AcknowledgeAction { get; set; }
    private string _selfUrl => navigationManager.Uri;

    // Shared EditContext for all action buttons on the page - errors from any action
    // are pushed here and rendered once by a single GdsErrorSummary.
    private readonly object _actionsModel = new();
    private EditContext? _actionsEditContext;
    private ValidationMessageStore? _actionsMessageStore;

    private readonly CancellationTokenSource _cts = new();
    private ReportOverviewViewModel? ViewModel { get; set; } = default!;

    private bool _isExportingJson;

    private bool _isExportingCsv;
    private string? _exportErrorMessage;

    protected override async Task OnInitializedAsync()
    {
        _actionsEditContext = new EditContext(_actionsModel);
        _actionsMessageStore = new ValidationMessageStore(_actionsEditContext);

        Breadcrumbs = CreateBreadcrumbs();

        // Don't get any data if you don't have permission.
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
            var reportResult = await reportStatusClient.GetFloodReportOverview(Reference, _cts.Token);

            if (reportResult.IsSuccess && reportResult.ResultModel is not null)
            {
                ViewModel = reportResult.ResultModel;
                var reportStatuses = await commonRepository.GetReportStatuses(_cts.Token);
                ReportStatus = reportStatuses.Where(rs => rs.Id == ViewModel.RecordStatusId).Select(rs => rs.Text).FirstOrDefault() ?? string.Empty;

                AcknowledgeAction = await actionLogRepository.GetAcknowledgeActionByReport(ViewModel.Reference, _cts.Token);
                logger.LogInformation("Flood report details loaded successfully for reference");
            }
            else
            {
                _notFound = true;
                logger.LogWarning("Flood report details not found for reference");
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
            await settingsService.AddRecentWork($"{Reference}", "Flood Report", string.Empty, $"report/details/{Reference}");
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

    private async Task PrintPageAsync()
    {
        await js.InvokeVoidAsync("print");
    }

    /// <summary>
    /// Exports the flood event details to a GeoJSON file and triggers a download in the browser.
    /// </summary>
    /// <returns></returns>
    private async Task ExportToJson()
    {
        _isExportingJson = true;
        _exportErrorMessage = null;
        try
        {
            if (ViewModel is null) throw new InvalidOperationException("ViewModel was null when exporting to GeoJSON");
            // GeoJSON requires coordinates in WGS84 (SRID 4326), but the geometry loaded for the
            // page is in British National Grid (SRID 27700), so use the Lat/Lon values instead.
            var geom = new Point(decimal.ToDouble(ViewModel.Longitude), decimal.ToDouble(ViewModel.Latitude));

            var geoJsonStream = await GeoJsonConverter.CreateGeoJsonAsync(
                new ReportOverviewViewModelToGeoJson(),
                ViewModel,
                geom,
                _cts.Token);

            if (geoJsonStream is null)
            {
                _exportErrorMessage = "An error occurred while preparing the export. Please try again.";
                return;
            }

            await js.InvokeVoidAsync("downloadFileFromStream", _cts.Token,
                ReportOverviewViewModelToGeoJson.CreateFileName(Reference),
                geoJsonStream);
        }
        catch (Exception ex)
        {
            _exportErrorMessage = "An error occurred while preparing the export. Please try again.";
            logger.LogError(ex, _exportErrorMessage);
        }
        finally
        {
            _isExportingJson = false;
        }
    }

    /// <summary>
    /// Exports the flood report details to a CSV file and triggers a download in the browser.
    /// </summary>
    /// <returns></returns>
    private async Task ExportToCsv()
    {
        _isExportingCsv = true;
        _exportErrorMessage = null;
        try
        {
            await js.InvokeVoidAsync("downloadFileFromStream", _cts.Token,
            ReportOverviewViewModelToCSV.CreateFileName(Reference),
            CSVConverter.CreateCsv(new ReportOverviewViewModelToCSV(), ViewModel));
        }
        catch (Exception ex)
        {
            _exportErrorMessage = "An error occurred while preparing the export. Please try again.";
            logger.LogError(ex, _exportErrorMessage);
        }
        finally
        {
            _isExportingCsv = false;
        }
    }

    private IReadOnlyCollection<GdsBreadcrumb> CreateBreadcrumbs()
    {
        return
        [
            GeneralPages.Home.ToGdsBreadcrumb(),
            new ($"{ReportPages.Details.Url}/{Reference}", $"Flood report {Reference}")
        ];
    }
}