using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces
{
    public interface ILyricsService
    {
        IReadOnlyList<LyricLine> ParseLrc(string lrcContent);
        Task<IReadOnlyList<LyricLine>> LoadLyricsForTrackAsync(TrackModel track, CancellationToken cancellationToken = default(CancellationToken));
    }
}

