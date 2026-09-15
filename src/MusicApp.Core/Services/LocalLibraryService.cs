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
    public class LocalLibraryService : ILocalLibraryService
    {
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
                throw new DirectoryNotFoundException("Target directory does not exist: " + directoryPath);
            }

            return Task.Run<IReadOnlyList<TrackModel>>(() =>
            {
                var tracks = new List<TrackModel>();
                int filesScanned = 0;

                var audioFiles = EnumerateAudioFilesSafely(directoryPath, cancellationToken);
                foreach (string filePath in audioFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    filesScanned++;
                    TrackModel track = ExtractTrackFromFile(filePath);
                    if (track != null)
                    {
                        tracks.Add(track);
                    }

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
                    string title = tagFile.Tag?.Title;
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        title = Path.GetFileNameWithoutExtension(filePath);
                    }

                    string artist = tagFile.Tag?.FirstPerformer;
                    if (string.IsNullOrWhiteSpace(artist))
                    {
                        artist = tagFile.Tag?.FirstAlbumArtist;
                    }
                    if (string.IsNullOrWhiteSpace(artist))
                    {
                        artist = "Ngh\u1EC7 s\u0129 kh\u00F4ng x\u00E1c \u0111\u1ECBnh";
                    }

                    string album = tagFile.Tag?.Album;
                    if (string.IsNullOrWhiteSpace(album))
                    {
                        album = "Th\u01B0 vi\u1EC7n n\u1ED9i b\u1ED9";
                    }

                    string genre = tagFile.Tag?.FirstGenre;
                    if (string.IsNullOrWhiteSpace(genre))
                    {
                        genre = "Local Audio";
                    }

                    int duration = 0;
                    if (tagFile.Properties != null && tagFile.Properties.Duration.TotalSeconds > 0)
                    {
                        duration = (int)Math.Round(tagFile.Properties.Duration.TotalSeconds);
                    }

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

                    // Compute deterministic deterministic ID based on lowercase path hash
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
                // Fallback for files with invalid or corrupted ID3 headers: ensure user can still play the audio
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

        private IEnumerable<string> EnumerateAudioFilesSafely(string rootPath, CancellationToken cancellationToken)
        {
            var directories = new Queue<string>();
            directories.Enqueue(rootPath);

            while (directories.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string currentDir = directories.Dequeue();

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
