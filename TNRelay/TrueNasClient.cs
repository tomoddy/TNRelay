using System.Net.Http.Headers;
using System.Net.Http.Json;
using TNRelay.Config;
using TNRelay.Models;

namespace TNRelay
{
    /// <summary>
    /// Client for fetching alerts from a single TrueNAS instance
    /// </summary>
    public class TrueNasClient
    {
        /// <summary>
        /// Underlying HTTP client
        /// </summary>
        private readonly HttpClient HttpClient;

        /// <summary>
        /// Source name (e.g. "robert", "davos")
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// Create a new TrueNAS client for a given source
        /// </summary>
        /// <param name="source">Source config (base URL, API key, name)</param>
        public TrueNasClient(TrueNasSource source)
        {
            SourceName = source.Name;
            HttpClient = new HttpClient
            {
                BaseAddress = new Uri(source.BaseUrl)
            };
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", source.ApiKey);
        }

        /// <summary>
        /// Fetch the current alert list
        /// </summary>
        /// <returns>List of alerts</returns>
        public async Task<List<TrueNasAlert>> GetAlertsAsync()
        {
            List<TrueNasAlert>? alerts = await HttpClient.GetFromJsonAsync<List<TrueNasAlert>>("/api/v2.0/alert/list");
            return alerts ?? [];
        }
    }
}