using CsvHelper;
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
using System.Formats.Asn1;
using System.Globalization;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    private string JsonDownloadFileName => $"flood-event-{Reference}.geojson";
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

    private string ExportToJson()
    {
        if (ViewModel is not EventOverviewViewModel model)
        {
            return string.Empty;
        }
        var exportDto = model.ToExportDto();
        if (exportDto is null || exportDto.Geom.IsEmpty)
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
                        .ToArray(),
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
                exportDto.StatusNote,
            },
        };

        var json = JsonSerializer.Serialize(feature, new JsonSerializerOptions
        {
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
        });

        return $"data:application/geo+json;charset=utf-8,{Uri.EscapeDataString(json)}";
    }

    private string ExportToCsv()
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
        //var csv = new StringBuilder()
        //    .AppendLine("Reference,Title,FloodingStart,FloodingEnd,PrimaryFloodCauses,SecondaryFloodCauses,ResponsibleOrganisations,UPRN,USRN,RecordStatus,FloodStatus,InvestigationStatus,EstimatedVulnerable,EstimatedUninhabitable,EstimatedTotalExternalOnly,EstimatedTotalInternallyFlooded,EstimatedDisruptedUtilities,EstimatedResidentialAffected,EstimatedResidentialFlooded,EstimatedResidentialOther,EstimatedCommercialAffected,EstimatedCommercialFlooded,EstimatedCommercialOther,EstimatedNoElectric,EstimatedNoGas,EstimatedNoFoul,EstimatedNoWaterSupply,StatusNote")
        //        .Append(string.Join(',',
        //            CsvEscapeString(exportDto.Reference),
        //            CsvEscapeString(exportDto.Title),
        //            CsvEscapeDateTime(exportDto.FloodingStart),
        //            CsvEscapeDateTime(exportDto.FloodingEnd),
        //            CsvEscapeStrings(exportDto.PrimaryFloodCauses),
        //            CsvEscapeStrings(exportDto.SecondaryFloodCauses),
        //            CsvEscapeStrings(exportDto.ResponsibleOrganisations),
        //            CsvEscapeLong(exportDto.UPRN),
        //            CsvEscapeLong(exportDto.USRN),
        //            CsvEscapeString(exportDto.RecordStatus),
        //            CsvEscapeString(exportDto.FloodStatus),
        //            CsvEscapeString(exportDto.InvestigationStatus),
        //            CsvEscapeInt(exportDto.EstimatedVulnerable),
        //            CsvEscapeInt(exportDto.EstimatedUninhabitable),
        //            CsvEscapeInt(exportDto.EstimatedTotalExternalOnly),
        //            CsvEscapeInt(exportDto.EstimatedTotalInternallyFlooded),
        //            CsvEscapeInt(exportDto.EstimatedDisruptedUtilities),
        //            CsvEscapeInt(exportDto.EstimatedResidentialAffected),
        //            CsvEscapeInt(exportDto.EstimatedResidentialFlooded),
        //            CsvEscapeInt(exportDto.EstimatedResidentialOther),
        //            CsvEscapeInt(exportDto.EstimatedCommercialAffected),
        //            CsvEscapeInt(exportDto.EstimatedCommercialFlooded),
        //            CsvEscapeInt(exportDto.EstimatedCommercialOther),
        //            CsvEscapeInt(exportDto.EstimatedNoElectric),
        //            CsvEscapeInt(exportDto.EstimatedNoGas),
        //            CsvEscapeInt(exportDto.EstimatedNoFoul),
        //            CsvEscapeInt(exportDto.EstimatedNoWaterSupply),
        //            CsvEscapeString(exportDto.StatusNote)
        //            )).AppendLine();

        // Write csv data to Memory Stream
        MemoryStream ms = new();

        using var writer = new StreamWriter(ms, leaveOpen: true);        
        using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csvWriter.WriteField("Reference");
        csvWriter.WriteField("Title");
        csvWriter.WriteField("FloodingStart");
        csvWriter.WriteField("FloodingEnd");
        csvWriter.WriteField("PrimaryFloodCauses");
        csvWriter.WriteField("SecondaryFloodCauses");
        csvWriter.WriteField("ResponsibleOrganisations");
        csvWriter.WriteField("UPRN");
        csvWriter.WriteField("USRN");
        csvWriter.WriteField("RecordStatus");
        csvWriter.WriteField("FloodStatus");
        csvWriter.WriteField("InvestigationStatus");
        csvWriter.WriteField("EstimatedVulnerable");
        csvWriter.WriteField("EstimatedUninhabitable");
        csvWriter.WriteField("EstimatedTotalExternalOnly");
        csvWriter.WriteField("EstimatedTotalInternallyFlooded");
        csvWriter.WriteField("EstimatedDisruptedUtilities");
        csvWriter.WriteField("EstimatedResidentialAffected");
        csvWriter.WriteField("EstimatedResidentialFlooded");
        csvWriter.WriteField("EstimatedResidentialOther");
        csvWriter.WriteField("EstimatedCommercialAffected");
        csvWriter.WriteField("EstimatedCommercialFlooded");
        csvWriter.WriteField("EstimatedCommercialOther");
        csvWriter.WriteField("EstimatedNoElectric");
        csvWriter.WriteField("EstimatedNoGas");
        csvWriter.WriteField("EstimatedNoFoul");
        csvWriter.WriteField("EstimatedNoWaterSupply");
        csvWriter.WriteField("StatusNote");
        csvWriter.NextRecord();

        csvWriter.WriteField(exportDto.Reference);
        csvWriter.WriteField(exportDto.Title);
        csvWriter.WriteField(exportDto.FloodingStart);
        csvWriter.WriteField(exportDto.FloodingEnd);
        csvWriter.WriteField(exportDto.PrimaryFloodCauses);
        csvWriter.WriteField(exportDto.SecondaryFloodCauses);
        csvWriter.WriteField(exportDto.ResponsibleOrganisations);
        csvWriter.WriteField(exportDto.UPRN);
        csvWriter.WriteField(exportDto.USRN);
        csvWriter.WriteField(exportDto.RecordStatus);
        csvWriter.WriteField(exportDto.FloodStatus);
        csvWriter.WriteField(exportDto.InvestigationStatus);
        csvWriter.WriteField(exportDto.EstimatedVulnerable);
        csvWriter.WriteField(exportDto.EstimatedUninhabitable);
        csvWriter.WriteField(exportDto.EstimatedTotalExternalOnly);
        csvWriter.WriteField(exportDto.EstimatedTotalInternallyFlooded);
        csvWriter.WriteField(exportDto.EstimatedDisruptedUtilities);
        csvWriter.WriteField(exportDto.EstimatedResidentialAffected);
        csvWriter.WriteField(exportDto.EstimatedResidentialFlooded);
        csvWriter.WriteField(exportDto.EstimatedResidentialOther);
        csvWriter.WriteField(exportDto.EstimatedCommercialAffected);
        csvWriter.WriteField(exportDto.EstimatedCommercialFlooded);
        csvWriter.WriteField(exportDto.EstimatedCommercialOther);
        csvWriter.WriteField(exportDto.EstimatedNoElectric);
        csvWriter.WriteField(exportDto.EstimatedNoGas);
        csvWriter.WriteField(exportDto.EstimatedNoFoul);
        csvWriter.WriteField(exportDto.EstimatedNoWaterSupply);
        csvWriter.WriteField(exportDto.StatusNote);
        csvWriter.NextRecord();

        writer.Flush();
        
        // Set pointer to start of memory stream for export
        ms.Position = 0;

        return string.Empty;
        //// return file created from memory stream
        ////return File(ms, "text/csv", "registerofinterests.csv");

        //await JS.InvokeVoidAsync("downloadFile", "users.csv", csvContent);

        //return $"data:text/csv;charset=utf-8,{Uri.EscapeDataString(csv.ToString())}";


        //string CsvEscapeInt(int? value)
        //    => CsvEscapeString(value?.ToString(CultureInfo.InvariantCulture) ?? null);

        //string CsvEscapeLong(long? value)
        //    => CsvEscapeString(value?.ToString(CultureInfo.InvariantCulture) ?? null);

        //string CsvEscapeDateTime(DateTimeOffset? value)
        //    => CsvEscapeString(value?.ToString(CultureInfo.InvariantCulture) ?? null);

        //string CsvEscapeStrings(IReadOnlyCollection<string> values)
        //    => CsvEscapeString(string.Join("; ", values));

        //string CsvEscapeString(string? value)
        //    => string.IsNullOrEmpty(value)
        //        ? string.Empty
        //        : $"\"{value.Replace("\"", "\"\"")}\"";
                 
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