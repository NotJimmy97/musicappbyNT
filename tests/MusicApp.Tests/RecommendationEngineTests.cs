using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Core.Common;
using MusicApp.Core.Models;
using MusicApp.Core.Persistence;
using MusicApp.Core.Persistence.Repositories;
using MusicApp.Core.Services;

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
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"rec_test_db_{Guid.NewGuid():N}.db");
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
                try { File.Delete(_tempDbFile); } catch { }
            }
        }

        [TestMethod]
        public async Task LogAction_Favorite_IncreasesAffinityScore()
        {
            // Arrange
            var track = new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Chạy Ngay Đi", "Sơn Tùng M-TP"),
                Title = "Chạy Ngay Đi",
                Artist = "Sơn Tùng M-TP",
                DurationSeconds = 240,
                SourceType = "local",
                SourceId = @"C:\Music\ChayNgayDi.mp3",
                AffinityScore = 1.0
            };
            int trackId = await _trackRepo.InsertOrUpdateAsync(track);

            // Act
            await _engine.LogActionAsync(trackId, "favorite");
            var updated = await _trackRepo.GetByIdAsync(trackId);

            // Assert
            Assert.IsNotNull(updated);
            Assert.AreEqual(11.0, updated.AffinityScore, 0.01);
        }

        [TestMethod]
        public void PickSmartShuffleNext_ReturnsCandidateFromPool()
        {
            // Arrange
            var pool = new List<TrackEntity>
            {
                new TrackEntity { Id = 1, Title = "Track 1", Artist = "Artist 1", AffinityScore = 20.0 },
                new TrackEntity { Id = 2, Title = "Track 2", Artist = "Artist 2", AffinityScore = 5.0 },
                new TrackEntity { Id = 3, Title = "Track 3", Artist = "Artist 3", AffinityScore = 1.0 }
            };

            // Act
            var picked = _engine.PickSmartShuffleNext(pool, currentTrackId: 1);

            // Assert
            Assert.IsNotNull(picked);
            Assert.AreNotEqual(1, picked.Id, "Should not immediately pick current track when multiple tracks exist.");
        }
    }
}
