using System;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces
{
    public interface IAudioService : IDisposable
    {
        PlaybackState CurrentState { get; }
        TimeSpan CurrentTime { get; }
        TimeSpan TotalTime { get; }
        float Volume { get; }

        IDspEqualizerService Equalizer { get; }

        event EventHandler<float[]> SpectrumDataReady;
        event EventHandler<PlaybackState> StateChanged;

        Task InitializeAsync(string streamUrl);
        void Play();
        void Pause();
        void Stop();
        void Seek(TimeSpan position);
        void SetVolume(float volume);
    }
}

