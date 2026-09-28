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
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"stream_cache_test_{Guid.NewGuid():N}.db");
            _tempCacheDir = Path.Combine(Path.GetTempPath(), $"audio_cache_test_{Guid.NewGuid():N}");
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
                try
                {
                    Directory.Delete(_tempCacheDir, true);
                }
                catch { }
            }

            if (File.Exists(_tempDbFile))
            {
                try
                {
                    File.Delete(_tempDbFile);
                }
                catch { }
            }
        }

        [TestMethod]
        public void ComputeTrackHash_ProducesConsistentSha1Hash()
        {
            // Arrange
            string trackId = "jamendo:123456";

            // Act
            string hash1 = _cacheService.ComputeTrackHash(trackId);
            string hash2 = _cacheService.ComputeTrackHash(trackId);

            // Assert
            Assert.IsNotNull(hash1);
            Assert.AreEqual(40, hash1.Length); // SHA1 hex string length
            Assert.AreEqual(hash1, hash2);
        }

        [TestMethod]
        public async Task SaveStreamToCacheAsync_And_TryGetCachedAudioFileAsync_SavesAndRetrievesFile()
        {
            // Arrange
            string trackId = "vietnamese:song_001";
            string hash = _cacheService.ComputeTrackHash(trackId);
            byte[] dummyMp3Data = Encoding.UTF8.GetBytes("ID3_DUMMY_MP3_AUDIO_STREAM_BINARY_DATA_FOR_TESTING");

            // Act - Lưu stream vào cache
            using (var ms = new MemoryStream(dummyMp3Data))
            {
                await _cacheService.SaveStreamToCacheAsync(hash, ms);
            }

            // Act - Lấy đường dẫn file cache
            string cachedPath = await _cacheService.TryGetCachedAudioFileAsync(hash);

            // Assert
            Assert.IsNotNull(cachedPath);
            Assert.IsTrue(File.Exists(cachedPath));
            byte[] readBytes = File.ReadAllBytes(cachedPath);
            CollectionAssert.AreEqual(dummyMp3Data, readBytes);

            // Kiểm tra tổng dung lượng cache trong DB
            long totalSize = await _cacheRepo.GetTotalCacheSizeAsync();
            Assert.AreEqual(dummyMp3Data.Length, totalSize);
        }

        [TestMethod]
        public async Task EvictOldestCacheAsync_RemovesOldestCachedFiles()
        {
            // Arrange
            string hash1 = _cacheService.ComputeTrackHash("track_1");
            string hash2 = _cacheService.ComputeTrackHash("track_2");
            byte[] data1 = new byte[1024];
            byte[] data2 = new byte[2048];

            using (var ms1 = new MemoryStream(data1))
            {
                await _cacheService.SaveStreamToCacheAsync(hash1, ms1);
            }

            await Task.Delay(50); // Đảm bảo khác biệt timestamp access

            using (var ms2 = new MemoryStream(data2))
            {
                await _cacheService.SaveStreamToCacheAsync(hash2, ms2);
            }

            // Act - Xóa ít nhất 500 byte (sẽ evict track_1 vì cũ nhất)
            await _cacheRepo.EvictOldestCacheAsync(500);

            // Assert
            string path1 = await _cacheService.TryGetCachedAudioFileAsync(hash1);
            string path2 = await _cacheService.TryGetCachedAudioFileAsync(hash2);

            Assert.IsNull(path1, "Track 1 cũ nhất nên đã bị evict khỏi cache.");
            Assert.IsNotNull(path2, "Track 2 mới hơn nên vẫn còn trong cache.");
        }
    }
}
