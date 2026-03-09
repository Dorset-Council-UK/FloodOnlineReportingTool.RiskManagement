using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FloodOnlineReportingTool.RiskManagement.Converters;

public class MultiPolygonJsonConverter : JsonConverter<MultiPolygon>
{
    public override MultiPolygon? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var wkt = reader.GetString();
        if (string.IsNullOrWhiteSpace(wkt))
            return null;

        var geometry = new WKTReader().Read(wkt);
        return geometry as MultiPolygon;
    }

    public override void Write(Utf8JsonWriter writer, MultiPolygon value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(new WKTWriter().Write(value));
    }
}
