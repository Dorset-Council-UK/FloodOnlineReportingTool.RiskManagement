using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FloodOnlineReportingTool.RiskManagement.Converters;

public class MultiPolygonJsonConverter() : JsonConverter<MultiPolygon>
{
    public override MultiPolygon? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var wkt = reader.GetString();
        if (string.IsNullOrWhiteSpace(wkt))
            return null;
        try
        {
            var geometry = new WKTReader().Read(wkt);
            return geometry as MultiPolygon;
        }
        catch (Exception)
        {
            //ignore any parsing errors and return null
            //this is unlikely to occur so not being logged,
            //but if it does, it will be handled gracefully by
            //returning null and not breaking the application
            return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, MultiPolygon value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(new WKTWriter().Write(value));
    }
}
