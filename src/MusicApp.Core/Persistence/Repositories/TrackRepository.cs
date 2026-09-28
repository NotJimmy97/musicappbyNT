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
    /// Repository quản lý dữ liệu bài hát.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Lưu trữ metadata bản nhạc offline và cache trực tuyến.
    /// KHÔNG chịu trách nhiệm: Tải stream hay quét thư mục.
    /// Vòng đời: Transient/Scoped.
    /// Luồng/DB: Thao tác DB qua Task bất đồng bộ.
    /// </remarks>
    public class TrackRepository : ITrackRepository
    {
        private readonly string _connectionString;

        public TrackRepository(string connectionString = null)
        {
            _connectionString = connectionString ?? DatabaseInitializer.ConnectionString;
        }

        public async Task<TrackEntity> GetByIdAsync(int id)
        {
            return await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT * FROM tracks WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapTrack(reader);
                            }
                        }
                    }
                }
                return null;
            }).ConfigureAwait(false);
        }

        public async Task<TrackEntity> GetByTrackKeyAsync(string trackKey)
        {
            if (string.IsNullOrWhiteSpace(trackKey)) return null;

            return await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT * FROM tracks WHERE track_key = @track_key", conn))
                    {
                        cmd.Parameters.AddWithValue("@track_key", trackKey);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapTrack(reader);
                            }
                        }
                    }
                }
                return null;
            }).ConfigureAwait(false);
        }

        public async Task<TrackEntity> GetByFilePathAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;

            return await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT * FROM tracks WHERE source_type = 'local' AND source_id = @filePath", conn))
                    {
                        cmd.Parameters.AddWithValue("@filePath", filePath);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapTrack(reader);
                            }
                        }
                    }
                }
                return null;
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TrackEntity>> GetAllLocalTracksAsync()
        {
            return await Task.Run(() =>
            {
                var list = new List<TrackEntity>();
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT * FROM tracks WHERE source_type = 'local' ORDER BY title ASC", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapTrack(reader));
                        }
                    }
                }
                return list;
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TrackEntity>> GetFavoritesAsync()
        {
            return await Task.Run(() =>
            {
                var list = new List<TrackEntity>();
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT * FROM tracks WHERE is_favorite = 1 ORDER BY last_played_at DESC, title ASC", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapTrack(reader));
                        }
                    }
                }
                return list;
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TrackEntity>> SearchAsync(string keyword, int limit = 50)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await GetAllLocalTracksAsync().ConfigureAwait(false);
            }

            return await Task.Run(() =>
            {
                var list = new List<TrackEntity>();
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT * FROM tracks 
                        WHERE title LIKE @kw OR artist LIKE @kw OR album LIKE @kw
                        ORDER BY affinity_score DESC, play_count DESC
                        LIMIT @limit";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@kw", $"%{keyword.Trim()}%");
                        cmd.Parameters.AddWithValue("@limit", Math.Max(1, limit));
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(MapTrack(reader));
                            }
                        }
                    }
                }
                return list;
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TrackEntity>> GetRecommendationsAsync(int limit = 25)
        {
            return await Task.Run(() =>
            {
                var list = new List<TrackEntity>();
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT * FROM tracks 
                        ORDER BY is_favorite DESC, affinity_score DESC, play_count DESC, random()
                        LIMIT @limit";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@limit", Math.Max(1, limit));
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(MapTrack(reader));
                            }
                        }
                    }
                }
                return list;
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TrackEntity>> GetSimilarTracksAsync(int trackId, int limit = 10)
        {
            var current = await GetByIdAsync(trackId).ConfigureAwait(false);
            if (current == null) return Enumerable.Empty<TrackEntity>();

            return await Task.Run(() =>
            {
                var list = new List<TrackEntity>();
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT * FROM tracks 
                        WHERE id != @id AND (artist = @artist OR (genre IS NOT NULL AND genre != '' AND genre = @genre))
                        ORDER BY is_favorite DESC, affinity_score DESC
                        LIMIT @limit";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", trackId);
                        cmd.Parameters.AddWithValue("@artist", current.Artist ?? string.Empty);
                        cmd.Parameters.AddWithValue("@genre", current.Genre ?? string.Empty);
                        cmd.Parameters.AddWithValue("@limit", Math.Max(1, limit));
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(MapTrack(reader));
                            }
                        }
                    }
                }
                return list;
            }).ConfigureAwait(false);
        }

        public async Task<int> InsertOrUpdateAsync(TrackEntity track)
        {
            if (track == null) return 0;

            return await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        INSERT INTO tracks (
                            track_key, source_type, source_id, title, artist, album, genre,
                            duration_seconds, bitrate, cover_uri, file_mtime, play_count,
                            skip_count, is_favorite, affinity_score, last_played_at, created_at
                        ) VALUES (
                            @track_key, @source_type, @source_id, @title, @artist, @album, @genre,
                            @duration_seconds, @bitrate, @cover_uri, @file_mtime, @play_count,
                            @skip_count, @is_favorite, @affinity_score, @last_played_at, @created_at
                        )
                        ON CONFLICT(track_key) DO UPDATE SET
                            source_type = excluded.source_type,
                            source_id = excluded.source_id,
                            title = excluded.title,
                            artist = excluded.artist,
                            album = excluded.album,
                            genre = excluded.genre,
                            duration_seconds = excluded.duration_seconds,
                            bitrate = excluded.bitrate,
                            cover_uri = COALESCE(excluded.cover_uri, tracks.cover_uri),
                            file_mtime = excluded.file_mtime;
                        SELECT last_insert_rowid();";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        PopulateTrackParameters(cmd, track);
                        object result = cmd.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out int id))
                        {
                            track.Id = id;
                            return id;
                        }
                    }
                }
                return 0;
            }).ConfigureAwait(false);
        }

        public async Task BatchInsertOrUpdateAsync(IEnumerable<TrackEntity> tracks)
        {
            if (tracks == null || !tracks.Any()) return;

            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        string sql = @"
                            INSERT INTO tracks (
                                track_key, source_type, source_id, title, artist, album, genre,
                                duration_seconds, bitrate, cover_uri, file_mtime, play_count,
                                skip_count, is_favorite, affinity_score, last_played_at, created_at
                            ) VALUES (
                                @track_key, @source_type, @source_id, @title, @artist, @album, @genre,
                                @duration_seconds, @bitrate, @cover_uri, @file_mtime, @play_count,
                                @skip_count, @is_favorite, @affinity_score, @last_played_at, @created_at
                            )
                            ON CONFLICT(track_key) DO UPDATE SET
                                source_type = excluded.source_type,
                                source_id = excluded.source_id,
                                title = excluded.title,
                                artist = excluded.artist,
                                album = excluded.album,
                                genre = excluded.genre,
                                duration_seconds = excluded.duration_seconds,
                                bitrate = excluded.bitrate,
                                cover_uri = COALESCE(excluded.cover_uri, tracks.cover_uri),
                                file_mtime = excluded.file_mtime;";

                        using (var cmd = new SQLiteCommand(sql, conn, transaction))
                        {
                            var pKey = cmd.Parameters.Add("@track_key", DbType.String);
                            var pSrcType = cmd.Parameters.Add("@source_type", DbType.String);
                            var pSrcId = cmd.Parameters.Add("@source_id", DbType.String);
                            var pTitle = cmd.Parameters.Add("@title", DbType.String);
                            var pArtist = cmd.Parameters.Add("@artist", DbType.String);
                            var pAlbum = cmd.Parameters.Add("@album", DbType.String);
                            var pGenre = cmd.Parameters.Add("@genre", DbType.String);
                            var pDuration = cmd.Parameters.Add("@duration_seconds", DbType.Int32);
                            var pBitrate = cmd.Parameters.Add("@bitrate", DbType.Int32);
                            var pCover = cmd.Parameters.Add("@cover_uri", DbType.String);
                            var pMTime = cmd.Parameters.Add("@file_mtime", DbType.String);
                            var pPlays = cmd.Parameters.Add("@play_count", DbType.Int32);
                            var pSkips = cmd.Parameters.Add("@skip_count", DbType.Int32);
                            var pFav = cmd.Parameters.Add("@is_favorite", DbType.Int32);
                            var pAffinity = cmd.Parameters.Add("@affinity_score", DbType.Double);
                            var pLastPlayed = cmd.Parameters.Add("@last_played_at", DbType.String);
                            var pCreated = cmd.Parameters.Add("@created_at", DbType.String);

                            foreach (var track in tracks)
                            {
                                pKey.Value = track.TrackKey ?? string.Empty;
                                pSrcType.Value = track.SourceType ?? "local";
                                pSrcId.Value = track.SourceId ?? string.Empty;
                                pTitle.Value = track.Title ?? string.Empty;
                                pArtist.Value = track.Artist ?? "Unknown Artist";
                                pAlbum.Value = (object)track.Album ?? DBNull.Value;
                                pGenre.Value = (object)track.Genre ?? DBNull.Value;
                                pDuration.Value = track.DurationSeconds;
                                pBitrate.Value = track.Bitrate;
                                pCover.Value = (object)track.CoverUri ?? DBNull.Value;
                                pMTime.Value = (object)track.FileMTime ?? DBNull.Value;
                                pPlays.Value = track.PlayCount;
                                pSkips.Value = track.SkipCount;
                                pFav.Value = track.IsFavorite ? 1 : 0;
                                pAffinity.Value = track.AffinityScore;
                                pLastPlayed.Value = (object)track.LastPlayedAt ?? DBNull.Value;
                                pCreated.Value = track.CreatedAt ?? DateTime.UtcNow.ToString("o");

                                cmd.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task UpdateAffinityScoreAsync(int trackId, double deltaScore)
        {
            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("UPDATE tracks SET affinity_score = MAX(0.0, affinity_score + @delta) WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@delta", deltaScore);
                        cmd.Parameters.AddWithValue("@id", trackId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task ToggleFavoriteAsync(int trackId)
        {
            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(@"
                        UPDATE tracks 
                        SET is_favorite = CASE WHEN is_favorite = 1 THEN 0 ELSE 1 END,
                            affinity_score = CASE WHEN is_favorite = 0 THEN affinity_score + 10.0 ELSE MAX(0.0, affinity_score - 10.0) END
                        WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", trackId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task IncrementPlayCountAsync(int trackId)
        {
            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(@"
                        UPDATE tracks 
                        SET play_count = play_count + 1,
                            last_played_at = @now,
                            affinity_score = affinity_score + 2.0
                        WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
                        cmd.Parameters.AddWithValue("@id", trackId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task IncrementSkipCountAsync(int trackId)
        {
            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(@"
                        UPDATE tracks 
                        SET skip_count = skip_count + 1,
                            affinity_score = MAX(0.0, affinity_score - 4.0)
                        WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", trackId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task DeleteTracksNotInFilesAsync(IEnumerable<string> existingFilePaths)
        {
            if (existingFilePaths == null) return;

            var pathSet = new HashSet<string>(existingFilePaths, StringComparer.OrdinalIgnoreCase);

            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    var toDelete = new List<int>();

                    using (var cmd = new SQLiteCommand("SELECT id, source_id FROM tracks WHERE source_type = 'local'", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string path = reader.GetString(1);
                            if (!pathSet.Contains(path))
                            {
                                toDelete.Add(id);
                            }
                        }
                    }

                    if (toDelete.Count > 0)
                    {
                        using (var trans = conn.BeginTransaction())
                        {
                            using (var delCmd = new SQLiteCommand("DELETE FROM tracks WHERE id = @id", conn, trans))
                            {
                                var pId = delCmd.Parameters.Add("@id", DbType.Int32);
                                foreach (int id in toDelete)
                                {
                                    pId.Value = id;
                                    delCmd.ExecuteNonQuery();
                                }
                            }
                            trans.Commit();
                        }
                    }
                }
            }).ConfigureAwait(false);
        }

        private static void PopulateTrackParameters(SQLiteCommand cmd, TrackEntity track)
        {
            cmd.Parameters.AddWithValue("@track_key", track.TrackKey ?? string.Empty);
            cmd.Parameters.AddWithValue("@source_type", track.SourceType ?? "local");
            cmd.Parameters.AddWithValue("@source_id", track.SourceId ?? string.Empty);
            cmd.Parameters.AddWithValue("@title", track.Title ?? string.Empty);
            cmd.Parameters.AddWithValue("@artist", track.Artist ?? "Unknown Artist");
            cmd.Parameters.AddWithValue("@album", (object)track.Album ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@genre", (object)track.Genre ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@duration_seconds", track.DurationSeconds);
            cmd.Parameters.AddWithValue("@bitrate", track.Bitrate);
            cmd.Parameters.AddWithValue("@cover_uri", (object)track.CoverUri ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@file_mtime", (object)track.FileMTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@play_count", track.PlayCount);
            cmd.Parameters.AddWithValue("@skip_count", track.SkipCount);
            cmd.Parameters.AddWithValue("@is_favorite", track.IsFavorite ? 1 : 0);
            cmd.Parameters.AddWithValue("@affinity_score", track.AffinityScore);
            cmd.Parameters.AddWithValue("@last_played_at", (object)track.LastPlayedAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@created_at", track.CreatedAt ?? DateTime.UtcNow.ToString("o"));
        }

        private static TrackEntity MapTrack(SQLiteDataReader reader)
        {
            return new TrackEntity
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
            };
        }
    }
}
