using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Core.Models;
using MusicApp.Core.Persistence;
using MusicApp.Core.Persistence.Repositories;
using MusicApp.Core.Services;
using MusicApp.Core.Utilities;

namespace MusicApp.Tests
{
    [TestClass]
    public class RecommendationEngineTests
    {
        private string _tempDbFile;
        private TrackRepository _trackRepo;
        private InteractionRepository _interactionRepo;
        private RecommendationEngine _engine;

        [TestInitialize]
        public void Setup()
        {
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"rec_engine_test_{Guid.NewGuid():N}.db");
            string connStr = $"Data Source={_tempDbFile};Version=3;Journal Mode=WAL;Synchronous=NORMAL;Cache Size=-64000;Foreign Keys=True;Default Timeout=5;";
            DatabaseInitializer.SetCustomConnectionString(connStr);
            DatabaseInitializer.Initialize();

            _trackRepo = new TrackRepository(connStr);
            _interactionRepo = new InteractionRepository(connStr);
            _engine = new RecommendationEngine(_trackRepo, _interactionRepo);
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
        public async Task LogAction_PlayComplete_IncreasesPlayCountAndAffinityScore()
        {
            // Arrange
            int trackId = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Hit Song", "Star"),
                Title = "Hit Song",
                Artist = "Star",
                SourceType = "local",
                SourceId = @"C:\Music\hit.mp3",
                PlayCount = 0,
                AffinityScore = 1.0
            });

            // Act
            await _engine.LogActionAsync(trackId, "play_complete", 200);
            var updated = await _trackRepo.GetByIdAsync(trackId);

            // Assert
            Assert.IsNotNull(updated);
            Assert.AreEqual(1, updated.PlayCount, "Play count should be incremented.");
            Assert.AreEqual(6.0, updated.AffinityScore, 0.01, "Affinity score should increase by 5.0.");
        }

        [TestMethod]
        public async Task LogAction_Skip_IncreasesSkipCountAndDecreasesAffinityScore()
        {
            // Arrange
            int trackId = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Skip Song", "Artist"),
                Title = "Skip Song",
                Artist = "Artist",
                SourceType = "local",
                SourceId = @"C:\Music\skip.mp3",
                SkipCount = 0,
                AffinityScore = 10.0
            });

            // Act
            await _engine.LogActionAsync(trackId, "skip", 5);
            var updated = await _trackRepo.GetByIdAsync(trackId);

            // Assert
            Assert.IsNotNull(updated);
            Assert.AreEqual(1, updated.SkipCount, "Skip count should be incremented.");
            Assert.AreEqual(6.0, updated.AffinityScore, 0.01, "Affinity score should decrease by 4.0.");
        }

        [TestMethod]
        public async Task LogAction_Favorite_ProvidesSubstantialAffinityBoost()
        {
            // Arrange
            int trackId = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Fav Song", "Artist"),
                Title = "Fav Song",
                Artist = "Artist",
                SourceType = "local",
                SourceId = @"C:\Music\fav.mp3",
                AffinityScore = 2.0
            });

            // Act
            await _engine.LogActionAsync(trackId, "favorite");
            var updated = await _trackRepo.GetByIdAsync(trackId);

            // Assert
            Assert.IsNotNull(updated);
            Assert.AreEqual(12.0, updated.AffinityScore, 0.01, "Favorite should add +10.0 affinity.");
        }

        [TestMethod]
        public void PickSmartShuffleNext_SelectsCandidateDifferentFromCurrentTrack()
        {
            // Arrange
            var pool = new List<TrackEntity>
            {
                new TrackEntity { Id = 1, Title = "Track 1", AffinityScore = 2.0 },
                new TrackEntity { Id = 2, Title = "Track 2", AffinityScore = 20.0 },
                new TrackEntity { Id = 3, Title = "Track 3", AffinityScore = 5.0 }
            };

            // Act
            var selected = _engine.PickSmartShuffleNext(pool, currentTrackId: 1);

            // Assert
            Assert.IsNotNull(selected);
            Assert.AreNotEqual(1, selected.Id, "Smart shuffle should not pick the currently playing track when alternatives exist.");
            Assert.IsTrue(pool.Any(t => t.Id == selected.Id));
        }

        [TestMethod]
        public void PickSmartShuffleNext_SingleTrackPool_ReturnsSameTrack()
        {
            // Arrange
            var pool = new List<TrackEntity>
            {
                new TrackEntity { Id = 1, Title = "Only Track", AffinityScore = 5.0 }
            };

            // Act
            var selected = _engine.PickSmartShuffleNext(pool, currentTrackId: 1);

            // Assert
            Assert.IsNotNull(selected);
            Assert.AreEqual(1, selected.Id);
        }
    }
}
