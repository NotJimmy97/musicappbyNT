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
    public class QueueRepositoryTests
    {
        private string _tempDbFile;
        private QueueRepository _queueRepo;
        private TrackRepository _trackRepo;

        [TestInitialize]
        public void Setup()
        {
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"queue_repo_test_{Guid.NewGuid():N}.db");
            string connStr = $"Data Source={_tempDbFile};Version=3;Journal Mode=WAL;Synchronous=NORMAL;Cache Size=-64000;Foreign Keys=True;Default Timeout=5;";
            DatabaseInitializer.SetCustomConnectionString(connStr);
            DatabaseInitializer.Initialize();

            _queueRepo = new QueueRepository(connStr);
            _trackRepo = new TrackRepository(connStr);
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
        public async Task SaveQueue_And_LoadQueue_PreservesTrackOrder()
        {
            // Arrange
            int track1Id = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Track 1", "Artist"),
                Title = "Track 1",
                Artist = "Artist",
                SourceType = "local",
                SourceId = @"C:\Music\1.mp3"
            });
            int track2Id = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Track 2", "Artist"),
                Title = "Track 2",
                Artist = "Artist",
                SourceType = "local",
                SourceId = @"C:\Music\2.mp3"
            });
            int track3Id = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Track 3", "Artist"),
                Title = "Track 3",
                Artist = "Artist",
                SourceType = "local",
                SourceId = @"C:\Music\3.mp3"
            });

            // Act - Lưu thứ tự 3 -> 1 -> 2
            await _queueRepo.SaveQueueAsync(new[] { track3Id, track1Id, track2Id });
            var loaded = (await _queueRepo.LoadQueueAsync()).ToList();

            // Assert
            Assert.AreEqual(3, loaded.Count);
            Assert.AreEqual("Track 3", loaded[0].Title);
            Assert.AreEqual("Track 1", loaded[1].Title);
            Assert.AreEqual("Track 2", loaded[2].Title);
        }

        [TestMethod]
        public async Task ClearQueue_RemovesAllQueueItems()
        {
            // Arrange
            int trackId = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Track", "Artist"),
                Title = "Track",
                Artist = "Artist",
                SourceType = "local",
                SourceId = @"C:\Music\1.mp3"
            });
            await _queueRepo.SaveQueueAsync(new[] { trackId });

            // Act
            await _queueRepo.ClearQueueAsync();
            var loaded = (await _queueRepo.LoadQueueAsync()).ToList();

            // Assert
            Assert.AreEqual(0, loaded.Count);
        }
    }
}
