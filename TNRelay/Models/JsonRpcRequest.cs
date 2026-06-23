using System.Text.Json.Serialization;

namespace TNRelay.Models
{
    /// <summary>
    /// A JSON-RPC 2.0 request
    /// </summary>
    public class JsonRpcRequest
    {
        /// <summary>
        /// Protocol version, always "2.0"
        /// </summary>
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        /// <summary>
        /// Request id, used to match the response
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Method name to invoke
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Method parameters
        /// </summary>
        [JsonPropertyName("params")]
        public object[] Params { get; set; } = [];
    }
}