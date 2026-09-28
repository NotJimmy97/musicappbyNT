using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Core.Models;
using MusicApp.Core.Persistence;
using MusicApp.Core.Persistence.Repositories;
using MusicApp.Core.Utilities;

namespace MusicApp.Tests
{
    [TestClass]
    public class TrackRepositoryTests
    {
        private string _tempDbFile;
        private TrackRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"track_repo_test_{Guid.NewGuid():N}.db");
            string connStr = $"Data Source={_tempDbFile};Version=3;Journal Mode=WAL;Synchronous=NORMAL;Cache Size=-64000;Foreign Keys=True;Default Timeout=5;";
            DatabaseInitializer.SetCustomConnectionString(connStr);
            DatabaseInitializer.Initialize();

            _repository = new TrackRepository(connStr);
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
        public async Task InsertOrUpdateTrack_SavesAndRetrievesTrack()
        {
            // Arrange
            var track = new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Lạc Trôi", "Sơn Tùng M-TP"),
                Title = "Lạc Trôi",
                Artist = "Sơn Tùng M-TP",
                Album = "Lạc Trôi Single",
                Duration = 240,
                SourceType = "local",
                SourceId = @"C:\Music\Lactroi.mp3",
                IsFavorite = false,
                AffinityScore = 1.0
            };

            // Act
            int trackId = await _repository.InsertOrUpdateAsync(track);
            var retrieved = await _repository.GetByIdAsync(trackId);

            // Assert
            Assert.IsTrue(trackId > 0, "Inserted track ID should be positive.");
            Assert.IsNotNull(retrieved, "Track should be retrieved from repository.");
            Assert.AreEqual("Lạc Trôi", retrieved.Title);
            Assert.AreEqual("Sơn Tùng M-TP", retrieved.Artist);
            Assert.AreEqual(240, retrieved.Duration);
            Assert.AreEqual(1.0, retrieved.AffinityScore);
        }

        [TestMethod]
        public async Task BatchInsertOrUpdate_InsertsMultipleTracksCorrectly()
        {
            // Arrange
            var tracks = new List<TrackEntity>
            {
                new TrackEntity
                {
                    TrackKey = TrackIdentityHelper.GenerateTrackKey("Song A", "Artist 1"),
                    Title = "Song A",
                    Artist = "Artist 1",
                    SourceType = "local",
                    SourceId = @"C:\Music\SongA.mp3",
                    Duration = 180
                },
                new TrackEntity
                {
                    TrackKey = TrackIdentityHelper.GenerateTrackKey("Song B", "Artist 2"),
                    Title = "Song B",
                    Artist = "Artist 2",
                    SourceType = "local",
                    SourceId = @"C:\Music\SongB.mp3",
                    Duration = 210
                }
            };

            // Act
            int count = await _repository.BatchInsertOrUpdateAsync(tracks);
            var allTracks = (await _repository.GetAllLocalTracksAsync()).ToList();

            // Assert
            Assert.AreEqual(2, count, "Batch insert count should match list size.");
            Assert.AreEqual(2, allTracks.Count);
            Assert.IsTrue(allTracks.Any(t => t.Title == "Song A"));
            Assert.IsTrue(allTracks.Any(t => t.Title == "Song B"));
        }

        [TestMethod]
        public async Task ToggleFavorite_UpdatesIsFavoriteStatus()
        {
            // Arrange
            var track = new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Waiting For You", "MONO"),
                Title = "Waiting For You",
                Artist = "MONO",
                SourceType = "local",
                SourceId = @"C:\Music\WaitingForYou.mp3",
                IsFavorite = false
            };
            int trackId = await _repository.InsertOrUpdateAsync(track);

            // Act - Toggle to True
            bool newStatus = await _repository.ToggleFavoriteAsync(trackId);
            var favorites = (await _repository.GetFavoritesAsync()).ToList();

            // Assert
            Assert.IsTrue(newStatus, "Status should be toggled to true.");
            Assert.AreEqual(1, favorites.Count);
            Assert.AreEqual("Waiting For You", favorites[0].Title);

            // Act - Toggle back to False
            bool secondStatus = await _repository.ToggleFavoriteAsync(trackId);
            var favoritesAfter = (await _repository.GetFavoritesAsync()).ToList();

            // Assert
            Assert.IsFalse(secondStatus, "Status should be toggled back to false.");
            Assert.AreEqual(0, favoritesAfter.Count);
        }

        [TestMethod]
        public async Task SearchAsync_FindsMatchingTracksByTitleOrArtist()
        {
            // Arrange
            var tracks = new List<TrackEntity>
            {
                new TrackEntity
                {
                    TrackKey = TrackIdentityHelper.GenerateTrackKey("Nắng Ấm Xa Dần", "Sơn Tùng M-TP"),
                    Title = "Nắng Ấm Xa Dần",
                    Artist = "Sơn Tùng M-TP",
                    SourceType = "local",
                    SourceId = @"C:\Music\NangAm.mp3"
                },
                new TrackEntity
                {
                    TrackKey = TrackIdentityHelper.GenerateTrackKey("Cơn Mưa Ngang Qua", "Sơn Tùng M-TP"),
                    Title = "Cơn Mưa Ngang Qua",
                    Artist = "Sơn Tùng M-TP",
                    SourceType = "local",
                    SourceId = @"C:\Music\ConMua.mp3"
                },
                new TrackEntity
                {
                    TrackKey = TrackIdentityHelper.GenerateTrackKey("See Tình", "Hoàng Thùy Linh"),
                    Title = "See Tình",
                    Artist = "Hoàng Thùy Linh",
                    SourceType = "local",
                    SourceId = @"C:\Music\SeeTinh.mp3"
                }
            };
            await _repository.BatchInsertOrUpdateAsync(tracks);

            // Act
            var searchByArtist = (await _repository.SearchAsync("Sơn Tùng")).ToList();
            var searchByTitle = (await _repository.SearchAsync("See")).ToList();

            // Assert
            Assert.AreEqual(2, searchByArtist.Count);
            Assert.AreEqual(1, searchByTitle.Count);
            Assert.AreEqual("See Tình", searchByTitle[0].Title);
        }

        [TestMethod]
        public async Task RecordPlayAndAffinityUpdates_WorkCorrectly()
        {
            // Arrange
            var track = new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Test Song", "Test Artist"),
                Title = "Test Song",
                Artist = "Test Artist",
                SourceType = "local",
                SourceId = @"C:\Music\Test.mp3",
                PlayCount = 0,
                AffinityScore = 1.0
            };
            int trackId = await _repository.InsertOrUpdateAsync(track);

            // Act
            await _repository.IncrementPlayCountAsync(trackId);
            await _repository.UpdateAffinityScoreAsync(trackId, 5.0);
            var updated = await _repository.GetByIdAsync(trackId);

            // Assert
            Assert.IsNotNull(updated);
            Assert.AreEqual(1, updated.PlayCount);
            Assert.AreEqual(6.0, updated.AffinityScore, 0.01);
        }
    }
}
