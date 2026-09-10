using System.Collections.Generic;

namespace SpotifyWpf.Core.Dtos
{
    public class SearchResponseDto
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<TrackDto> Items { get; set; }

        public SearchResponseDto()
        {
            Items = new List<TrackDto>();
        }
    }
}
