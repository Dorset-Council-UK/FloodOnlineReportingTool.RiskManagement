using FloodOnlineReportingTool.Database.Models.Lookups;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface ICommonRepository
{
    Task<IList<FloodProblem>> GetFloodProblems(string category, CancellationToken ct);
    Task<DownstreamApiLayer?> GetDownstreamApiLayers(string apiSourceRecord, CancellationToken ct);
    Task<IList<ReportStatus>> GetReportStatuses(CancellationToken ct);
    Task<IList<FloodStatus>> GetFloodStatuses(CancellationToken ct);
    Task<IList<InvestigationStatus>> GetInvestigationStatuses(CancellationToken ct);
}
