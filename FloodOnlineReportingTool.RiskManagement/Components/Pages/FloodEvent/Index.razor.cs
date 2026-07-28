using FloodOnlineReportingTool.RiskManagement.Models.Order;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class Index(
    ILogger<Index> logger,
    IJSRuntime js,
    NavigationManager navigationManager
)
{
    [Parameter]
    public string? SearchReference { get; set; }
    private string _selfUrl => navigationManager.Uri;

    private bool _redirectingToSearchOptions;

    protected override void OnInitialized()
    {
        if (string.IsNullOrWhiteSpace(SearchReference))
        {
            logger.LogInformation("No search reference provided in query parameters. Unable to load search results.");
            _redirectingToSearchOptions = true;
            navigationManager.NavigateTo(SearchPages.Options.Url);
        }
    }

    // TODO - uncomment below code and tweak once this page is more complete (this triggers a recent work item to be added to local storage,
    // but the model is not yet populated)

    //protected override async Task OnAfterRenderAsync(bool firstRender)
    //{
    //    if (Model is not null)
    //    {
    //        //add to recent work store
    //        await settingsService.AddRecentWork($"{SearchReference}", "Flood Event", Model.SearchTitle, $"event/{SearchReference}");
    //    }
    //    await base.OnAfterRenderAsync(firstRender);
    //}

    private async Task PrintPageAsync()
    {
        await js.InvokeVoidAsync("print");
    }

    private string JsonDownloadFileName => $"flood-event-{SearchReference}.geojson";
    private string CsvDownloadFileName => $"flood-event-{SearchReference}.csv";

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
}