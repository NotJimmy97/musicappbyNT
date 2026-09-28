using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using MusicApp.Core.Interfaces.Persistence;
using Newtonsoft.Json;

namespace MusicApp.Core.Persistence.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly string _connectionString;

        public SettingsRepository(string connectionString = null)
        {
            _connectionString = connectionString ?? DatabaseInitializer.ConnectionString;
        }

        public async Task<T> GetAsync<T>(string key, T defaultValue = default(T))
        {
            if (string.IsNullOrWhiteSpace(key)) return defaultValue;

            return await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT value FROM app_settings WHERE key = @key", conn))
                    {
                        cmd.Parameters.AddWithValue("@key", key);
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            return defaultValue;
                        }

                        string stringVal = result.ToString();
                        if (typeof(T) == typeof(string))
                        {
                            return (T)(object)stringVal;
                        }

                        try
                        {
                            return JsonConvert.DeserializeObject<T>(stringVal);
                        }
                        catch
                        {
                            return defaultValue;
                        }
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task SetAsync<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            string jsonValue = typeof(T) == typeof(string) ? value?.ToString() : JsonConvert.SerializeObject(value);

            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("INSERT OR REPLACE INTO app_settings (key, value) VALUES (@key, @value)", conn))
                    {
                        cmd.Parameters.AddWithValue("@key", key);
                        cmd.Parameters.AddWithValue("@value", jsonValue ?? string.Empty);
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }
    }
}
