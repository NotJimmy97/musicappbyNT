using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using MusicApp.Core.Interfaces.Persistence;
using MusicApp.Core.Models;

namespace MusicApp.Core.Persistence.Repositories
{
    public class PresetRepository : IPresetRepository
    {
        private readonly string _connectionString;

        public PresetRepository(string connectionString = null)
        {
            _connectionString = connectionString ?? DatabaseInitializer.ConnectionString;
        }

        public async Task<IEnumerable<EqPresetEntity>> GetAllAsync()
        {
            return await Task.Run(() =>
            {
                var list = new List<EqPresetEntity>();
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT * FROM eq_presets ORDER BY is_custom ASC, name ASC", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new EqPresetEntity
                            {
                                Name = reader["name"].ToString(),
                                GainsJson = reader["gains_json"].ToString(),
                                IsCustom = Convert.ToInt32(reader["is_custom"]) == 1
                            });
                        }
                    }
                }
                return list;
            }).ConfigureAwait(false);
        }

        public async Task SaveCustomPresetAsync(string name, float[] gains)
        {
            if (string.IsNullOrWhiteSpace(name) || gains == null) return;

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(gains);

            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        INSERT INTO eq_presets (name, gains_json, is_custom)
                        VALUES (@name, @gains, 1)
                        ON CONFLICT(name) DO UPDATE SET gains_json = excluded.gains_json, is_custom = 1";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name.Trim());
                        cmd.Parameters.AddWithValue("@gains", json);
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task DeleteCustomPresetAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("DELETE FROM eq_presets WHERE name = @name AND is_custom = 1", conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }
    }
}
