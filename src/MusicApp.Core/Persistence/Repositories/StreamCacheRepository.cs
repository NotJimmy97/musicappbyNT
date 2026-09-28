using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using MusicApp.Core.Interfaces.Persistence;

namespace MusicApp.Core.Persistence.Repositories
{
    public class StreamCacheRepository : IStreamCacheRepository
    {
        private readonly string _connectionString;

        public StreamCacheRepository(string connectionString = null)
        {
            _connectionString = connectionString ?? DatabaseInitializer.ConnectionString;
        }

        public async Task<string> GetCachedFilePathAsync(string trackHash)
        {
            if (string.IsNullOrWhiteSpace(trackHash)) return null;

            return await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT file_path FROM stream_cache WHERE track_hash = @hash AND is_fully_cached = 1", conn))
                    {
                        cmd.Parameters.AddWithValue("@hash", trackHash);
                        var path = cmd.ExecuteScalar()?.ToString();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            return path;
                        }
                    }
                }
                return null;
            }).ConfigureAwait(false);
        }

        public async Task RegisterCacheFileAsync(string trackHash, string filePath, long sizeBytes, bool isFull)
        {
            if (string.IsNullOrWhiteSpace(trackHash) || string.IsNullOrWhiteSpace(filePath)) return;

            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        INSERT INTO stream_cache (track_hash, file_path, file_size_bytes, last_accessed_at, is_fully_cached)
                        VALUES (@hash, @path, @size, @now, @isFull)
                        ON CONFLICT(track_hash) DO UPDATE SET
                            file_path = excluded.file_path,
                            file_size_bytes = excluded.file_size_bytes,
                            last_accessed_at = excluded.last_accessed_at,
                            is_fully_cached = excluded.is_fully_cached;";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@hash", trackHash);
                        cmd.Parameters.AddWithValue("@path", filePath);
                        cmd.Parameters.AddWithValue("@size", sizeBytes);
                        cmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
                        cmd.Parameters.AddWithValue("@isFull", isFull ? 1 : 0);
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task TouchCacheAccessAsync(string trackHash)
        {
            if (string.IsNullOrWhiteSpace(trackHash)) return;

            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("UPDATE stream_cache SET last_accessed_at = @now WHERE track_hash = @hash", conn))
                    {
                        cmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
                        cmd.Parameters.AddWithValue("@hash", trackHash);
                        cmd.ExecuteNonQuery();
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task EvictOldestCacheAsync(long targetFreedBytes)
        {
            if (targetFreedBytes <= 0) return;

            await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    var toDelete = new List<Tuple<string, string, long>>();

                    using (var cmd = new SQLiteCommand("SELECT track_hash, file_path, file_size_bytes FROM stream_cache ORDER BY last_accessed_at ASC", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        long freed = 0;
                        while (reader.Read() && freed < targetFreedBytes)
                        {
                            string hash = reader.GetString(0);
                            string path = reader.GetString(1);
                            long size = reader.GetInt64(2);
                            toDelete.Add(Tuple.Create(hash, path, size));
                            freed += size;
                        }
                    }

                    foreach (var item in toDelete)
                    {
                        try
                        {
                            if (File.Exists(item.Item2))
                            {
                                File.Delete(item.Item2);
                            }
                        }
                        catch { }

                        using (var delCmd = new SQLiteCommand("DELETE FROM stream_cache WHERE track_hash = @hash", conn))
                        {
                            delCmd.Parameters.AddWithValue("@hash", item.Item1);
                            delCmd.ExecuteNonQuery();
                        }
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task<long> GetTotalCacheSizeAsync()
        {
            return await Task.Run(() =>
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT COALESCE(SUM(file_size_bytes), 0) FROM stream_cache", conn))
                    {
                        object res = cmd.ExecuteScalar();
                        return Convert.ToInt64(res);
                    }
                }
            }).ConfigureAwait(false);
        }
    }
}
