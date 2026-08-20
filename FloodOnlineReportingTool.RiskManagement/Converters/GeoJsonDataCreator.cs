using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Converters;

/// <summary>
/// Objects inheriting from this class can be used to create GeoJSON features from a given object of type T.
/// </summary>
/// <typeparam name="T">The type of object to create a GeoJSON feature from.</typeparam>
public abstract record GeoJsonDataCreator<T>
    where T : class
{
    /// <summary>
    /// Creates a GeoJSON Feature combining the provided geometry with the attributes derived from the object.
    /// </summary>
    /// <param name="objectToWriteToGeoJson">The object to write to the GeoJSON feature.</param>
    /// <param name="geometry">The geometry to include in the feature. Should be in WGS84 (SRID 4326) per the GeoJSON spec.</param>
    /// <returns>A Feature, or null if the object or geometry is missing/empty.</returns>
    public Feature? CreateFeature(T? objectToWriteToGeoJson, Geometry? geometry)
    {
        if (objectToWriteToGeoJson is null || geometry is null || geometry.IsEmpty)
        {
            return null;
        }

        return new Feature(geometry, CreateAttributesTable(objectToWriteToGeoJson));
    }

    /// <summary>
    /// Builds the attributes table for the provided object, used as the GeoJSON feature's properties.
    /// </summary>
    /// <param name="objectToWriteToGeoJson">The object to derive attributes from.</param>
    protected abstract AttributesTable CreateAttributesTable(T objectToWriteToGeoJson);
}
