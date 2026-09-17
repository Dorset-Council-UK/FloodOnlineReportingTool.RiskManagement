namespace FloodOnlineReportingTool.RiskManagement.Services
{
    public interface IAzureApiClient
    {
        public Task<IReadOnlyCollection<(int Index, int? ParentIndex, (string Classification, int Premises) Data)>?> GetAddressSummary(int epsg, string searchArea, string area, CancellationToken ct);
    }
}
