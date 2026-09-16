using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Interfaces;
using MusicApp.Core.Models;

namespace MusicApp.Core.Services
{
    /// <summary>
    /// Dich vu quet va doc sieu du lieu thu vien am thanh cuc bo (Local Audio Library Scanner).
    /// 
    /// Tac dung:
    /// - Quet toan bo tap tin am thanh trong thu muc chi dinh tren may tinh nguoi dung.
    /// - Doc the ID3 metadata (Title, Artist, Album, Genre, Duration, Album Art) bang thu vien TagLibSharp.
    /// - Tao doi tuong TrackModel hoan chinh phuc vu danh sach phat va giao dien.
    /// 
    /// Van de giai quyet:
    /// - Tranh gay treo ung dung (UI Freeze): Toan bo qua trinh quet I/O va phan tich nhi phan deu chay tren Task.Run luong nen.
    /// - Xu ly ngoai le an toan tuyet doi: Thuat toan duyet thu muc theo chieu rong (BFS Queue) bat giu va bo qua cac loi
    ///   nhu khong co quyen truy cap (UnauthorizedAccessException), thu muc he thong bao ve (SecurityException)
    ///   hoac duong dan vuot qua gioi han 260 ky tu (PathTooLongException) ma khong lam dung chuong trinh.
    /// - Ho tro co che huy tac vu (CancellationToken) va bao cao tien do thoi gian thuc (IProgress) len UI.
    /// - Chuyen doi anh bia ID3 thanh chuoi Base64 Data URI de WPF Image Control co the bind truc tiep ma khong can ghi file ra dia.
    /// 
    /// Cach thuc van hanh:
    /// - EnumerateAudioFilesSafely duyet cay thu muc bang Queue, yield return tung duong dan file thoa man phan mo rong am thanh.
    /// - ExtractTrackFromFile tao ID deterministic dua tren ma bam MD5/HashCode cua duong dan de tranh trung lap.
    /// </summary>
    public class LocalLibraryService : ILocalLibraryService
    {
        // Danh sach cac phan mo rong am thanh duoc ho tro boi TagLib va NAudio
        private static readonly HashSet<string> SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3",
            ".flac",
            ".m4a",
            ".aac",
            ".wav",
            ".wma",
            ".ogg"
        };

        /// <summary>
        /// Quet thu muc bat dong bo tren luong Worker Thread, lien tuc phat bao cao tien do qua progress.
        /// </summary>
        /// <param name="directoryPath">Duong dan thu muc tren o cung can quet.</param>
        /// <param name="progress">Kenh bao cao tien do len giao dien.</param>
        /// <param name="cancellationToken">Token huy tac vu quet khi nguoi dung dung thao tac.</param>
        /// <returns>Danh sach cac doi tuong TrackModel duoc quet thanh cong.</returns>
        public Task<IReadOnlyList<TrackModel>> ScanDirectoryAsync(
            string directoryPath, 
            IProgress<ScanProgressReport> progress = null, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException("Thu muc chi dinh khong ton tai: " + directoryPath);
            }

            return Task.Run<IReadOnlyList<TrackModel>>(() =>
            {
                var tracks = new List<TrackModel>();
                int filesScanned = 0;

                // Duyet an toan qua tung tap tin am thanh
                var audioFiles = EnumerateAudioFilesSafely(directoryPath, cancellationToken);
                foreach (string filePath in audioFiles)
                {
                    // Kiem tra tin hieu huy tu nguoi dung truoc khi doc file tiep theo
                    cancellationToken.ThrowIfCancellationRequested();

                    filesScanned++;
                    TrackModel track = ExtractTrackFromFile(filePath);
                    if (track != null)
                    {
                        tracks.Add(track);
                    }

                    // Phat tin hieu cap nhat tien do len UI thread
                    progress?.Report(new ScanProgressReport
                    {
                        FilesScanned = filesScanned,
                        TracksFound = tracks.Count,
                        CurrentFile = Path.GetFileName(filePath)
                    });
                }

                return tracks;
            }, cancellationToken);
        }

        /// <summary>
        /// Doc sieu du lieu ID3 tu file bang TagLibSharp va khoi tao TrackModel.
        /// Neu file loi hoac khong co tag, se tu dong ap dung gia tri mac dinh tu ten tap tin de dam bao luon phat duoc.
        /// </summary>
        /// <param name="filePath">Duong dan day du cua tap tin am thanh tren o cung.</param>
        /// <returns>TrackModel hop le hoac null neu phan mo rong khong phu hop.</returns>
        public TrackModel ExtractTrackFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            string extension = Path.GetExtension(filePath);
            if (!SupportedExtensions.Contains(extension))
            {
                return null;
            }

            try
            {
                using (var tagFile = TagLib.File.Create(filePath))
                {
                    // Trich xuat tieu de: neu the trong thi lay ten tap tin lam tieu de
                    string title = tagFile.Tag?.Title;
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        title = Path.GetFileNameWithoutExtension(filePath);
                    }

                    // Trich xuat nghe si
                    string artist = tagFile.Tag?.FirstPerformer;
                    if (string.IsNullOrWhiteSpace(artist))
                    {
                        artist = tagFile.Tag?.FirstAlbumArtist;
                    }
                    if (string.IsNullOrWhiteSpace(artist))
                    {
                        artist = "Ngh\u1EC7 s\u0129 kh\u00F4ng x\u00E1c \u0111\u1ECBnh";
                    }

                    // Trich xuat album
                    string album = tagFile.Tag?.Album;
                    if (string.IsNullOrWhiteSpace(album))
                    {
                        album = "Th\u01B0 vi\u1EC7n n\u1ED9i b\u1ED9";
                    }

                    // Trich xuat the loai
                    string genre = tagFile.Tag?.FirstGenre;
                    if (string.IsNullOrWhiteSpace(genre))
                    {
                        genre = "Local Audio";
                    }

                    // Tinh thoi luong theo giay
                    int duration = 0;
                    if (tagFile.Properties != null && tagFile.Properties.Duration.TotalSeconds > 0)
                    {
                        duration = (int)Math.Round(tagFile.Properties.Duration.TotalSeconds);
                    }

                    // Trich xuat anh bia nhung (Embedded Picture) thanh Base64 Data URI
                    string coverImageUrl = null;
                    if (tagFile.Tag?.Pictures != null && tagFile.Tag.Pictures.Length > 0)
                    {
                        var pic = tagFile.Tag.Pictures[0];
                        if (pic.Data != null && pic.Data.Data != null && pic.Data.Data.Length > 0)
                        {
                            string mime = string.IsNullOrWhiteSpace(pic.MimeType) ? "image/jpeg" : pic.MimeType;
                            coverImageUrl = "data:" + mime + ";base64," + Convert.ToBase64String(pic.Data.Data);
                        }
                    }

                    // Tao ID dinh danh dua tren ma bam hash cua duong dan (chuan hoa chu thuong)
                    string trackId = "local_" + Math.Abs(filePath.ToLowerInvariant().GetHashCode()).ToString("X8");

                    return new TrackModel
                    {
                        Id = trackId,
                        Title = title,
                        Artist = artist,
                        Album = album,
                        Genre = genre,
                        DurationSeconds = duration,
                        CoverImageUrl = coverImageUrl,
                        StreamUrl = filePath,
                        License = "Offline Local Media"
                    };
                }
            }
            catch (Exception)
            {
                // Co che du phong (Fallback): Neu file bi loi cau truc header ID3, van cho phep dua vao danh sach de phat
                return new TrackModel
                {
                    Id = "local_" + Math.Abs(filePath.ToLowerInvariant().GetHashCode()).ToString("X8"),
                    Title = Path.GetFileNameWithoutExtension(filePath),
                    Artist = "Ngh\u1EC7 s\u0129 kh\u00F4ng x\u00E1c \u0111\u1ECBnh",
                    Album = "Th\u01B0 vi\u1EC7n n\u1ED9i b\u1ED9",
                    Genre = "Local Audio",
                    DurationSeconds = 0,
                    CoverImageUrl = null,
                    StreamUrl = filePath,
                    License = "Offline Local Media"
                };
            }
        }

        /// <summary>
        /// Thuat toan duyet cay thu muc theo chieu rong (BFS Queue) dam bao an toan tuyet doi truoc cac ngoai le I/O.
        /// Su dung yield return de tiep nhan danh sach file theo dang Stream ma khong can nap toan bo vao bo nho RAM.
        /// </summary>
        private IEnumerable<string> EnumerateAudioFilesSafely(string rootPath, CancellationToken cancellationToken)
        {
            var directories = new Queue<string>();
            directories.Enqueue(rootPath);

            while (directories.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string currentDir = directories.Dequeue();

                // 1. Duyet va them cac thu muc con vao hang doi (Queue)
                string[] subDirs = null;
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException) { }
                catch (SecurityException) { }
                catch (DirectoryNotFoundException) { }
                catch (PathTooLongException) { }
                catch (IOException) { }

                if (subDirs != null)
                {
                    for (int i = 0; i < subDirs.Length; i++)
                    {
                        directories.Enqueue(subDirs[i]);
                    }
                }

                // 2. Duyet cac tap tin trong thu muc hien tai
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException) { }
                catch (SecurityException) { }
                catch (DirectoryNotFoundException) { }
                catch (PathTooLongException) { }
                catch (IOException) { }

                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        string file = files[i];
                        string ext = Path.GetExtension(file);
                        if (ext != null && SupportedExtensions.Contains(ext))
                        {
                            yield return file;
                        }
                    }
                }
            }
        }
    }
}
