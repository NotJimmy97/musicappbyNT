using System;
using MusicApp.Core.Common;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    public class TrackItemViewModel : ObservableObject
    {
        public TrackModel Track { get; }

        public string Id => Track.Id;
        public string Title => Track.Title;
        public string Artist => Track.Artist;
        public string Album => Track.Album;
        public int DurationSeconds => Track.DurationSeconds;
        public string CoverImageUrl => Track.CoverImageUrl;
        public string StreamUrl => Track.StreamUrl;
        public string License => Track.License;
        public string Genre => Track.Genre ?? "Electronic";

        public string FormattedDuration
        {
            get
            {
                var time = TimeSpan.FromSeconds(DurationSeconds);
                return time.TotalHours >= 1 ? time.ToString(@"hh\:mm\:ss") : time.ToString(@"mm\:ss");
            }
        }

        public RelayCommand PlayCommand { get; }

        public TrackItemViewModel(TrackModel track, Action<TrackModel> onPlay)
        {
            Track = track ?? throw new ArgumentNullException(nameof(track));
            PlayCommand = new RelayCommand(_ => onPlay?.Invoke(Track));
        }
    }
}

