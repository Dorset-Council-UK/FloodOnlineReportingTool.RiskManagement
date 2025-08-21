using FloodOnlineReportingTool.RiskManagement.Data.Models;
using System.Net;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Services
{

    public class ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger) : IApiService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ILogger<ApiService> _logger = logger;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task<FloodReport?> GetReportByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("GetReportByIdAsync called with null or empty id");
                throw new ArgumentException("Report ID cannot be null or empty", nameof(id));
            }

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var requestPath = $"reports/{Uri.EscapeDataString(id)}";

            try
            {
                _logger.LogInformation("Making API request to get report with ID: {ReportId}", id);

                using var response = await httpClient.GetAsync(requestPath, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Report not found with ID: {ReportId}", id);
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("API request failed with status {StatusCode}: {ErrorContent}",
                        response.StatusCode, errorContent);

                    throw new HttpRequestException(
                        $"API request failed with status {response.StatusCode}: {response.ReasonPhrase}",
                        null,
                        response.StatusCode);
                }

                var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    _logger.LogWarning("API returned empty response for report ID: {ReportId}", id);
                    return null;
                }

                var report = JsonSerializer.Deserialize<FloodReport>(jsonContent, _jsonOptions);

                _logger.LogInformation("Successfully retrieved report with ID: {ReportId}", id);
                return report;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Request was cancelled for report ID: {ReportId}", id);
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while getting report with ID: {ReportId}", id);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error for report ID: {ReportId}", id);
                throw new InvalidOperationException("Failed to deserialize API response", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while getting report with ID: {ReportId}", id);
                throw;
            }
        }
    }
}
