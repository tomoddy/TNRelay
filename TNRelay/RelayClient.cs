using System.Net.Http.Headers;
using TNRelay.Config;
using TNRelay.Models;

namespace TNRelay
{
    /// <summary>
    /// Sends new alerts to the external relay API via the Ting notification endpoint
    /// </summary>
    public class RelayClient
    {
        /// <summary>
        /// Underlying HTTP client
        /// </summary>
        private readonly HttpClient HttpClient;

        /// <summary>
        /// Create a new relay client
        /// </summary>
        /// <param name="config">Relay config</param>
        public RelayClient(RelayConfig config)
        {
            HttpClient = new HttpClient { BaseAddress = new Uri(config.Endpoint) };
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
        }

        /// <summary>
        /// Send a single alert as a Ting notification
        /// </summary>
        /// <param name="source">Source name (e.g. "robert")</param>
        /// <param name="alert">The alert to forward</param>
        public async Task SendAsync(string source, TrueNasAlert alert)
        {
            string title = Uri.EscapeDataString($"TrueNAS Alert [{source}] - {alert.Level}");
            string body = Uri.EscapeDataString(alert.Formatted ?? alert.Text ?? string.Empty);

            HttpResponseMessage response = await HttpClient.GetAsync($"/Ting?title={title}&body={body}");
            response.EnsureSuccessStatusCode();
        }
    }
}