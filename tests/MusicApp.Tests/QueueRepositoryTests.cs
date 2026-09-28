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
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"queue_test_db_{Guid.NewGuid():N}.db");
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
                try { File.Delete(_tempDbFile); } catch { }
            }
        }

        [TestMethod]
        public async Task SaveQueue_And_LoadQueue_PreservesTrackOrder()
        {
            // Arrange
            var t1 = new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Q1", "Art1"),
                Title = "Q1",
                Artist = "Art1",
                DurationSeconds = 120,
                SourceType = "local",
                SourceId = @"C:\Music\q1.mp3"
            };
            var t2 = new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Q2", "Art2"),
                Title = "Q2",
                Artist = "Art2",
                DurationSeconds = 180,
                SourceType = "local",
                SourceId = @"C:\Music\q2.mp3"
            };
            int id1 = await _trackRepo.InsertOrUpdateAsync(t1);
            int id2 = await _trackRepo.InsertOrUpdateAsync(t2);

            // Act: Save queue with order [id2, id1]
            await _queueRepo.SaveQueueAsync(new List<int> { id2, id1 });
            var loaded = (await _queueRepo.LoadQueueAsync()).ToList();

            // Assert
            Assert.AreEqual(2, loaded.Count);
            Assert.AreEqual("Q2", loaded[0].Title);
            Assert.AreEqual("Q1", loaded[1].Title);
        }

        [TestMethod]
        public async Task ClearQueue_RemovesAllQueuedTracks()
        {
            // Arrange
            var t1 = new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Q3", "Art3"),
                Title = "Q3",
                Artist = "Art3",
                DurationSeconds = 100,
                SourceType = "local",
                SourceId = @"C:\Music\q3.mp3"
            };
            int id1 = await _trackRepo.InsertOrUpdateAsync(t1);
            await _queueRepo.SaveQueueAsync(new List<int> { id1 });

            // Act
            await _queueRepo.ClearQueueAsync();
            var loaded = (await _queueRepo.LoadQueueAsync()).ToList();

            // Assert
            Assert.AreEqual(0, loaded.Count);
        }
    }
}
