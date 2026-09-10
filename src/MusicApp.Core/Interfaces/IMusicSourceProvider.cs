using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Dtos;

namespace MusicApp.Core.Interfaces
{
    public interface IMusicSourceProvider
    {
        string ProviderName { get; }
        Task<List<TrackDto>> SearchTracksAsync(string query, int limit, CancellationToken cancellationToken);
        Task<Stream> GetAudioStreamAsync(string trackId, long? startByte, long? endByte, CancellationToken cancellationToken);
    }
}

