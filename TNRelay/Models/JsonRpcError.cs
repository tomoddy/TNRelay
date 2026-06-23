using System.Text.Json.Serialization;

namespace TNRelay.Models
{
    /// <summary>
    /// A JSON-RPC 2.0 error payload
    /// </summary>
    public class JsonRpcError
    {
        /// <summary>
        /// Error code
        /// </summary>
        [JsonPropertyName("code")]
        public int Code { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}