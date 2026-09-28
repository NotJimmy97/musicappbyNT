using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using MusicApp.Core.Interfaces.Persistence;
using MusicApp.Core.Models;

namespace MusicApp.Core.Persistence.Repositories
{
    /// <summary>
    /// Repository quản lý nhật ký tương tác người dùng.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Ghi và đọc log hành vi người dùng (Play, Skip...).
    /// KHÔNG chịu trách nhiệm: Xử lý logic tính toán Recommendation.
    /// Vòng đời: Transient/Scoped.
    /// Luồng/DB: Thao tác DB qua Task bất đồng bộ.
    /// </remarks>
    public class InteractionRepository : IInteractionRepository
    {
        private readonly string _connectionString;

        public InteractionRepository(string connectionString = null)
        {
            _connectionString = connectionString ?? DatabaseInitializer.ConnectionString;
        }

        public async Task LogInteractionAsync(int trackId, string actionType, int durationPlayedSeconds)
        {
            if (string.IsNullOrWhiteSpace(actionType)) return;

            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        INSERT INTO user_interactions (track_id, action_type, duration_played, created_at)
                        VALUES (@tId, @action, @duration, @now)";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@tId", trackId);
                        cmd.Parameters.AddWithValue("@action", actionType);
                        cmd.Parameters.AddWithValue("@duration", durationPlayedSeconds);
                        cmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<UserInteractionEntity>> GetRecentInteractionsAsync(int limit = 100)
        {
            return await Task.Run(() =>
            {
                var list = new List<UserInteractionEntity>();
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT * FROM user_interactions ORDER BY created_at DESC LIMIT @limit", conn))
                    {
                        cmd.Parameters.AddWithValue("@limit", Math.Max(1, limit));
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new UserInteractionEntity
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    TrackId = Convert.ToInt32(reader["track_id"]),
                                    ActionType = reader["action_type"].ToString(),
                                    DurationPlayedSeconds = Convert.ToInt32(reader["duration_played"]),
                                    CreatedAt = reader["created_at"].ToString()
                                });
                            }
                        }
                    }
                }
                return list;
            }).ConfigureAwait(false);
        }
    }
}
