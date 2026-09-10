namespace MusicApp.Core.Dtos
{
    public class TrackDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public int DurationSeconds { get; set; }
        public string CoverImageUrl { get; set; }
        public string StreamEndpoint { get; set; }
        public string License { get; set; }
        public string Genre { get; set; }
    }
}

