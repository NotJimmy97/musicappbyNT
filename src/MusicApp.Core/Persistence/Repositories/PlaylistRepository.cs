using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using MusicApp.Core.Interfaces.Persistence;
using MusicApp.Core.Models;

namespace MusicApp.Core.Persistence.Repositories
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly string _connectionString;

        public PlaylistRepository(string connectionString = null)
        {
            _connectionString = connectionString ?? DatabaseInitializer.ConnectionString;
        }

        public async Task<IEnumerable<PlaylistEntity>> GetAllPlaylistsAsync()
        {
            return await Task.Run(() =>
            {
                var list = new List<PlaylistEntity>();
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT * FROM playlists ORDER BY name ASC", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new PlaylistEntity
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                Description = reader["description"] == DBNull.Value ? null : reader["description"].ToString(),
                                CoverUri = reader["cover_uri"] == DBNull.Value ? null : reader["cover_uri"].ToString(),
                                CreatedAt = reader["created_at"].ToString(),
                                UpdatedAt = reader["updated_at"].ToString()
                            });
                        }
                    }
                }
                return list;
            }).ConfigureAwait(false);
        }

        public async Task<PlaylistEntity> GetByIdAsync(int playlistId)
        {
            return await Task.Run(async () =>
            {
                PlaylistEntity playlist = null;
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT * FROM playlists WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", playlistId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                playlist = new PlaylistEntity
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Name = reader["name"].ToString(),
                                    Description = reader["description"] == DBNull.Value ? null : reader["description"].ToString(),
                                    CoverUri = reader["cover_uri"] == DBNull.Value ? null : reader["cover_uri"].ToString(),
                                    CreatedAt = reader["created_at"].ToString(),
                                    UpdatedAt = reader["updated_at"].ToString()
                                };
                            }
                        }
                    }
                }

                if (playlist != null)
                {
                    var tracks = await GetTracksInPlaylistAsync(playlistId).ConfigureAwait(false);
                    playlist.Tracks = new List<TrackEntity>(tracks);
                }

                return playlist;
            }).ConfigureAwait(false);
        }

        public async Task<PlaylistEntity> CreatePlaylistAsync(string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Ten playlist khong duoc de trong", nameof(name));
            }

            return await Task.Run(() =>
            {
                string now = DateTime.UtcNow.ToString("o");
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        INSERT INTO playlists (name, description, created_at, updated_at) 
                        VALUES (@name, @desc, @now, @now);
                        SELECT last_insert_rowid();";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name.Trim());
                        cmd.Parameters.AddWithValue("@desc", (object)description ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@now", now);

                        int newId = Convert.ToInt32(cmd.ExecuteScalar());
                        return new PlaylistEntity
                        {
                            Id = newId,
                            Name = name.Trim(),
                            Description = description,
                            CreatedAt = now,
                            UpdatedAt = now
                        };
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task UpdatePlaylistAsync(int playlistId, string name, string description)
        {
            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("UPDATE playlists SET name = @name, description = @desc, updated_at = @now WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", playlistId);
                        cmd.Parameters.AddWithValue("@name", name.Trim());
                        cmd.Parameters.AddWithValue("@desc", (object)description ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task DeletePlaylistAsync(int playlistId)
        {
            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("DELETE FROM playlists WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", playlistId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task AddTrackToPlaylistAsync(int playlistId, int trackId)
        {
            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string maxPosSql = "SELECT COALESCE(MAX(position), -1) + 1 FROM playlist_tracks WHERE playlist_id = @pId";
                    int nextPos = 0;
                    using (var maxCmd = new SQLiteCommand(maxPosSql, conn))
                    {
                        maxCmd.Parameters.AddWithValue("@pId", playlistId);
                        nextPos = Convert.ToInt32(maxCmd.ExecuteScalar());
                    }

                    string insertSql = @"
                        INSERT OR IGNORE INTO playlist_tracks (playlist_id, track_id, position, added_at)
                        VALUES (@pId, @tId, @pos, @now)";
                    using (var insertCmd = new SQLiteCommand(insertSql, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@pId", playlistId);
                        insertCmd.Parameters.AddWithValue("@tId", trackId);
                        insertCmd.Parameters.AddWithValue("@pos", nextPos);
                        insertCmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task RemoveTrackFromPlaylistAsync(int playlistId, int trackId)
        {
            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("DELETE FROM playlist_tracks WHERE playlist_id = @pId AND track_id = @tId", conn))
                    {
                        cmd.Parameters.AddWithValue("@pId", playlistId);
                        cmd.Parameters.AddWithValue("@tId", trackId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task ReorderTracksAsync(int playlistId, IEnumerable<int> orderedTrackIds)
        {
            if (orderedTrackIds == null) return;

            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        using (var cmd = new SQLiteCommand("UPDATE playlist_tracks SET position = @pos WHERE playlist_id = @pId AND track_id = @tId", conn, trans))
                        {
                            var pPos = cmd.Parameters.Add("@pos", DbType.Int32);
                            var pPlaylistId = cmd.Parameters.Add("@pId", DbType.Int32);
                            var pTrackId = cmd.Parameters.Add("@tId", DbType.Int32);

                            pPlaylistId.Value = playlistId;

                            int pos = 0;
                            foreach (int trackId in orderedTrackIds)
                            {
                                pPos.Value = pos++;
                                pTrackId.Value = trackId;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TrackEntity>> GetTracksInPlaylistAsync(int playlistId)
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
                        INNER JOIN playlist_tracks pt ON t.id = pt.track_id
                        WHERE pt.playlist_id = @pId
                        ORDER BY pt.position ASC";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@pId", playlistId);
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
                }
                return list;
            }).ConfigureAwait(false);
        }
    }
}
