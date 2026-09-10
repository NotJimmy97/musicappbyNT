using System.Collections.Generic;

namespace MusicApp.Core.Dtos
{
    public class SearchResponseDto
    {
        public int Total { get; set; }
        public List<TrackDto> Items { get; set; } = new List<TrackDto>();
    }
}

