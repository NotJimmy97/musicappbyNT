using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces
{
    public class ScanProgressReport
    {
        public int FilesScanned { get; set; }
        public int TracksFound { get; set; }
        public string CurrentFile { get; set; }
    }

    public interface ILocalLibraryService
    {
        Task<IReadOnlyList<TrackModel>> ScanDirectoryAsync(
            string directoryPath, 
            IProgress<ScanProgressReport> progress = null, 
            CancellationToken cancellationToken = default(CancellationToken));

        TrackModel ExtractTrackFromFile(string filePath);
    }
}
