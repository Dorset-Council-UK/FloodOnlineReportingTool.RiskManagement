using FloodOnlineReportingTool.RiskManagement.Extensions;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class Index(
    ILogger<Index> logger,
    IJSRuntime js,
    ISettingsService settingsService,
    NavigationManager navigationManager
)
{
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];
    [Parameter]
    public string? Reference { get; set; }
    private string _selfUrl => navigationManager.Uri;

    private bool _redirectingToSearchOptions;

    protected override void OnInitialized()
    {
        Breadcrumbs = CreateBreadcrumbs();

        if (string.IsNullOrWhiteSpace(Reference))
        {
            logger.LogInformation("No search reference provided in query parameters. Unable to load search results.");
            _redirectingToSearchOptions = true;
            navigationManager.NavigateTo(SearchPages.Options.Url);
        }
    }

    private EventOverviewViewModel? ViewModel { get; set; } = default!;

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

    private string JsonDownloadFileName => $"flood-event-{Reference}.geojson";
    private string CsvDownloadFileName => $"flood-event-{Reference}.csv";

    private string ExportToJson()
    {
        if (ViewModel is not EventOverviewViewModel model)
        {
            return string.Empty;
        }
        var exportDto = model.ToExportDto();
        if (exportDto is null || exportDto.Geom is null || exportDto.Geom.IsEmpty)
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
                exportDto.Title
            }
        };

        var json = JsonSerializer.Serialize(feature, new JsonSerializerOptions
        {
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
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
        var csv = new StringBuilder()
            .AppendLine("Reference,Title")
            .AppendLine(string.Join(',',
                CsvEscape(exportDto.Reference),
                CsvEscape(exportDto.Title)));

        return $"data:text/csv;charset=utf-8,{Uri.EscapeDataString(csv.ToString())}";
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