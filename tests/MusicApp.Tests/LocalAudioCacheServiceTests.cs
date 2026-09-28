using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Core.Persistence;
using MusicApp.Core.Persistence.Repositories;
using MusicApp.Core.Services;

namespace MusicApp.Tests
{
    [TestClass]
    public class LocalAudioCacheServiceTests
    {
        private string _tempDbFile;
        private string _tempCacheDir;
        private StreamCacheRepository _cacheRepo;
        private LocalAudioCacheService _cacheService;

        [TestInitialize]
        public void Setup()
        {
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"cache_test_db_{Guid.NewGuid():N}.db");
            _tempCacheDir = Path.Combine(Path.GetTempPath(), $"cache_test_dir_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempCacheDir);

            string connStr = $"Data Source={_tempDbFile};Version=3;Journal Mode=WAL;Synchronous=NORMAL;Cache Size=-64000;Foreign Keys=True;Default Timeout=5;";
            DatabaseInitializer.SetCustomConnectionString(connStr);
            DatabaseInitializer.Initialize();

            _cacheRepo = new StreamCacheRepository(connStr);
            _cacheService = new LocalAudioCacheService(_cacheRepo, _tempCacheDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DatabaseInitializer.ResetInitialization();
            if (Directory.Exists(_tempCacheDir))
            {
                try { Directory.Delete(_tempCacheDir, true); } catch { }
            }
            if (File.Exists(_tempDbFile))
            {
                try { File.Delete(_tempDbFile); } catch { }
            }
        }

        [TestMethod]
        public void ComputeTrackHash_ReturnsDeterministicSha1()
        {
            // Act
            string hash1 = _cacheService.ComputeTrackHash("track_123");
            string hash2 = _cacheService.ComputeTrackHash("track_123");
            string hash3 = _cacheService.ComputeTrackHash("track_456");

            // Assert
            Assert.IsNotNull(hash1);
            Assert.AreEqual(40, hash1.Length);
            Assert.AreEqual(hash1, hash2);
            Assert.AreNotEqual(hash1, hash3);
        }

        [TestMethod]
        public async Task SaveStreamToCacheAsync_SavesFileAndRegistersInDb()
        {
            // Arrange
            string trackId = "sample_stream_01";
            string hash = _cacheService.ComputeTrackHash(trackId);
            byte[] dummyData = Encoding.UTF8.GetBytes("MOCK_AUDIO_STREAM_BINARY_DATA_FOR_TESTING");

            // Act
            using (var ms = new MemoryStream(dummyData))
            {
                await _cacheService.SaveStreamToCacheAsync(hash, ms);
            }

            string cachedPath = await _cacheService.TryGetCachedAudioFileAsync(hash);

            // Assert
            Assert.IsNotNull(cachedPath, "Cached path should be retrieved from repository.");
            Assert.IsTrue(File.Exists(cachedPath), "Cached audio file must exist on disk.");
            byte[] readBytes = File.ReadAllBytes(cachedPath);
            Assert.AreEqual(dummyData.Length, readBytes.Length);
        }
    }
}
