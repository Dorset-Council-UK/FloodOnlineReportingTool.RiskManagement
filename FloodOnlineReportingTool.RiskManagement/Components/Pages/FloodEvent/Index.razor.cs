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

    // TODO - the model is not yet populated

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        //if (Model is not null)
        //{
            //add to recent work store
            await settingsService.AddRecentWork($"{Reference}", "Flood Event", "", $"event/{Reference}");
        //}
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
        //if (ViewModel is not SearchResultFloodReport model)
        //{
        //    return string.Empty;
        //}
        //var exportDto = model.ToExportDto(ReportStatus);
        //if (exportDto is null)
        //{
            // Invalid record
            return string.Empty;
        //}
        //var feature = new
        //{
        //    type = "Feature",
        //    geometry = new
        //    {
        //        type = "Point",
        //        coordinates = new[] { exportDto.X, exportDto.Y }
        //    },
        //    properties = new
        //    {
        //        exportDto.Reference,
        //        exportDto.ReportedOn,
        //        exportDto.RecordStatus,
        //        exportDto.Easting,
        //        exportDto.Northing
        //    }
        //};

        //var json = JsonSerializer.Serialize(feature, new JsonSerializerOptions
        //{
        //    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
        //});

        //return $"data:application/geo+json;charset=utf-8,{Uri.EscapeDataString(json)}";
    }

    private string ExportToCsv()
    {
        //if (ViewModel is not SearchResultFloodReport model)
        //{
        //    return string.Empty;
        //}
        //var exportDto = model.ToExportDto(ReportStatus);
        //if (exportDto is null)
        //{
            // Invalid record
            return string.Empty;
        //}
        //var csv = new StringBuilder()
        //    .AppendLine("Reference,ReportedOn,RecordStatus,Easting,Northing")
        //    .AppendLine(string.Join(',',
        //        CsvEscape(exportDto.Reference),
        //        CsvEscape(exportDto.ReportedOn),
        //        CsvEscape(exportDto.RecordStatus),
        //        exportDto.Easting.ToString(System.Globalization.CultureInfo.InvariantCulture),
        //        exportDto.Northing.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        //return $"data:text/csv;charset=utf-8,{Uri.EscapeDataString(csv.ToString())}";
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