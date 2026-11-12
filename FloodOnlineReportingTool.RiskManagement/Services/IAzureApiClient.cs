using FloodOnlineReportingTool.RiskManagement.Models.Azure;

namespace FloodOnlineReportingTool.RiskManagement.Services
{
    public interface IAzureApiClient
    {
        public Task<IReadOnlyCollection<(int Index, int? ParentIndex, (string Classification, int Premises) Data)>?> GetAddressSummary(int epsg, string area, CancellationToken ct);

        public Task<IReadOnlyCollection<ApiAddress>?> AddressSearch(string postcode, SearchAreaOptions searchArea, Uri? referer, CancellationToken ct);
    }
}
