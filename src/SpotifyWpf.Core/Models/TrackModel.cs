using System;

namespace SpotifyWpf.Core.Models
{
    public class TrackModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public int DurationSeconds { get; set; }
        public string CoverImageUrl { get; set; }
        public string StreamUrl { get; set; }
        public string License { get; set; }

        public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);

        public string FormattedDuration
        {
            get
            {
                var duration = Duration;
                return duration.TotalHours >= 1
                    ? duration.ToString(@"hh\:mm\:ss")
                    : duration.ToString(@"mm\:ss");
            }
        }
    }
}
