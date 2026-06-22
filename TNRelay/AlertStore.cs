using Npgsql;
using TNRelay.Config;

namespace TNRelay
{
    /// <summary>
    /// Tracks which alerts have already been forwarded, using Postgres for persistence
    /// </summary>
    public class AlertStore
    {
        /// <summary>
        /// Connection string
        /// </summary>
        private readonly string ConnectionString;

        /// <summary>
        /// Create a new alert store
        /// </summary>
        /// <param name="config">Postgres config</param>
        public AlertStore(PostgresConfig config) => ConnectionString = config.ConnectionString;

        /// <summary>
        /// Check whether an alert has already been forwarded
        /// </summary>
        /// <param name="source">Source name</param>
        /// <param name="alertId">Alert UUID</param>
        /// <returns>True if already forwarded</returns>
        public async Task<bool> HasBeenForwardedAsync(string source, string alertId)
        {
            await using NpgsqlConnection connection = new(ConnectionString);
            await connection.OpenAsync();

            await using NpgsqlCommand command = new("SELECT 1 FROM \"TNRelay\" WHERE source = @source AND alert_id = @alertId", connection);
            command.Parameters.AddWithValue("source", source);
            command.Parameters.AddWithValue("alertId", alertId);

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync();
        }

        /// <summary>
        /// Mark an alert as forwarded
        /// </summary>
        /// <param name="source">Source name</param>
        /// <param name="alertId">Alert UUID</param>
        public async Task MarkForwardedAsync(string source, string alertId)
        {
            await using NpgsqlConnection connection = new(ConnectionString);
            await connection.OpenAsync();

            await using NpgsqlCommand command = new("INSERT INTO \"TNRelay\" (source, alert_id) VALUES (@source, @alertId) ON CONFLICT DO NOTHING", connection);
            command.Parameters.AddWithValue("source", source);
            command.Parameters.AddWithValue("alertId", alertId);
            await command.ExecuteNonQueryAsync();
        }
    }
}