using Microsoft.JSInterop;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Converters;

/// <summary>
/// A class to manage GeoJSON file manipulation.
/// </summary>
public static class GeoJsonConverter
{
    private static readonly JsonSerializerOptions GeoJsonSerializerOptions = new()
    {
        Converters = { new GeoJsonConverterFactory() },
    };

    /// <summary>
    /// Create a GeoJSON file from a given object and geometry and return it as a DotNetStreamReference for download.
    /// </summary>
    /// <typeparam name="T">The (single) type of object to write to a GeoJSON file.</typeparam>
    /// <param name="geoJsonDataCreator">The GeoJsonDataCreator instance used to convert the object into GeoJSON attributes.</param>
    /// <param name="objectToWriteToGeoJson">The object to write to the GeoJSON file.</param>
    /// <param name="geometry">The geometry to include in the GeoJSON file. Should be in WGS84 (SRID 4326) per the GeoJSON spec.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A DotNetStreamReference representing the created GeoJSON file, or null if the object or geometry is missing/empty.</returns>
    public static async Task<DotNetStreamReference?> CreateGeoJsonAsync<T>(
        GeoJsonDataCreator<T> geoJsonDataCreator,
        T? objectToWriteToGeoJson,
        Geometry? geometry,
        CancellationToken cancellationToken)
        where T : class
    {
        var feature = geoJsonDataCreator.CreateFeature(objectToWriteToGeoJson, geometry);
        if (feature is null)
        {
            return null;
        }

        // Write GeoJSON data to Memory Stream
        MemoryStream ms = new();

        await JsonSerializer.SerializeAsync(ms, feature, GeoJsonSerializerOptions, cancellationToken);

        // Set pointer to start of memory stream for export
        ms.Position = 0;

        // return file created from memory stream
        return new DotNetStreamReference(stream: ms);
    }
}
