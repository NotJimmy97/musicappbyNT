using System;
using System.Windows.Input;
using SpotifyWpf.Core.Common;
using SpotifyWpf.Core.Models;

namespace SpotifyWpf.Client.ViewModels
{
    public class TrackItemViewModel : ObservableObject
    {
        public TrackModel Model { get; }
        public ICommand PlayCommand { get; }

        public string Id => Model.Id;
        public string Title => Model.Title;
        public string Artist => Model.Artist;
        public string Album => Model.Album;
        public int DurationSeconds => Model.DurationSeconds;
        public string FormattedDuration => Model.FormattedDuration;
        public string CoverImageUrl => Model.CoverImageUrl;
        public string StreamUrl => Model.StreamUrl;
        public string License => Model.License;

        public TrackItemViewModel(TrackModel model, Action<TrackModel> onPlay)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            PlayCommand = new RelayCommand(_ => onPlay?.Invoke(Model));
        }
    }
}
