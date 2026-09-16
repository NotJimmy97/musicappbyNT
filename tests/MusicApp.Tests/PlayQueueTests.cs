using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Core.Interfaces;
using MusicApp.Core.Models;
using MusicApp.ViewModels;

namespace MusicApp.Tests
{
    /// <summary>
    /// Bo kiem thu don vi va kiem dinh chat luong (Gate 3 Verification) cho hang doi phat nhac (Play Queue).
    /// 
    /// - Tac dung: Kiem thu toan bo vong doi va cac thao tac tren PlayQueueViewModel: Them bai (Enqueue),
    ///   lay bai hat ke tiep theo thu tu uu tien (DequeueNext), keo tha doi vi tri (Move / Drag-and-Drop),
    ///   xoa bai (Remove), xoa sach hang doi (Clear), va phat truc tiep tu hang doi (PlayNowFromQueue).
    /// 
    /// - Van de giai quyet:
    ///   1. Kiem dinh Gate 3 (Queue Priority & Reordering): Them 3 bai hat vao hang doi, keo bai hat thu 3
    ///      len dau hang doi (index 0); xac minh DequeueNext() bat buoc phai tra ve dung bai hat thu 3 vua duoc uu tien.
    ///   2. Tinh toan thoi luong chinh xac (TotalQueueDuration): Moi khi them/xoa/don sach hang doi,
    ///      tong thoi luong va chuoi dinh dang thoi gian (FormattedQueueDuration) phai duoc cap nhat tuc thi.
    ///   3. Uu tien hang doi trong MainViewModel: Khi nhan nut Next hoac bai hat hien tai ket thuc,
    ///      MainViewModel phai uu tien lay bai hat tu PlayQueue truoc khi chuyen tiep tren danh sach mac dinh.
    /// 
    /// - Cach thuc van hanh:
    ///   Tao cac TrackModel gia lap voi thoi luong xac dinh, thuc thi cac lenh va kiem tra trang thai ObservableCollection.
    /// </summary>
    [TestClass]
    public class PlayQueueTests
    {
        private TrackModel CreateMockTrack(string id, string title, int durationSeconds)
        {
            return new TrackModel
            {
                Id = id,
                Title = title,
                Artist = "Test Artist",
                Album = "Test Album",
                DurationSeconds = durationSeconds,
                StreamUrl = "http://localhost:5245/api/v1/stream/" + id,
                Genre = "V-Pop"
            };
        }

        [TestMethod]
        public void PlayQueueViewModel_InitialState_QueueIsEmpty()
        {
            var queueVm = new PlayQueueViewModel(null);

            Assert.AreEqual(0, queueVm.QueueCount);
            Assert.AreEqual(TimeSpan.Zero, queueVm.TotalQueueDuration);
            Assert.AreEqual("00:00", queueVm.FormattedQueueDuration);
            Assert.IsNull(queueVm.DequeueNext());
        }

        [TestMethod]
        public void PlayQueueViewModel_Enqueue_AddsTracksAndRecalculatesDuration()
        {
            var queueVm = new PlayQueueViewModel(null);
            var track1 = CreateMockTrack("t1", "Track 1", 180);
            var track2 = CreateMockTrack("t2", "Track 2", 120);

            queueVm.Enqueue(track1);
            queueVm.Enqueue(track2);

            Assert.AreEqual(2, queueVm.QueueCount);
            Assert.AreEqual(TimeSpan.FromSeconds(300), queueVm.TotalQueueDuration);
            Assert.AreEqual("05:00", queueVm.FormattedQueueDuration);
            Assert.AreEqual("Track 1", queueVm.QueueTracks[0].Title);
            Assert.AreEqual("Track 2", queueVm.QueueTracks[1].Title);
        }

        [TestMethod]
        public void PlayQueueViewModel_DequeueNext_PopsFirstTrackAndUpdatesStats()
        {
            var queueVm = new PlayQueueViewModel(null);
            var track1 = CreateMockTrack("t1", "Track 1", 200);
            var track2 = CreateMockTrack("t2", "Track 2", 100);

            queueVm.Enqueue(track1);
            queueVm.Enqueue(track2);

            var popped1 = queueVm.DequeueNext();
            Assert.IsNotNull(popped1);
            Assert.AreEqual("t1", popped1.Id);
            Assert.AreEqual(1, queueVm.QueueCount);
            Assert.AreEqual(TimeSpan.FromSeconds(100), queueVm.TotalQueueDuration);

            var popped2 = queueVm.DequeueNext();
            Assert.IsNotNull(popped2);
            Assert.AreEqual("t2", popped2.Id);
            Assert.AreEqual(0, queueVm.QueueCount);

            var popped3 = queueVm.DequeueNext();
            Assert.IsNull(popped3);
        }

        [TestMethod]
        public void PlayQueueViewModel_Move_ReordersItems_Gate3Verification()
        {
            // Gate 3 requirement: Add 3 tracks, move track 3 to the top, track 3 must be played next
            TrackModel playedTrack = null;
            var queueVm = new PlayQueueViewModel(t => playedTrack = t);

            var track1 = CreateMockTrack("t1", "Bài 1", 100);
            var track2 = CreateMockTrack("t2", "Bài 2", 150);
            var track3 = CreateMockTrack("t3", "Bài 3", 200);

            queueVm.Enqueue(track1);
            queueVm.Enqueue(track2);
            queueVm.Enqueue(track3);

            Assert.AreEqual(3, queueVm.QueueCount);
            Assert.AreEqual("Bài 1", queueVm.QueueTracks[0].Title);
            Assert.AreEqual("Bài 2", queueVm.QueueTracks[1].Title);
            Assert.AreEqual("Bài 3", queueVm.QueueTracks[2].Title);

            // Reorder track 3 (index 2) to the top (index 0)
            queueVm.Move(2, 0);

            Assert.AreEqual(3, queueVm.QueueCount);
            Assert.AreEqual("Bài 3", queueVm.QueueTracks[0].Title);
            Assert.AreEqual("Bài 1", queueVm.QueueTracks[1].Title);
            Assert.AreEqual("Bài 2", queueVm.QueueTracks[2].Title);

            // Dequeue next must return Bài 3
            var nextTrack = queueVm.DequeueNext();
            Assert.IsNotNull(nextTrack);
            Assert.AreEqual("t3", nextTrack.Id);
            Assert.AreEqual("Bài 3", nextTrack.Title);
        }

        [TestMethod]
        public void PlayQueueViewModel_Remove_RemovesTargetTrackAndUpdatesStats()
        {
            var queueVm = new PlayQueueViewModel(null);
            var track1 = CreateMockTrack("t1", "Track 1", 100);
            var track2 = CreateMockTrack("t2", "Track 2", 150);

            queueVm.Enqueue(track1);
            queueVm.Enqueue(track2);

            var itemToRemove = queueVm.QueueTracks[0];
            queueVm.Remove(itemToRemove);

            Assert.AreEqual(1, queueVm.QueueCount);
            Assert.AreEqual("Track 2", queueVm.QueueTracks[0].Title);
            Assert.AreEqual(TimeSpan.FromSeconds(150), queueVm.TotalQueueDuration);
        }

        [TestMethod]
        public void PlayQueueViewModel_Clear_ClearsAllTracks()
        {
            var queueVm = new PlayQueueViewModel(null);
            queueVm.Enqueue(CreateMockTrack("t1", "Track 1", 100));
            queueVm.Enqueue(CreateMockTrack("t2", "Track 2", 150));
            queueVm.Enqueue(CreateMockTrack("t3", "Track 3", 200));

            queueVm.Clear();

            Assert.AreEqual(0, queueVm.QueueCount);
            Assert.AreEqual(TimeSpan.Zero, queueVm.TotalQueueDuration);
        }

        [TestMethod]
        public void PlayQueueViewModel_PlayNowFromQueueCommand_RemovesAndPlaysImmediately()
        {
            TrackModel playedTrack = null;
            var queueVm = new PlayQueueViewModel(t => playedTrack = t);

            var track1 = CreateMockTrack("t1", "Track 1", 100);
            var track2 = CreateMockTrack("t2", "Track 2", 200);

            queueVm.Enqueue(track1);
            queueVm.Enqueue(track2);

            // Play track 2 directly from queue
            var itemToPlay = queueVm.QueueTracks[1];
            queueVm.PlayNowFromQueueCommand.Execute(itemToPlay);

            Assert.IsNotNull(playedTrack);
            Assert.AreEqual("t2", playedTrack.Id);
            Assert.AreEqual(1, queueVm.QueueCount);
            Assert.AreEqual("Track 1", queueVm.QueueTracks[0].Title);
        }

        [TestMethod]
        public void MainViewModel_PlayNextTrack_WithQueuedTrack_PrioritizesQueueOverPlaylist()
        {
            var fakeAudio = new FakeAudioService();
            var nowPlaying = new NowPlayingViewModel(fakeAudio);
            var fakeApi = new FakeMusicApiClient();
            var mainVm = new MainViewModel(fakeApi, nowPlaying);

            var queuedTrack = CreateMockTrack("queue_vip_01", "VIP Queued Track", 180);
            mainVm.PlayQueue.Enqueue(queuedTrack);

            // Trigger PlayNextAction
            nowPlaying.NextTrackCommand.Execute(null);

            // Verify queue track was dequeued
            Assert.AreEqual(0, mainVm.PlayQueue.QueueCount);
        }

        private class FakeAudioService : IAudioService
        {
            public PlaybackState CurrentState { get; set; } = PlaybackState.Stopped;
            public TimeSpan CurrentTime { get; set; } = TimeSpan.Zero;
            public TimeSpan TotalTime { get; set; } = TimeSpan.Zero;
            public float Volume { get; set; } = 1.0f;
            public IDspEqualizerService Equalizer { get; set; } = new FakeDspEqualizerService();

#pragma warning disable 0067
            public event EventHandler<float[]> SpectrumDataReady;
#pragma warning restore 0067
            public event EventHandler<PlaybackState> StateChanged;

            public Task InitializeAsync(string streamUrl) => Task.FromResult(0);
            public void Play() { CurrentState = PlaybackState.Playing; StateChanged?.Invoke(this, CurrentState); }
            public void Pause() { CurrentState = PlaybackState.Paused; StateChanged?.Invoke(this, CurrentState); }
            public void Stop() { CurrentState = PlaybackState.Stopped; StateChanged?.Invoke(this, CurrentState); }
            public void Seek(TimeSpan position) { CurrentTime = position; }
            public void SetVolume(float volume) { Volume = volume; }
            public void Dispose() { }
        }

        private class FakeDspEqualizerService : IDspEqualizerService
        {
            public bool IsEnabled { get; set; } = true;
            public float[] BandFrequencies { get; set; } = new float[] { 32f, 64f, 125f, 250f, 500f, 1000f, 2000f, 4000f, 8000f, 16000f };
            public float[] BandGains { get; set; } = new float[10];
            public event EventHandler EqualizerChanged;

            public void SetBandGain(int bandIndex, float gainDb)
            {
                if (bandIndex >= 0 && bandIndex < BandGains.Length)
                {
                    BandGains[bandIndex] = gainDb;
                    EqualizerChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            public void SetAllBands(float[] gains)
            {
                if (gains != null)
                {
                    for (int i = 0; i < Math.Min(BandGains.Length, gains.Length); i++)
                    {
                        BandGains[i] = gains[i];
                    }
                    EqualizerChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private class FakeMusicApiClient : IMusicApiClient
        {
            public Task<Core.Dtos.SearchResponseDto> SearchTracksAsync(string query, int limit = 20, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                return Task.FromResult(new Core.Dtos.SearchResponseDto
                {
                    Total = 0,
                    Items = new System.Collections.Generic.List<Core.Dtos.TrackDto>()
                });
            }

            public Task<string> SearchTracksRawAsync(string query, int limit, System.Threading.CancellationToken cancellationToken)
            {
                return Task.FromResult("{\"total\":0,\"items\":[]}");
            }

            public void Dispose()
            {
            }
        }
    }
}
