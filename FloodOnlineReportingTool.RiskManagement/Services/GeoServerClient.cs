using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using GdsBlazorComponents;
using Microsoft.Azure.Amqp.Framing;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public class GeoServerClient(HttpClient httpClient, ILogger<GeoServerClient> logger) : IGeoServerClient
{
    private const string WfsPath = "/geoserver/boundaryline/ows";
    
    public async Task<IReadOnlyCollection<GdsOptionItem<string>>> SearchParishNameAsync(string searchTerm, CancellationToken ct = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // Build CQL filter for case-insensitive LIKE query that matches anywhere in the string
            var cqlFilter = string.IsNullOrWhiteSpace(searchTerm) 
                ? "name IS NOT NULL" 
                : $"strToLowerCase(name) LIKE '%{EscapeCql(searchTerm.ToLowerInvariant())}%'" ;
            
            var queryParams = new Dictionary<string, string>
            {
                ["service"] = "WFS",
                ["version"] = "1.0.0",
                ["request"] = "GetFeature",
                ["typeName"] = "boundaryline:parish",
                ["outputFormat"] = "application/json",
                ["maxFeatures"] = "100",
                ["CQL_FILTER"] = cqlFilter,
                ["propertyName"] = "name" // Only return the name property for efficiency
            };
            
            var queryString = string.Join("&", queryParams.Select(kvp => 
                $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            
            var requestUri = $"{WfsPath}?{queryString}";
            
            // HttpClient will automatically include the Bearer token via GeoServerTokenHandler
            var response = await httpClient.GetFromJsonAsync<GeoJsonFeatureCollection>(requestUri, ct);
            
            if (response?.Features == null)
            {
                return new List<GdsOptionItem<string>>();
            }
            
            // Extract names, remove duplicates, and sort
            var results = response.Features
                .Select(f => f.Properties?.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList()!;
            
            logger.LogInformation("GeoServer query completed in {ElapsedMs}ms, returned {Count} results", 
                stopwatch.ElapsedMilliseconds, results.Count);

            // Create option items with "Select an option" as the first option
            var optionItems = new List<GdsOptionItem<string>>
            {
                new GdsOptionItem<string>("0".AsSpan(), "Select an option", string.Empty, false)
            };
            optionItems.AddRange(results.Select((o, idx) => new GdsOptionItem<string>((idx + 1).ToString().AsSpan(), o, o, false)));

            return optionItems;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            logger.LogWarning("GeoServer request timed out after {ElapsedMs}ms for search term: {SearchTerm}", 
                stopwatch.ElapsedMilliseconds, searchTerm);
            return new List<GdsOptionItem<string>>();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to query GeoServer for parish names with search term: {SearchTerm}", searchTerm);
            return new List<GdsOptionItem<string>>();
        }
    }
    
    private static string EscapeCql(string input)
    {
        // Escape single quotes for CQL and remove any special regex characters
        return input
            .Replace("'", "''")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }
    
    // GeoJSON response models
    private record GeoJsonFeatureCollection(
        [property: JsonPropertyName("features")] List<GeoJsonFeature>? Features
    );
    
    private record GeoJsonFeature(
        [property: JsonPropertyName("properties")] ParishProperties? Properties
    );
    
    private record ParishProperties(
        [property: JsonPropertyName("name")] string? Name
    );
}