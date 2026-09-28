using System;
using System.Data.SQLite;
using System.IO;

namespace MusicApp.Core.Persistence
{
    /// <summary>
    /// Khởi tạo và quản lý cấu trúc cơ sở dữ liệu SQLite.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Đảm bảo tạo bảng và migrate schema khi app khởi động.
    /// KHÔNG chịu trách nhiệm: Chứa logic truy vấn dữ liệu CRUD.
    /// Vòng đời: Các phương thức tĩnh, chạy một lần khi startup.
    /// Luồng/DB: Quản lý ConnectionString tĩnh và thực thi đồng bộ khi khởi động.
    /// </remarks>
    public static class DatabaseInitializer
    {
        private static string _customConnectionString;
        private static readonly object _initLock = new object();
        private static bool _isInitialized;

        /// <summary>
        /// Duong dan mac dinh toi tep co so du lieu (%LOCALAPPDATA%\MusicApp\musicapp.db).
        /// </summary>
        public static string DefaultDatabasePath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MusicApp", "musicapp.db");

        /// <summary>
        /// Thu muc luu tru anh thumbnail 120x120 (%LOCALAPPDATA%\MusicApp\Covers).
        /// </summary>
        public static string CoversDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MusicApp", "Covers");

        /// <summary>
        /// Thu muc luu tru tep cache stream CAS cua Spotify (%LOCALAPPDATA%\MusicApp\Cache).
        /// </summary>
        public static string AudioCacheDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MusicApp", "Cache");

        /// <summary>
        /// Chuoi ket noi SQLite chuan duoc toi uu hoa voi WAL Mode va Cache Memory.
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (!string.IsNullOrEmpty(_customConnectionString))
                {
                    return _customConnectionString;
                }

                string dbPath = DefaultDatabasePath;
                return $"Data Source={dbPath};Version=3;Journal Mode=WAL;Synchronous=NORMAL;Cache Size=-64000;Foreign Keys=True;Default Timeout=5;";
            }
        }

        /// <summary>
        /// Thiet lap chuoi ket noi tuy bien (chu yeu dung cho Unit Tests voi database tam).
        /// </summary>
        public static void SetCustomConnectionString(string connectionString)
        {
            lock (_initLock)
            {
                _customConnectionString = connectionString;
                _isInitialized = false;
            }
        }

        /// <summary>
        /// Khoi tao co so du lieu: tao thu muc, tao bang, thiet lap PRAGMA va seed du lieu mac dinh.
        /// Thao tac nay la Idempotent (co the chay nhieu lan an toan).
        /// </summary>
        public static void Initialize()
        {
            lock (_initLock)
            {
                if (_isInitialized) return;

                // Neu dung duong dan mac dinh, dam bao cac thu muc ton tai
                if (string.IsNullOrEmpty(_customConnectionString))
                {
                    string dir = Path.GetDirectoryName(DefaultDatabasePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    if (!Directory.Exists(CoversDirectory))
                    {
                        Directory.CreateDirectory(CoversDirectory);
                    }

                    if (!Directory.Exists(AudioCacheDirectory))
                    {
                        Directory.CreateDirectory(AudioCacheDirectory);
                    }
                }

                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = GetSchemaSql();
                        cmd.ExecuteNonQuery();
                    }

                    SeedDefaultEqPresets(connection);
                }

                _isInitialized = true;
            }
        }

        /// <summary>
        /// Mo mot ket noi SQLite moi da duoc bat cac PRAGMA toc do cao.
        /// </summary>
        public static SQLiteConnection CreateConnection()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        private static string GetSchemaSql()
        {
            return @"
                PRAGMA journal_mode = WAL;
                PRAGMA synchronous = NORMAL;
                PRAGMA cache_size = -64000;
                PRAGMA temp_store = MEMORY;
                PRAGMA foreign_keys = ON;

                -- 1. BẢNG CÀI ĐẶT ỨNG DỤNG
                CREATE TABLE IF NOT EXISTS app_settings (
                    key   TEXT PRIMARY KEY,
                    value TEXT NOT NULL
                );

                -- 2. KHO BÀI HÁT TỔNG HỢP (UNIFIED TRACK CATALOG)
                CREATE TABLE IF NOT EXISTS tracks (
                    id               INTEGER PRIMARY KEY AUTOINCREMENT,
                    track_key        TEXT NOT NULL UNIQUE,
                    source_type      TEXT NOT NULL,
                    source_id        TEXT NOT NULL,
                    title            TEXT NOT NULL,
                    artist           TEXT NOT NULL,
                    album            TEXT,
                    genre            TEXT,
                    duration_seconds INTEGER NOT NULL DEFAULT 0,
                    bitrate          INTEGER DEFAULT 128,
                    cover_uri        TEXT,
                    file_mtime       TEXT,
                    play_count       INTEGER NOT NULL DEFAULT 0,
                    skip_count       INTEGER NOT NULL DEFAULT 0,
                    is_favorite      INTEGER NOT NULL DEFAULT 0,
                    affinity_score   REAL NOT NULL DEFAULT 0.0,
                    last_played_at   TEXT,
                    created_at       TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS idx_tracks_artist ON tracks(artist);
                CREATE INDEX IF NOT EXISTS idx_tracks_album ON tracks(album);
                CREATE INDEX IF NOT EXISTS idx_tracks_favorite ON tracks(is_favorite);
                CREATE INDEX IF NOT EXISTS idx_tracks_affinity ON tracks(affinity_score DESC);

                -- 3. QUẢN LÝ THƯ MỤC SCAN LOCAL
                CREATE TABLE IF NOT EXISTS library_folders (
                    folder_path     TEXT PRIMARY KEY,
                    last_scanned_at TEXT NOT NULL,
                    total_files     INTEGER NOT NULL DEFAULT 0
                );

                -- 4. BỘ NHỚ ĐỆM TỆP STREAM TRỰC TUYẾN (SPOTIFY DISK CACHE)
                CREATE TABLE IF NOT EXISTS stream_cache (
                    track_hash       TEXT PRIMARY KEY,
                    file_path        TEXT NOT NULL,
                    file_size_bytes  INTEGER NOT NULL,
                    last_accessed_at TEXT NOT NULL,
                    is_fully_cached  INTEGER NOT NULL DEFAULT 0
                );
                CREATE INDEX IF NOT EXISTS idx_cache_accessed ON stream_cache(last_accessed_at ASC);

                -- 5. DANH SÁCH PHÁT (CUSTOM PLAYLISTS)
                CREATE TABLE IF NOT EXISTS playlists (
                    id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    name        TEXT NOT NULL UNIQUE,
                    description TEXT,
                    cover_uri   TEXT,
                    created_at  TEXT NOT NULL,
                    updated_at  TEXT NOT NULL
                );

                -- 6. LIÊN KẾT PLAYLIST VÀ BÀI HÁT
                CREATE TABLE IF NOT EXISTS playlist_tracks (
                    playlist_id INTEGER NOT NULL REFERENCES playlists(id) ON DELETE CASCADE,
                    track_id    INTEGER NOT NULL REFERENCES tracks(id) ON DELETE CASCADE,
                    position    INTEGER NOT NULL,
                    added_at    TEXT NOT NULL,
                    PRIMARY KEY (playlist_id, track_id)
                );
                CREATE INDEX IF NOT EXISTS idx_playlist_pos ON playlist_tracks(playlist_id, position ASC);

                -- 7. HÀNG ĐỢI PHÁT NHẠC HIỆN TẠI (PLAY QUEUE)
                CREATE TABLE IF NOT EXISTS play_queue (
                    position INTEGER PRIMARY KEY,
                    track_id INTEGER NOT NULL REFERENCES tracks(id) ON DELETE CASCADE
                );

                -- 8. NHẬT KÝ TƯƠNG TÁC NGƯỜI DÙNG (USER INTERACTIONS)
                CREATE TABLE IF NOT EXISTS user_interactions (
                    id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    track_id        INTEGER NOT NULL REFERENCES tracks(id) ON DELETE CASCADE,
                    action_type     TEXT NOT NULL,
                    duration_played INTEGER DEFAULT 0,
                    created_at      TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS idx_interactions_track ON user_interactions(track_id);
                CREATE INDEX IF NOT EXISTS idx_interactions_time ON user_interactions(created_at DESC);

                -- 9. BỘ LỌC CÂN BẰNG ÂM SẮC (EQ PRESETS)
                CREATE TABLE IF NOT EXISTS eq_presets (
                    name        TEXT PRIMARY KEY,
                    gains_json  TEXT NOT NULL,
                    is_custom   INTEGER NOT NULL DEFAULT 0
                );

                PRAGMA user_version = 1;
            ";
        }

        private static void SeedDefaultEqPresets(SQLiteConnection connection)
        {
            using (var checkCmd = new SQLiteCommand("SELECT COUNT(*) FROM eq_presets", connection))
            {
                long count = (long)checkCmd.ExecuteScalar();
                if (count > 0) return;
            }

            var defaultPresets = new[]
            {
                new { Name = "Flat", Gains = "[0,0,0,0,0,0,0,0,0,0]" },
                new { Name = "Rock", Gains = "[4.5,3.0,1.5,0.0,-1.0,-0.5,1.5,2.5,3.5,4.0]" },
                new { Name = "Pop", Gains = "[-1.5,-1.0,0.5,2.0,3.5,3.0,1.5,0.5,-0.5,-1.0]" },
                new { Name = "Jazz", Gains = "[3.0,2.0,1.0,1.5,-1.5,-1.5,0.0,1.5,2.5,3.0]" },
                new { Name = "Classical", Gains = "[4.0,3.0,2.5,2.0,-1.0,-1.0,0.0,1.5,2.5,3.5]" },
                new { Name = "Bass Boost", Gains = "[6.0,5.0,4.0,2.0,0.5,0.0,-1.0,-1.5,-2.0,-2.0]" },
                new { Name = "Vocal Boost", Gains = "[-2.0,-2.0,-1.0,1.5,4.0,4.5,3.5,1.5,0.0,-1.0]" },
                new { Name = "Treble Boost", Gains = "[-2.0,-2.0,-1.5,-1.0,0.0,1.5,3.0,4.5,5.5,6.0]" }
            };

            using (var transaction = connection.BeginTransaction())
            {
                using (var insertCmd = new SQLiteCommand("INSERT OR IGNORE INTO eq_presets (name, gains_json, is_custom) VALUES (@name, @gains, 0)", connection, transaction))
                {
                    var pName = insertCmd.Parameters.Add("@name", System.Data.DbType.String);
                    var pGains = insertCmd.Parameters.Add("@gains", System.Data.DbType.String);

                    foreach (var preset in defaultPresets)
                    {
                        pName.Value = preset.Name;
                        pGains.Value = preset.Gains;
                        insertCmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }
    }
}
