using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.RiskManagement.Converters;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

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
    private string _selfUrl => navigationManager.Uri;
    private bool _notFound = false;

    private readonly CancellationTokenSource _cts = new();
    private EventOverviewViewModel? ViewModel { get; set; } = default!;
    private bool _isExportingJson;

    private bool _isExportingCsv;
    private string? _exportErrorMessage;
    private bool _redirectingToSearchOptions;

    protected override async Task OnInitializedAsync()
    {
        Breadcrumbs = CreateBreadcrumbs();

        if (string.IsNullOrWhiteSpace(Reference))
        {
            logger.LogInformation("No search reference provided in query parameters. Unable to load search results.");
            _redirectingToSearchOptions = true;
            navigationManager.NavigateTo(SearchPages.Options.Url);
        }

        try
        {
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
                };
            }
            else
            {
                _notFound = true;
                logger.LogWarning("Flood event details not found for reference");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the flood event with reference {Reference}.", Reference);
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