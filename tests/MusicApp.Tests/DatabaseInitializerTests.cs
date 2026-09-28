using System;
using System.Data.SQLite;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Core.Persistence;

namespace MusicApp.Tests
{
    [TestClass]
    public class DatabaseInitializerTests
    {
        private string _tempDbFile;

        [TestInitialize]
        public void Setup()
        {
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"musicapp_test_{Guid.NewGuid():N}.db");
            DatabaseInitializer.SetCustomConnectionString($"Data Source={_tempDbFile};Version=3;Journal Mode=WAL;Synchronous=NORMAL;Cache Size=-64000;Foreign Keys=True;Default Timeout=5;");
        }

        [TestCleanup]
        public void Cleanup()
        {
            DatabaseInitializer.ResetInitialization();
            if (File.Exists(_tempDbFile))
            {
                try
                {
                    File.Delete(_tempDbFile);
                }
                catch
                {
                    // Ignore SQLite file lock on immediate cleanup
                }
            }
        }

        [TestMethod]
        public void Initialize_CreatesDatabaseAndAllRequiredTables()
        {
            // Act
            DatabaseInitializer.Initialize();

            // Assert
            Assert.IsTrue(File.Exists(_tempDbFile), "Database file must be created on disk.");

            using (var conn = new SQLiteConnection(DatabaseInitializer.ConnectionString))
            {
                conn.Open();
                string[] expectedTables = new[]
                {
                    "tracks",
                    "playlists",
                    "playlist_tracks",
                    "play_queue",
                    "user_interactions",
                    "scanned_folders",
                    "stream_cache",
                    "app_settings",
                    "eq_presets"
                };

                foreach (var table in expectedTables)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@tableName;";
                        cmd.Parameters.AddWithValue("@tableName", table);
                        long count = (long)cmd.ExecuteScalar();
                        Assert.AreEqual(1, count, $"Table '{table}' should exist in the SQLite database.");
                    }
                }
            }
        }

        [TestMethod]
        public void Initialize_IsIdempotent_CanBeCalledMultipleTimes()
        {
            // Act & Assert (Should not throw)
            DatabaseInitializer.Initialize();
            DatabaseInitializer.Initialize();
            DatabaseInitializer.Initialize();

            Assert.IsTrue(File.Exists(_tempDbFile));
        }

        [TestMethod]
        public void Initialize_SeedsDefaultEqPresets()
        {
            // Act
            DatabaseInitializer.Initialize();

            // Assert
            using (var conn = new SQLiteConnection(DatabaseInitializer.ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM eq_presets;";
                    long presetCount = (long)cmd.ExecuteScalar();
                    Assert.IsTrue(presetCount >= 8, $"Database should seed at least 8 default EQ presets, found {presetCount}.");
                }
            }
        }
    }
}
