using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TNRelay.Config;
using TNRelay.Models;

namespace TNRelay
{
    /// <summary>
    /// Client for fetching alerts from a single TrueNAS instance via JSON-RPC over WebSocket
    /// </summary>
    /// <remarks>
    /// Create a new TrueNAS client for a given source
    /// </remarks>
    /// <param name="source">Source config (base URL, API key, name)</param>
    public class TrueNasClient(TrueNasSource source)
    {
        /// <summary>
        /// WebSocket URI, e.g. wss://robert.local/api/current
        /// </summary>
        private readonly Uri WebSocketUri = new($"wss://{source.BaseUrl}/api/current");

        /// <summary>
        /// API key for this source
        /// </summary>
        private readonly string ApiKey = source.ApiKey;

        /// <summary>
        /// Source name (e.g. "robert", "davos")
        /// </summary>
        public string SourceName { get; } = source.Name;

        /// <summary>
        /// Next request id
        /// </summary>
        private int NextId = 1;

        /// <summary>
        /// Fetch the current alert list
        /// </summary>
        /// <returns>List of alerts</returns>
        public async Task<List<TrueNasAlert>> GetAlertsAsync()
        {
            using ClientWebSocket socket = new();
            await socket.ConnectAsync(WebSocketUri, CancellationToken.None);

            bool loggedIn = await SendAsync<bool>(socket, "auth.login_with_api_key", [ApiKey]);
            if (!loggedIn)
                throw new Exception($"Login failed for source \"{SourceName}\"");

            List<TrueNasAlert> alerts = await SendAsync<List<TrueNasAlert>>(socket, "alert.list", []);

            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
            return alerts;
        }

        /// <summary>
        /// Send a single JSON-RPC request and return its result
        /// </summary>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <param name="socket">Open WebSocket connection</param>
        /// <param name="method">Method name</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Deserialized result</returns>
        private async Task<T> SendAsync<T>(ClientWebSocket socket, string method, object[] parameters)
        {
            JsonRpcRequest request = new()
            {
                Id = NextId++,
                Method = method,
                Params = parameters
            };

            byte[] requestBytes = JsonSerializer.SerializeToUtf8Bytes(request);
            await socket.SendAsync(requestBytes, WebSocketMessageType.Text, true, CancellationToken.None);

            using MemoryStream messageStream = new();
            byte[] buffer = new byte[8192];
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                messageStream.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            messageStream.Position = 0;
            JsonRpcResponse<T>? response = await JsonSerializer.DeserializeAsync<JsonRpcResponse<T>>(messageStream) ?? throw new Exception($"Empty response for method \"{method}\" on source \"{SourceName}\"");
            if (response.Error is not null)
                throw new Exception($"\"{method}\" failed on source \"{SourceName}\": {response.Error.Message}");

            return response.Result!;
        }
    }
}