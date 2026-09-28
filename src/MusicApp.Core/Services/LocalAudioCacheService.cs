using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Core.Interfaces.Persistence;
using MusicApp.Core.Persistence;

namespace MusicApp.Core.Services
{
    /// <summary>
    /// Dịch vụ quản lý bộ nhớ đệm luồng âm thanh trực tuyến.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Ghi/đọc/xóa file âm thanh trên đĩa vật lý.
    /// KHÔNG chịu trách nhiệm: Truy xuất database (giao lại cho StreamCacheRepository).
    /// Vòng đời: Singleton/Scoped, tái sử dụng directory path.
    /// Luồng: Thực hiện I/O bất đồng bộ. Phải đảm bảo an toàn ghi đè.
    /// </remarks>
    public class LocalAudioCacheService
    {
        private readonly IStreamCacheRepository _cacheRepo;
        private readonly string _cacheDirectory;
        private const long MaxCacheSizeBytes = 1024L * 1024L * 1024L; // 1 GB cache limit

        public LocalAudioCacheService(IStreamCacheRepository cacheRepo, string cacheDirectory = null)
        {
            _cacheRepo = cacheRepo ?? throw new ArgumentNullException(nameof(cacheRepo));
            _cacheDirectory = cacheDirectory ?? DatabaseInitializer.AudioCacheDirectory;

            if (!Directory.Exists(_cacheDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_cacheDirectory);
                }
                catch { }
            }
        }

        public string ComputeTrackHash(string streamUrlOrId)
        {
            if (string.IsNullOrWhiteSpace(streamUrlOrId)) return null;

            using (var sha1 = SHA1.Create())
            {
                byte[] bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(streamUrlOrId.Trim()));
                var sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public async Task<string> TryGetCachedAudioFileAsync(string trackHash)
        {
            if (string.IsNullOrWhiteSpace(trackHash)) return null;

            string path = await _cacheRepo.GetCachedFilePathAsync(trackHash).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                await _cacheRepo.TouchCacheAccessAsync(trackHash).ConfigureAwait(false);
                return path;
            }

            return null;
        }

        public string GetTargetCacheFilePath(string trackHash)
        {
            return Path.Combine(_cacheDirectory, $"{trackHash}.audio");
        }

        public async Task SaveStreamToCacheAsync(string trackHash, Stream sourceStream)
        {
            if (string.IsNullOrWhiteSpace(trackHash) || sourceStream == null) return;

            string targetPath = GetTargetCacheFilePath(trackHash);
            string tempPath = targetPath + ".tmp";

            await Task.Run(async () =>
            {
                try
                {
                    using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                    {
                        await sourceStream.CopyToAsync(fs).ConfigureAwait(false);
                    }

                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                    }
                    File.Move(tempPath, targetPath);

                    var fileInfo = new FileInfo(targetPath);
                    await _cacheRepo.RegisterCacheFileAsync(trackHash, targetPath, fileInfo.Length, true).ConfigureAwait(false);

                    // Kiem tra neu tong cache vuot 1GB thi xoa theo LRU
                    long totalSize = await _cacheRepo.GetTotalCacheSizeAsync().ConfigureAwait(false);
                    if (totalSize > MaxCacheSizeBytes)
                    {
                        await _cacheRepo.EvictOldestCacheAsync(totalSize - MaxCacheSizeBytes).ConfigureAwait(false);
                    }
                }
                catch
                {
                    if (File.Exists(tempPath))
                    {
                        try { File.Delete(tempPath); } catch { }
                    }
                }
            }).ConfigureAwait(false);
        }
    }
}
