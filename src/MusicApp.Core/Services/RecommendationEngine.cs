using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusicApp.Core.Interfaces.Persistence;
using MusicApp.Core.Models;

namespace MusicApp.Core.Services
{
    /// <summary>
    /// Dong co goi y bai hat thong minh chay hoan toan cuc bo (Client-Side Affinity Recommendation Engine).
    /// Hoc gu nguoi dung tu hanh vi Click, Play, Skip, Heart va thuc thi thuat toan Smart Shuffle Boltzmann.
    /// </summary>
    public class RecommendationEngine
    {
        private readonly ITrackRepository _trackRepo;
        private readonly IInteractionRepository _interactionRepo;
        private readonly object _randomLock = new object();
        private readonly Random _random = new Random();

        public RecommendationEngine(ITrackRepository trackRepo, IInteractionRepository interactionRepo)
        {
            _trackRepo = trackRepo ?? throw new ArgumentNullException(nameof(trackRepo));
            _interactionRepo = interactionRepo ?? throw new ArgumentNullException(nameof(interactionRepo));
        }

        /// <summary>
        /// Ghi nhan hanh vi tuong tac va cap nhat diem so quan he AffinityScore cho ban nhac.
        /// </summary>
        public async Task LogActionAsync(int trackId, string actionType, int durationPlayedSeconds = 0)
        {
            if (trackId <= 0 || string.IsNullOrWhiteSpace(actionType)) return;

            double delta = 0.0;
            switch (actionType.ToLowerInvariant())
            {
                case "favorite":
                case "heart":
                    delta = 10.0;
                    break;
                case "unfavorite":
                case "unheart":
                    delta = -10.0;
                    break;
                case "add_playlist":
                    delta = 8.0;
                    break;
                case "play_complete":
                    delta = 5.0;
                    await _trackRepo.IncrementPlayCountAsync(trackId).ConfigureAwait(false);
                    break;
                case "play_30s":
                    delta = 2.0;
                    break;
                case "click":
                    delta = 1.5;
                    break;
                case "skip":
                    delta = -4.0;
                    await _trackRepo.IncrementSkipCountAsync(trackId).ConfigureAwait(false);
                    break;
                default:
                    delta = 0.5;
                    break;
            }

            await _interactionRepo.LogInteractionAsync(trackId, actionType, durationPlayedSeconds).ConfigureAwait(false);
            if (Math.Abs(delta) > 0.001)
            {
                await _trackRepo.UpdateAffinityScoreAsync(trackId, delta).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Lay danh sach cac bai hat duoc de xuat cao nhat danh cho nguoi dung.
        /// </summary>
        public async Task<IEnumerable<TrackEntity>> GetRecommendationsAsync(int limit = 25)
        {
            return await _trackRepo.GetRecommendationsAsync(limit).ConfigureAwait(false);
        }

        /// <summary>
        /// Thuat toan Smart Shuffle lua chon bai hat tiep theo dua tren phan phoi xac suat Boltzmann (Softmax Sampling).
        /// Uu tien cac bai co AffinityScore cao nhung van giu xac suat cho cac bai khac de tao su kham pha.
        /// </summary>
        public TrackEntity PickSmartShuffleNext(IList<TrackEntity> pool, int currentTrackId)
        {
            if (pool == null || pool.Count == 0) return null;
            if (pool.Count == 1) return pool[0];

            // Loc bo bai dang phat neu co nhieu hon 1 bai
            var candidates = pool.Where(t => t.Id != currentTrackId).ToList();
            if (candidates.Count == 0) candidates = pool.ToList();

            // Nhiet do Boltzmann (Temperature tau): cang nho thi cang thien vi bai diem cao, cang lon thi cang ngau nhien
            const double tau = 5.0;

            // Tinh toan diem tho va tim maxScore de on dinh so hoc (Max-subtraction Softmax Invariant)
            double[] rawScores = new double[candidates.Count];
            double maxScore = double.MinValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                double score = Math.Max(0.0, candidates[i].AffinityScore);
                if (candidates[i].IsFavorite) score += 5.0;

                // Neu bài chưa từng nghe (play_count = 0), bonus 2.0 để ưu tiên khám phá
                if (candidates[i].PlayCount == 0) score += 2.0;

                rawScores[i] = score;
                if (score > maxScore) maxScore = score;
            }

            // Tinh tong e^((Score - maxScore) / tau) - Triet tieu hoan toan tran so duong Infinity / NaN
            double sumExp = 0.0;
            double[] expScores = new double[candidates.Count];

            for (int i = 0; i < candidates.Count; i++)
            {
                double expVal = Math.Exp((rawScores[i] - maxScore) / tau);
                expScores[i] = expVal;
                sumExp += expVal;
            }

            if (sumExp <= 0.0 || double.IsNaN(sumExp) || double.IsInfinity(sumExp))
            {
                lock (_randomLock)
                {
                    return candidates[_random.Next(candidates.Count)];
                }
            }

            // Bốc thăm ngẫu nhiên theo trọng số phân phối xác suất tích lũy (Cumulative Probability Distribution)
            double target;
            lock (_randomLock)
            {
                target = _random.NextDouble() * sumExp;
            }
            double cumulative = 0.0;

            for (int i = 0; i < candidates.Count; i++)
            {
                cumulative += expScores[i];
                if (cumulative >= target)
                {
                    return candidates[i];
                }
            }

            return candidates[candidates.Count - 1];
        }
    }
}
