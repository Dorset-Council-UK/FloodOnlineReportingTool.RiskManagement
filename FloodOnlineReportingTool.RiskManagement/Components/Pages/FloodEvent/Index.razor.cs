using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.RiskManagement.Converters;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using QRCoder;
using System.Net;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class Index(
    ILogger<Index> logger,
    IJSRuntime js,
    ISettingsService settingsService,
    IFloodEventRepository floodEventRepository,
    ICommonRepository commonRepository,
    NavigationManager navigationManager
)
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    // Static reverse lookup: OrganisationIds Guid → display name.
    // This avoids an extra DB call since organisation names are defined as constants in the contracts.
    private static readonly IReadOnlyDictionary<Guid, string> _organisationNames = new Dictionary<Guid, string>
    {
        [OrganisationIds.Dorset] = "Dorset Council",
        [OrganisationIds.Wessex] = "Wessex Water",
        [OrganisationIds.DevonCornwall] = "Devon & Cornwall",
    };
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];
    [Parameter]
    public required string Reference { get; set; }
    private string SelfUrl => navigationManager.Uri;

    private readonly CancellationTokenSource _cts = new();
    private EventOverviewViewModel? ViewModel { get; set; } = default!;
    private bool _isLoading = false;
    private bool _isExportingJson;

    private bool _isExportingCsv;
    private string? _exportErrorMessage;
    private string? _mediaUrlQrCodeDataUrl;
    private bool _showMediaUrlQrCode;
    private int _mediaUrlExtensionWeeks = 2;
    private string _mediaUrlExtensionWeeksText = "2";
    private string? _mediaUrlExtensionError;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        Breadcrumbs = CreateBreadcrumbs();

        if (string.IsNullOrWhiteSpace(Reference))
        {
            logger.LogInformation("No search reference provided in query parameters. Unable to load flood event details page.");
            navigationManager.NavigateTo(SearchPages.Options.Url);
        }

        try
        {
            // Don't get any data if you don't have permission
            if (AuthenticationState is null)
            {
                logger.LogWarning("Attempt to access flood event details page without authentication");
                return;
            }

            var authState = await AuthenticationState;
            if (!authState.User.IsAuthenticated)
            {
                logger.LogWarning("Attempt to access flood event details page without authentication");
                return;
            }

            var eventResult = await floodEventRepository.GetBySearchReference(Reference, 27700, _cts.Token);
            if (eventResult.IsSuccess && eventResult.ResultModel is not null)
            {
                var floodEvent = eventResult.ResultModel;

                var eventStatusesTask = floodEventRepository.GetEventStatuses(_cts.Token);
                var floodStatusesTask = commonRepository.GetFloodStatuses(_cts.Token);
                var investigationStatusesTask = commonRepository.GetInvestigationStatuses(_cts.Token);
                var primaryCausesTask = commonRepository.GetFloodProblems("Primary", _cts.Token);
                var secondaryCausesTask = commonRepository.GetFloodProblems("Secondary", _cts.Token);

                await Task.WhenAll(eventStatusesTask, floodStatusesTask, investigationStatusesTask, primaryCausesTask, secondaryCausesTask);

                var recordStatus = (await eventStatusesTask).FirstOrDefault(s => s.Id == floodEvent.RecordStatus)?.Text ?? string.Empty;

                var floodStatus = (await floodStatusesTask).FirstOrDefault(s => s.Id == floodEvent.FloodStatus)?.Text ?? string.Empty;

                var investigationStatus = (await investigationStatusesTask).FirstOrDefault(s => s.Id == floodEvent.InvestigationStatus)?.Text ?? string.Empty;

                // Resolve flood cause names from the SavedSearch.FloodSources Guids.
                // FloodSources maps to FloodProblem records (previously called "flood sources" but
                // renamed to "flood causes" to reduce confusion).
                var savedSearchFloodSources = floodEvent.SavedSearch.FloodSources.ToHashSet();
                var primaryCauseNames = (await primaryCausesTask)
                    .Where(p => savedSearchFloodSources.Contains(p.Id))
                    .Select(p => p.TypeName ?? string.Empty)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                var secondaryCauseNames = (await secondaryCausesTask)
                    .Where(p => savedSearchFloodSources.Contains(p.Id))
                    .Select(p => p.TypeName ?? string.Empty)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                // Resolve organisation names from the SavedSearch.LimitByOrganisation Guids using the
                // static reverse lookup of OrganisationIds constants.
                var responsibleOrganisationNames = (floodEvent.SavedSearch.LimitByOrganisation ?? [])
                    .Select(id => _organisationNames.TryGetValue(id, out var name) ? name : null)
                    .Where(name => name is not null)
                    .Select(name => name!)
                    .ToList();

                ViewModel = new EventOverviewViewModel
                {
                    SearchReference = floodEvent.SavedSearch.Reference,
                    SearchTitle = floodEvent.SavedSearch.SearchTitle,
                    UPRN = floodEvent.SavedSearch.UPRN,
                    USRN = floodEvent.SavedSearch.USRN,
                    Geom = floodEvent.SavedSearch.Geom,
                    CreatedAt = floodEvent.CreatedAt,
                    UpdatedAt = floodEvent.SavedSearch.UpdatedAt ?? floodEvent.CreatedAt,
                    UpdatedBy = floodEvent.SavedSearch.UpdatedBy ?? string.Empty,
                    FloodingStart = floodEvent.SavedSearch.ResultsFrom ?? default,
                    FloodingEnd = floodEvent.SavedSearch.ResultsTo ?? default,
                    RecordStatus = recordStatus,
                    FloodStatus = floodStatus,
                    InvestigationStatus = investigationStatus,
                    StatusNote = floodEvent.StatusNote,
                    EstimatedVulnerable = floodEvent.EstimatedVulnerable,
                    EstimatedUninhabitable = floodEvent.EstimatedUninhabitable,
                    EstimatedTotalExternalOnly = floodEvent.EstimatedTotalExternalOnly,
                    EstimatedTotalInternallyFlooded = floodEvent.EstimatedTotalInternallyFlooded,
                    EstimatedDisruptedUtilities = floodEvent.EstimatedDisruptedUtilities ?? 0,
                    EstimatedResidentialAffected = floodEvent.EstimatedResidentialAffected,
                    EstimatedResidentialFlooded = floodEvent.EstimatedResidentialFlooded,
                    EstimatedResidentialOther = floodEvent.EstimatedResidentialOther,
                    EstimatedCommercialAffected = floodEvent.EstimatedCommercialAffected,
                    EstimatedCommercialFlooded = floodEvent.EstimatedCommercialFlooded,
                    EstimatedCommercialOther = floodEvent.EstimatedCommercialOther,
                    EstimatedNoElectric = floodEvent.EstimatedNoElectric,
                    EstimatedNoGas = floodEvent.EstimatedNoGas,
                    EstimatedNoFoul = floodEvent.EstimatedNoFoul,
                    EstimatedNoWaterSupply = floodEvent.EstimatedNoWaterSupply,
                    PrimaryFloodCauses = primaryCauseNames,
                    SecondaryFloodCauses = secondaryCauseNames,
                    ResponsibleOrganisations = responsibleOrganisationNames,
                    MediaReference = floodEvent.MediaReference,
                    MediaReferenceExpiration = floodEvent.MediaReferenceExpiration,
                    MediaUploadLock = floodEvent.MediaUploadLock,
                };

                _mediaUrlQrCodeDataUrl = null;
                _showMediaUrlQrCode = false;
            }
            else
            {
                logger.LogWarning("Flood event details not found for reference");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the flood event with reference {Reference}.", Reference);
        }
        finally
        {
            _isLoading = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    { 

        if (ViewModel is not null)
        {
            //add to recent work store
            await settingsService.AddRecentWork($"{Reference}", "Flood Event", string.Empty, $"event/{Reference}");
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task CreatePublicMediaUrl()
    {
        var eventResult = await floodEventRepository.GetBySearchReference(Reference, 27700, _cts.Token);

        if (eventResult.IsSuccess && eventResult.ResultModel != null)
        {
            var mediaExpiry = DateTimeOffset.UtcNow.AddDays(14);
            var (isSuccess, mediaReference) = await floodEventRepository.CreateMediaUrl(eventResult.ResultModel.Id, mediaExpiry, _cts.Token);

            if (isSuccess && ViewModel is not null && !string.IsNullOrWhiteSpace(mediaReference))
            {
                ViewModel.MediaReference = mediaReference;
                ViewModel.MediaReferenceExpiration = mediaExpiry;
                ViewModel.MediaUploadLock = false;
                _mediaUrlQrCodeDataUrl = null;
                _showMediaUrlQrCode = false;
                return;
            }
        }

        logger.LogWarning("Attempt to add media URL failed");
        return;
    }

    private string GetMediaUrl()
    {
        if (ViewModel is null || string.IsNullOrEmpty(ViewModel.MediaReference))
        {
            return "";
        }

        var mediaPath = $"{EventPages.MediaUpload.Url.TrimEnd('/')}/{ViewModel.MediaReference}";
        return navigationManager.ToAbsoluteUri(mediaPath).ToString();
    }

    private MarkupString GetDisplayMediaUrl()
    {
        var mediaUrl = GetMediaUrl();
        if (string.IsNullOrWhiteSpace(mediaUrl))
        {
            return new MarkupString(string.Empty);
        }

        var encodedMediaUrl = WebUtility.HtmlEncode(mediaUrl)
            .Replace("/", "/<wbr>")
            .Replace("-", "-<wbr>")
            .Replace("_", "_<wbr>")
            .Replace("?", "?<wbr>")
            .Replace("&", "&<wbr>");

        return new MarkupString(encodedMediaUrl);
    }

    private string GetMediaUploadLockButtonText()
    {
        return ViewModel?.MediaUploadLock == true ? "Unlock media upload" : "Lock media upload";
    }

    private string? CreateMediaUrlQrCode(string mediaUrl)
    {
        if (string.IsNullOrWhiteSpace(mediaUrl))
        {
            return null;
        }

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(mediaUrl, QRCodeGenerator.ECCLevel.Q);
        var pngQrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = pngQrCode.GetGraphic(8);

        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
    }

    private Task ShowMediaUrlQrCode()
    {
        if (!_showMediaUrlQrCode)
        {
            _mediaUrlQrCodeDataUrl ??= CreateMediaUrlQrCode(GetMediaUrl());
            _showMediaUrlQrCode = true;
        }

        return Task.CompletedTask;
    }

    private async Task ToggleMediaUploadLock()
    {
        if (ViewModel is null || string.IsNullOrWhiteSpace(ViewModel.MediaReference))
        {
            return;
        }

        var eventResult = await floodEventRepository.GetBySearchReference(Reference, 27700, _cts.Token);
        if (!eventResult.IsSuccess || eventResult.ResultModel is null)
        {
            logger.LogWarning("Attempt to toggle media upload lock failed while retrieving flood event for reference {Reference}", Reference);
            return;
        }

        var targetLockState = !ViewModel.MediaUploadLock;
        var isSuccess = await floodEventRepository.SetMediaUploadLock(eventResult.ResultModel.Id, targetLockState, _cts.Token);
        if (!isSuccess)
        {
            logger.LogWarning("Attempt to toggle media upload lock failed for flood event {FloodEventId}", eventResult.ResultModel.Id);
            return;
        }

        ViewModel.MediaUploadLock = targetLockState;
    }

    private async Task ExtendMediaUrlExpiry()
    {
        _mediaUrlExtensionError = null;

        if (ViewModel is null || string.IsNullOrWhiteSpace(ViewModel.MediaReference))
        {
            return;
        }

        if (!int.TryParse(_mediaUrlExtensionWeeksText, out var extensionWeeks) || extensionWeeks <= 0 || extensionWeeks > 52)
        {
            _mediaUrlExtensionError = "Enter a valid number of weeks between 1 and 52.";
            return;
        }

        _mediaUrlExtensionWeeks = extensionWeeks;

        var eventResult = await floodEventRepository.GetBySearchReference(Reference, 27700, _cts.Token);
        if (!eventResult.IsSuccess || eventResult.ResultModel is null)
        {
            logger.LogWarning("Attempt to extend media URL expiry failed while retrieving flood event for reference {Reference}", Reference);
            return;
        }

        var mediaExpiry = DateTimeOffset.UtcNow.AddDays(_mediaUrlExtensionWeeks * 7);
        var isSuccess = await floodEventRepository.UpdateMediaUrlExpiry(eventResult.ResultModel.Id, mediaExpiry, _cts.Token);
        if (!isSuccess)
        {
            logger.LogWarning("Attempt to extend media URL expiry failed for flood event {FloodEventId}", eventResult.ResultModel.Id);
            return;
        }

        ViewModel.MediaReferenceExpiration = mediaExpiry;
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
            // page is in British National Grid (SRID 27700), so re-fetch it reprojected for export.
            const int wgs84Srid = 4326;
            var wgs84EventResult = await floodEventRepository.GetBySearchReference(Reference, wgs84Srid, _cts.Token);
            var geom = wgs84EventResult.ResultModel?.SavedSearch.Geom;

            var geoJsonStream = await GeoJsonConverter.CreateGeoJsonAsync(
                new EventOverviewViewModelToGeoJson(),
                ViewModel,
                geom,
                _cts.Token);

            if (geoJsonStream is null)
            {
                return;
            }

            await js.InvokeVoidAsync("downloadFileFromStream", 
                _cts.Token,
                EventOverviewViewModelToGeoJson.CreateFileName(Reference),
                geoJsonStream
            );
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
    /// Exports the flood event details to a CSV file and triggers a download in the browser.
    /// </summary>
    /// <returns></returns>
    private async Task ExportToCsv()
    {
        _isExportingCsv = true;
        _exportErrorMessage = null;
        try
        {
            await js.InvokeVoidAsync("downloadFileFromStream", 
                _cts.Token,
                EventOverviewViewModelToCSV.CreateFileName(Reference),
                CSVConverter.CreateCsv(new EventOverviewViewModelToCSV(), ViewModel)
            );
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
            new ($"{EventPages.View.Url}/{Reference}", $"Flood event {Reference}"),
        ];
    }
}