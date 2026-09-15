using System;
using MusicApp.Core.Common;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    public class LyricLineViewModel : ObservableObject
    {
        public LyricLine Line { get; }

        public TimeSpan Timestamp => Line.Timestamp;
        public string Text => Line.Text;
        public int Index => Line.Index;

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public RelayCommand SeekCommand { get; }

        public LyricLineViewModel(LyricLine line, Action<TimeSpan> onSeek)
        {
            Line = line ?? throw new ArgumentNullException(nameof(line));
            SeekCommand = new RelayCommand(_ => onSeek?.Invoke(Timestamp));
        }
    }
}
