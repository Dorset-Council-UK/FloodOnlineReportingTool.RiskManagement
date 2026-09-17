using FloodOnlineReportingTool.RiskManagement.Models.Azure;
using FloodOnlineReportingTool.RiskManagement.Options;
using Microsoft.Identity.Abstractions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using System.Text;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public class AzureApiClient(
    IDownstreamApi downstreamApi,
    ILogger<AzureApiClient> logger) : IAzureApiClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new GeoJsonConverterFactory() },
    };

    public async Task<IReadOnlyCollection<(int Index, int? ParentIndex, (string Classification, int Premises) Data)>?> GetAddressSummary(int epsg, string searchArea, string area, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(area))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(area));
        }

        try
        {
            logger.LogInformation("Checking downstream API for flood summary");

            var relativePath = $"/v1/places/address/summary/geojson?epsg={epsg}&searchArea={searchArea}";
            var geometry = JsonSerializer.Deserialize<Geometry>(area, _jsonSerializerOptions)
                ?? throw new ArgumentException("Value must be a valid GeoJSON geometry.", nameof(area));

            geometry.SRID = epsg;

            var featureCollection = new FeatureCollection
            {
                new Feature(geometry, new AttributesTable())
            };
            var postBodyJson = JsonSerializer.Serialize(featureCollection, _jsonSerializerOptions);

            using var response = await downstreamApi.CallApiAsync(
                DownstreamAPIOptions.AzureApiGeoApi,
                options =>
                {
                    options.HttpMethod = HttpMethod.Post.Method;
                    options.RelativePath = relativePath;
                    var existingCustomizeHttpRequestMessage = options.CustomizeHttpRequestMessage;
                    options.CustomizeHttpRequestMessage = message =>
                    {
                        existingCustomizeHttpRequestMessage?.Invoke(message);
                        message.Content = new StringContent(postBodyJson, Encoding.UTF8, "application/json");
                    };
                },
                cancellationToken: ct);

            if (response is null)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Downstream API returned status code {(int)response.StatusCode}.", null, response.StatusCode);
            }

            if (response.Content is null)
            {
                return null;
            }

            var responseBodyJson = await response.Content.ReadAsStringAsync(ct);
            logger.LogInformation(
                "Received address summary response with status code {StatusCode} and {ResponseLength} characters",
                (int)response.StatusCode,
                responseBodyJson.Length);

            var addressSummaryResponse = JsonSerializer.Deserialize<AddressSummaryResponseDto>(responseBodyJson, _jsonSerializerOptions);
            if (addressSummaryResponse is null)
            {
                return null;
            }

            return FlattenCategories(addressSummaryResponse);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.Unauthorized)
        {
            logger.LogWarning("Downstream API returned {StatusCode} for address summary request", ex.StatusCode);
            return null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.BadRequest
                                                        or System.Net.HttpStatusCode.NotFound)
        {
            logger.LogDebug("Downstream API returned {StatusCode} for reference or no matching report exists", ex.StatusCode);
            return null;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while getting address summary from downstream API");
            return null;
        }
    }

    private static IReadOnlyCollection<(int Index, int? ParentIndex, (string Classification, int Premises) Data)> FlattenCategories(AddressSummaryResponseDto response)
    {
        var flattened = new List<(int Index, int? ParentIndex, (string Classification, int Premises) Data)>();
        var currentIndex = 1;

        if (response.PrimaryClassifications is null)
        {
            return flattened;
        }

        foreach (var primaryClassification in response.PrimaryClassifications)
        {
            AddCategory(primaryClassification.Key, primaryClassification.Value, null);
        }

        return flattened;

        void AddCategory(string categoryName, AddressSummaryCategoryDto? category, int? parentIndex)
        {
            if (category is null)
            {
                return;
            }

            var index = currentIndex++;
            flattened.Add((index, parentIndex, (categoryName, category.Premises)));

            if (category.Subcategories is not null)
            {
                foreach (var subcategory in category.Subcategories)
                {
                    AddCategory(subcategory.Key, subcategory.Value, index);
                }
            }

            if (category.ChildCategoryCounts is not null)
            {
                foreach (var childCategory in category.ChildCategoryCounts)
                {
                    var childIndex = currentIndex++;
                    flattened.Add((childIndex, index, (childCategory.Key, childCategory.Value)));
                }
            }
        }
    }

    public async Task<IReadOnlyCollection<ApiAddress>?> AddressSearch(string postcode, SearchAreaOptions searchArea, Uri? referer, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(postcode))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(postcode));
        }

        try
        {
            var relativePath = $"/v1/Places/Address?query={Uri.EscapeDataString(postcode)}&searchArea={searchArea}";

            using var response = await downstreamApi.CallApiAsync(
                    DownstreamAPIOptions.AzureApiGeoApi,
                    options =>
                    {
                        options.HttpMethod = HttpMethod.Get.Method;
                        options.RelativePath = relativePath;
                    },
                    cancellationToken: ct);

            if (response is null)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Downstream API returned status code {(int)response.StatusCode}.", null, response.StatusCode);
            }

            if (response.Content is null)
            {
                return null;
            }

            var responseBodyJson = await response.Content.ReadAsStringAsync(ct);
            logger.LogInformation(
                "Received address search response with status code {StatusCode} and {ResponseLength} characters",
                (int)response.StatusCode,
                responseBodyJson.Length);

            var addressResults = JsonSerializer.Deserialize<IReadOnlyCollection<ApiAddress>?>(responseBodyJson, _jsonSerializerOptions);
            if (addressResults is null)
            {
                return null;
            }


            return addressResults;
        }
        catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.Unauthorized)
        {
            logger.LogWarning("Downstream API returned {StatusCode} for address search request", ex.StatusCode);
            return null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.BadRequest
                                                        or System.Net.HttpStatusCode.NotFound)
        {
            logger.LogDebug("Downstream API returned {StatusCode} for address search request", ex.StatusCode);
            return null;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while getting address search results from downstream API");
            return null;
        }
    }
}
