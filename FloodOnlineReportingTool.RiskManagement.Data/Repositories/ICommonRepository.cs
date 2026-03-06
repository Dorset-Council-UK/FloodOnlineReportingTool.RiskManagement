using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public interface ICommonRepository
{
    Task<IList<FloodProblem>> GetFloodProblems(string category, CancellationToken ct);

    Task<DownstreamApiLayer?> GetDownstreamApiLayers(string apiSourceRecord, CancellationToken ct);
}
