namespace FloodOnlineReportingTool.Database.Models.Enums;

// Only use this for flood extents now. It was used for everything but the 
// main polygons are always search areas. They don't represent the actual flood extent. 
public enum ConfidenceOptions
{
    Approximated,
    Confirmed,
    Unknown
}
