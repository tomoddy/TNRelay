using System.Text.Json.Serialization;

namespace TNRelay.Models
{
    /// <summary>
    /// A JSON-RPC 2.0 response
    /// </summary>
    /// <typeparam name="T">Type of the result payload</typeparam>
    public class JsonRpcResponse<T>
    {
        /// <summary>
        /// Request id this response matches
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Result payload, if successful
        /// </summary>
        [JsonPropertyName("result")]
        public T? Result { get; set; }

        /// <summary>
        /// Error payload, if the call failed
        /// </summary>
        [JsonPropertyName("error")]
        public JsonRpcError? Error { get; set; }
    }
}