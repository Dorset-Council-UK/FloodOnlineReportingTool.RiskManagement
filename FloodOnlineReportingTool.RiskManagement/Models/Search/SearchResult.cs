namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public record TEMP_FloodReport(
    Guid Id,
    DateTimeOffset CreatedUtc,
    DateTimeOffset? UpdatedUtc,
    Guid EligibilityCheckId,
    long? Uprn,
    long? Usrn,
    double Easting,
    double Northing,
    DateTimeOffset? ImpactStartUtc,
    int ImpactDurationHours,
    ICollection<TEMP_FloodReportSource> Sources
);

public record TEMP_FloodReportSource(
    Guid FloodReportId,
    Guid SourceId,
    DateTimeOffset CreatedUtc,
    DateTimeOffset? UpdatedUtc,
    string Name
);

public record SearchResult(bool Succeeded, IReadOnlyCollection<TEMP_FloodReport> FloodReports, ICollection<string> Errors)
{
    internal static SearchResult Success(IReadOnlyCollection<TEMP_FloodReport> floodReports) => new(Succeeded: true, floodReports, []);

    internal static SearchResult Failure(ICollection<string> errors) => new(Succeeded: false, [], errors);
}