using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using MusicApp.Core.Interfaces.Persistence;
using MusicApp.Core.Models;

namespace MusicApp.Core.Persistence.Repositories
{
    /// <summary>
    /// Repository quản lý hàng đợi phát nhạc cục bộ.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Lưu trữ và phục hồi Play Queue giữa các phiên.
    /// KHÔNG chịu trách nhiệm: Giữ trạng thái con trỏ bài hát đang phát.
    /// Vòng đời: Transient/Scoped.
    /// Luồng/DB: Thao tác DB qua Task bất đồng bộ.
    /// </remarks>
    public class QueueRepository : IQueueRepository
    {
        private readonly string _connectionString;

        public QueueRepository(string connectionString = null)
        {
            _connectionString = connectionString ?? DatabaseInitializer.ConnectionString;
        }

        public async Task SaveQueueAsync(IEnumerable<int> trackIds)
        {
            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        using (var delCmd = new SQLiteCommand("DELETE FROM play_queue", conn, trans))
                        {
                            delCmd.ExecuteNonQuery();
                        }

                        if (trackIds != null && trackIds.Any())
                        {
                            using (var insCmd = new SQLiteCommand("INSERT INTO play_queue (position, track_id) VALUES (@pos, @tId)", conn, trans))
                            {
                                var pPos = insCmd.Parameters.Add("@pos", DbType.Int32);
                                var pTrackId = insCmd.Parameters.Add("@tId", DbType.Int32);

                                int pos = 0;
                                foreach (int id in trackIds)
                                {
                                    pPos.Value = pos++;
                                    pTrackId.Value = id;
                                    insCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        trans.Commit();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TrackEntity>> LoadQueueAsync()
        {
            return await Task.Run(() =>
            {
                var list = new List<TrackEntity>();
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT t.* 
                        FROM tracks t
                        INNER JOIN play_queue q ON t.id = q.track_id
                        ORDER BY q.position ASC";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new TrackEntity
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                TrackKey = reader["track_key"].ToString(),
                                SourceType = reader["source_type"].ToString(),
                                SourceId = reader["source_id"].ToString(),
                                Title = reader["title"].ToString(),
                                Artist = reader["artist"].ToString(),
                                Album = reader["album"] == DBNull.Value ? null : reader["album"].ToString(),
                                Genre = reader["genre"] == DBNull.Value ? null : reader["genre"].ToString(),
                                DurationSeconds = Convert.ToInt32(reader["duration_seconds"]),
                                Bitrate = Convert.ToInt32(reader["bitrate"]),
                                CoverUri = reader["cover_uri"] == DBNull.Value ? null : reader["cover_uri"].ToString(),
                                FileMTime = reader["file_mtime"] == DBNull.Value ? null : reader["file_mtime"].ToString(),
                                PlayCount = Convert.ToInt32(reader["play_count"]),
                                SkipCount = Convert.ToInt32(reader["skip_count"]),
                                IsFavorite = Convert.ToInt32(reader["is_favorite"]) == 1,
                                AffinityScore = Convert.ToDouble(reader["affinity_score"]),
                                LastPlayedAt = reader["last_played_at"] == DBNull.Value ? null : reader["last_played_at"].ToString(),
                                CreatedAt = reader["created_at"].ToString()
                            });
                        }
                    }
                }
                return list;
            }).ConfigureAwait(false);
        }

        public async Task ClearQueueAsync()
        {
            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("DELETE FROM play_queue", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }
    }
}
