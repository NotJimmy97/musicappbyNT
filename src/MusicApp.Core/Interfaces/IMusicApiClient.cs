using System;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Dtos;

namespace MusicApp.Core.Interfaces
{
    public interface IMusicApiClient : IDisposable
    {
        Task<SearchResponseDto> SearchTracksAsync(string query, int limit, CancellationToken cancellationToken);
        Task<string> SearchTracksRawAsync(string query, int limit, CancellationToken cancellationToken);
    }
}
