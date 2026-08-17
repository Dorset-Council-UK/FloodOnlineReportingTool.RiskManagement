using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Lookups;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.RiskManagement.Extensions;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Report;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using System.Text;
using System.Text.Json;

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

    private bool _redirectingToSearchOptions;

    private bool _isExportingJson;
    private string JsonDownloadFileName => $"flood-event-{Reference}.geojson";

    private bool _isExportingCsv;
    private string CsvDownloadFileName => $"flood-event-{Reference}.csv";

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

                var recordStatus = eventStatusesTask.Result
                    .FirstOrDefault(s => s.Id == floodEvent.RecordStatus)?.Text ?? string.Empty;

                var floodStatus = floodStatusesTask.Result
                    .FirstOrDefault(s => s.Id == floodEvent.FloodStatus)?.Text ?? string.Empty;

                var investigationStatus = investigationStatusesTask.Result
                    .FirstOrDefault(s => s.Id == floodEvent.InvestigationStatus)?.Text ?? string.Empty;

                // Resolve flood cause names from the SavedSearch.FloodSources Guids.
                // FloodSources maps to FloodProblem records (previously called "flood sources" but
                // renamed to "flood causes" to reduce confusion).
                var savedSearchFloodSources = floodEvent.SavedSearch.FloodSources.ToHashSet();
                var primaryCauseNames = primaryCausesTask.Result
                    .Where(p => savedSearchFloodSources.Contains(p.Id))
                    .Select(p => p.TypeName ?? string.Empty)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                var secondaryCauseNames = secondaryCausesTask.Result
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

    private string? _exportErrorMessage;

    //private async Task DownloadGeoJson()
    //{
    //    if (_isExportingJson) {  return; }
    //    _isExportingJson = true;

    //    try
    //    {
    //        var fileStream = await ExportToJsonAsync();
    //        if (fileStream is null)  {  return; }

    //        using (fileStream)
    //        using (var streamReference = new DotNetStreamReference(fileStream))
    //        {
    //            await js.InvokeVoidAsync("downloadFileFromStream", JsonDownloadFileName, streamReference);
    //        }
    //    }
    //    finally
    //    {
    //        _isExportingJson = false;
    //    }
    //}

    private string ExportToJson()
    {
        if (ViewModel is not EventOverviewViewModel model)
        {
            return string.Empty;
        }
        var exportDto = model.ToExportDto();
        if (exportDto is null)
        {
            // Invalid record
            return string.Empty;
        }
        var feature = new
        {
            type = "Feature",
            geometry = new
            {
                type = "MultiPolygon",
                coordinates = exportDto.Geom.Geometries
                        .OfType<NetTopologySuite.Geometries.Polygon>()
                        .Select(polygon =>
                            new[] { polygon.ExteriorRing }
                                .Concat(Enumerable.Range(0, polygon.NumInteriorRings)
                                    .Select(polygon.GetInteriorRingN))
                                .Select(ring => ring.Coordinates
                                    .Select(c => new[] { c.X, c.Y })
                                    .ToArray())
                                .ToArray())
                        .ToArray()
            },
            properties = new
            {
                exportDto.Reference,
                exportDto.Title,
                exportDto.FloodingStart,
                exportDto.FloodingEnd,
                exportDto.PrimaryFloodCauses,
                exportDto.SecondaryFloodCauses,
                exportDto.ResponsibleOrganisations,
                exportDto.UPRN,
                exportDto.USRN,
                exportDto.RecordStatus,
                exportDto.FloodStatus,
                exportDto.InvestigationStatus,
                exportDto.EstimatedVulnerable,
                exportDto.EstimatedUninhabitable,
                exportDto.EstimatedTotalExternalOnly,
                exportDto.EstimatedTotalInternallyFlooded,
                exportDto.EstimatedDisruptedUtilities,
                exportDto.EstimatedResidentialAffected,
                exportDto.EstimatedResidentialFlooded,
                exportDto.EstimatedResidentialOther,
                exportDto.EstimatedCommercialAffected,
                exportDto.EstimatedCommercialOther,
                exportDto.EstimatedCommercialFlooded,
                exportDto.EstimatedNoElectric,
                exportDto.EstimatedNoGas,
                exportDto.EstimatedNoFoul,
                exportDto.EstimatedNoWaterSupply,
                exportDto.StatusNote
            }
        };

        var json = JsonSerializer.Serialize(feature, new JsonSerializerOptions
        {
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
        });

        return $"data:application/geo+json;charset=utf-8,{Uri.EscapeDataString(json)}";
    }

    //private Task<MemoryStream?> ExportToJsonAsync()
    //{
    //    _exportErrorMessage = null;
    //    if (ViewModel is not EventOverviewViewModel model)
    //    {
    //        _exportErrorMessage = "Unable to export: the view model is not available.";
    //        logger.LogWarning(_exportErrorMessage);
    //        return Task.FromResult<MemoryStream?>(null);
    //    }

    //    var exportDto = model.ToExportDto();
    //    if (exportDto is null || exportDto.Geom is null || exportDto.Geom.IsEmpty)
    //    {
    //        _exportErrorMessage = "Unable to export: the geometry data is invalid or empty.";
    //        logger.LogWarning(_exportErrorMessage);
    //        return Task.FromResult<MemoryStream?>(null);
    //    }

    //    try
    //    {
    //        var feature = new
    //        {
    //            type = "Feature",
    //            geometry = new
    //            {
    //                type = "MultiPolygon",
    //                coordinates = exportDto.Geom.Geometries
    //                    .OfType<NetTopologySuite.Geometries.Polygon>()
    //                    .Select(polygon =>
    //                        new[] { polygon.ExteriorRing }
    //                            .Concat(Enumerable.Range(0, polygon.NumInteriorRings)
    //                                .Select(polygon.GetInteriorRingN))
    //                            .Select(ring => ring.Coordinates
    //                                .Select(c => new[] { c.X, c.Y })
    //                                .ToArray())
    //                            .ToArray())
    //                    .ToArray()
    //            },
    //            properties = new
    //            {
    //                exportDto.Reference,
    //                exportDto.Title,
    //                exportDto.FloodingStart,
    //                exportDto.FloodingEnd,
    //                exportDto.PrimaryFloodCauses,
    //                exportDto.SecondaryFloodCauses,
    //                exportDto.ResponsibleOrganisations,
    //                exportDto.UPRN,
    //                exportDto.USRN,
    //                exportDto.RecordStatus,
    //                exportDto.FloodStatus,
    //                exportDto.InvestigationStatus,
    //                exportDto.EstimatedVulnerable,
    //                exportDto.EstimatedUninhabitable,
    //                exportDto.EstimatedTotalExternalOnly,
    //                exportDto.EstimatedTotalInternallyFlooded,
    //                exportDto.EstimatedDisruptedUtilities,
    //                exportDto.EstimatedResidentialAffected,
    //                exportDto.EstimatedResidentialFlooded,
    //                exportDto.EstimatedResidentialOther,
    //                exportDto.EstimatedCommercialAffected,
    //                exportDto.EstimatedCommercialOther,
    //                exportDto.EstimatedCommercialFlooded,
    //                exportDto.EstimatedNoElectric,
    //                exportDto.EstimatedNoGas,
    //                exportDto.EstimatedNoFoul,
    //                exportDto.EstimatedNoWaterSupply,
    //                exportDto.StatusNote
    //            }
    //        };

    //        var json = JsonSerializer.Serialize(feature, new JsonSerializerOptions
    //        {
    //            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
    //        });

    //        var bytes = Encoding.UTF8.GetBytes(json);
    //        var stream = new MemoryStream(bytes);

    //        return Task.FromResult<MemoryStream?>(stream);
    //    }
    //    catch (Exception ex)
    //    {
    //        _exportErrorMessage = "An error occurred while preparing the export. Please try again.";
    //        logger.LogError(ex, _exportErrorMessage);
    //        return Task.FromResult<MemoryStream?>(null);
    //    }
    //}

    private async Task DownloadCsv()
    {
        if (_isExportingCsv) { return; }
        _isExportingCsv = true;

        try
        {
            var fileStream = await ExportToCsvAsync();
            if (fileStream is null) { return; }

            using (fileStream)
            using (var streamReference = new DotNetStreamReference(fileStream))
            {
                await js.InvokeVoidAsync("downloadFileFromStream", CsvDownloadFileName, streamReference);
            }
        }
        finally
        {
            _isExportingCsv = false;
        }
    }

    private Task<MemoryStream?> ExportToCsvAsync()
    {
        _exportErrorMessage = null;
        if (ViewModel is not EventOverviewViewModel model)
        {
            _exportErrorMessage = "Unable to export: the view model is not available.";
            logger.LogWarning(_exportErrorMessage);
            return Task.FromResult<MemoryStream?>(null);
        }

        var exportDto = model.ToExportDto();
        if (exportDto is null)
        {
            _exportErrorMessage = "Unable to export: the export data is invalid.";
            logger.LogWarning(_exportErrorMessage);
            return Task.FromResult<MemoryStream?>(null);
        }

        try
        {
            var csv = new StringBuilder()
                .AppendLine("Reference,Title,FloodingStart,FloodingEnd,PrimaryFloodCauses,SecondaryFloodCauses,ResponsibleOrganisations,UPRN,USRN,RecordStatus,FloodStatus,InvestigationStatus,EstimatedVulnerable,EstimatedUninhabitable,EstimatedTotalExternalOnly,EstimatedTotalInternallyFlooded,EstimatedDisruptedUtilities,EstimatedResidentialAffected,EstimatedResidentialFlooded,EstimatedResidentialOther,EstimatedCommercialAffected,EstimatedCommercialFlooded,EstimatedCommercialOther,EstimatedNoElectric,EstimatedNoGas,EstimatedNoFoul,EstimatedNoWaterSupply,StatusNote")
                .AppendJoin(',', string.Join(',',
                    CsvEscape(exportDto.Reference),
                    CsvEscape(exportDto.Title),
                    CsvEscape(exportDto.FloodingStart.ToString(CultureInfo.InvariantCulture)),
                    CsvEscape(exportDto.FloodingEnd.ToString(CultureInfo.InvariantCulture)),
                    CsvEscape(exportDto.PrimaryFloodCauses.ToString()),
                    CsvEscape(exportDto.SecondaryFloodCauses.ToString()),
                    CsvEscape(exportDto.ResponsibleOrganisations.ToString()),
                    CsvEscape(exportDto.UPRN?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    CsvEscape(exportDto.USRN?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    CsvEscape(exportDto.RecordStatus),
                    CsvEscape(exportDto.FloodStatus),
                    CsvEscape(exportDto.InvestigationStatus),
                    CsvEscape(exportDto.EstimatedVulnerable.ToString(CultureInfo.InvariantCulture)),
                    CsvEscape(exportDto.EstimatedUninhabitable.ToString(CultureInfo.InvariantCulture)),
                    CsvEscape(exportDto.EstimatedTotalExternalOnly.ToString(CultureInfo.InvariantCulture)),
                    CsvEscape(exportDto.EstimatedTotalInternallyFlooded.ToString(CultureInfo.InvariantCulture)),
                    CsvEscape(exportDto.EstimatedDisruptedUtilities.ToString(CultureInfo.InvariantCulture)),
                    CsvEscape(exportDto.EstimatedResidentialAffected?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    CsvEscape(exportDto.EstimatedResidentialFlooded?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    CsvEscape(exportDto.EstimatedResidentialOther?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    CsvEscape(exportDto.EstimatedCommercialAffected?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    CsvEscape(exportDto.EstimatedCommercialFlooded?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    CsvEscape(exportDto.EstimatedCommercialOther?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    CsvEscape(exportDto.EstimatedNoElectric?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    CsvEscape(exportDto.EstimatedNoGas?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    CsvEscape(exportDto.EstimatedNoFoul?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    CsvEscape(exportDto.EstimatedNoWaterSupply?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    CsvEscape(exportDto.StatusNote ?? string.Empty)
                    )).AppendLine();

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var stream = new MemoryStream(bytes);

            return Task.FromResult<MemoryStream?>(stream);
        }
        catch (Exception ex)
        {
            _exportErrorMessage = "An error occurred while preparing the export. Please try again.";
            logger.LogError(ex, _exportErrorMessage);
            return Task.FromResult<MemoryStream?>(null);
        }
    }

    private static string CsvEscape(string? value)
    {
        return string.IsNullOrEmpty(value)
            ? string.Empty
            : $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private IReadOnlyCollection<GdsBreadcrumb> CreateBreadcrumbs()
    {
        return
        [
            GeneralPages.Home.ToGdsBreadcrumb(),
            new ($"{EventPages.View.Url}/{Reference}", $"Flood event {Reference}")
        ];
    }
}