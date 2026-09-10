using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Dtos;
using MusicApp.Core.Interfaces;

namespace MusicApp.Bff.Providers
{
    public class MusicSourceRouter
    {
        public JamendoSourceProvider Jamendo { get; }
        public VietnameseMusicSourceProvider Vietnamese { get; }

        public MusicSourceRouter()
        {
            Jamendo = new JamendoSourceProvider();
            Vietnamese = new VietnameseMusicSourceProvider();
        }

        public async Task<List<TrackDto>> SearchAsync(string query, int limit, CancellationToken cancellationToken)
        {
            int safeLimit = limit > 0 ? limit : 20;

            if (string.IsNullOrWhiteSpace(query))
            {
                var combined = new List<TrackDto>();
                combined.AddRange(Vietnamese.GetAllTracks());
                combined.AddRange(Jamendo.GetCuratedTracksMatching(string.Empty));
                return combined.Take(safeLimit).ToList();
            }

            string q = query.Trim().ToLowerInvariant();
            string qNorm = VietnameseMusicSourceProvider.RemoveDiacritics(q);

            bool isVietnameseIntent =
                qNorm.Contains("viet") ||
                qNorm.Contains("vpop") ||
                qNorm.Contains("v-pop") ||
                qNorm.Contains("trinh") ||
                qNorm.Contains("son tung") ||
                qNorm.Contains("den vau") ||
                qNorm.Contains("acoustic") ||
                qNorm.Contains("guitar") ||
                qNorm.Contains("que huong") ||
                qNorm.Contains("diem xua") ||
                qNorm.Contains("ha trang");

            if (isVietnameseIntent)
            {
                var vnTracks = await Vietnamese.SearchTracksAsync(query, safeLimit, cancellationToken).ConfigureAwait(false);
                if (vnTracks.Count >= safeLimit)
                {
                    return vnTracks;
                }

                int remaining = safeLimit - vnTracks.Count;
                var internationalTracks = await Jamendo.SearchTracksAsync(query, remaining, cancellationToken).ConfigureAwait(false);

                var merged = new List<TrackDto>(vnTracks);
                merged.AddRange(internationalTracks);
                return merged;
            }
            else
            {
                // Run parallel search across both providers and merge
                var vnTask = Vietnamese.SearchTracksAsync(query, safeLimit, cancellationToken);
                var jamendoTask = Jamendo.SearchTracksAsync(query, safeLimit, cancellationToken);

                await Task.WhenAll(vnTask, jamendoTask).ConfigureAwait(false);

                var vnResults = vnTask.Result ?? new List<TrackDto>();
                var jamendoResults = jamendoTask.Result ?? new List<TrackDto>();

                var merged = new List<TrackDto>();
                merged.AddRange(vnResults);
                merged.AddRange(jamendoResults);

                // Deduplicate by Id if needed
                return merged.GroupBy(t => t.Id).Select(g => g.First()).Take(safeLimit).ToList();
            }
        }

        public string ResolveAudioUrl(string trackId)
        {
            if (string.IsNullOrWhiteSpace(trackId))
            {
                return string.Empty;
            }

            if (trackId.StartsWith("vn_track_"))
            {
                return Vietnamese.ResolveTrackAudioUrl(trackId);
            }

            return Jamendo.ResolveTrackAudioUrl(trackId);
        }
    }
}
