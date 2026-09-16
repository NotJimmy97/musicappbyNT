using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Core.Models;
using MusicApp.Core.Services;
using MusicApp.ViewModels;

namespace MusicApp.Tests
{
    [TestClass]
    public class LyricsTests
    {
        private string _testDirectory;

        [TestInitialize]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "MusicApp_Lyrics_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch
            {
            }
        }

        [TestMethod]
        public void LrcParser_NullOrEmptyContent_ReturnsEmptyList()
        {
            var parser = new LrcParser();
            Assert.AreEqual(0, parser.Parse(null).Count);
            Assert.AreEqual(0, parser.Parse("").Count);
            Assert.AreEqual(0, parser.Parse("   \r\n  ").Count);
        }

        [TestMethod]
        public void LrcParser_StandardTimestamps_ParsesLinesChronologically()
        {
            var parser = new LrcParser();
            string lrc = @"
[ti:Diễm Xưa]
[ar:Trịnh Công Sơn]
[00:15.50]Mưa vẫn mưa bay trên tầng tháp cổ
[00:05.00]Giai điệu mở đầu
[00:25.00]Dài tay em mấy thuở mắt xanh xao";

            var lines = parser.Parse(lrc);

            Assert.AreEqual(3, lines.Count);
            // Verify chronological sort
            Assert.AreEqual("Giai điệu mở đầu", lines[0].Text);
            Assert.AreEqual(TimeSpan.FromSeconds(5), lines[0].Timestamp);
            Assert.AreEqual(0, lines[0].Index);

            Assert.AreEqual("Mưa vẫn mưa bay trên tầng tháp cổ", lines[1].Text);
            Assert.AreEqual(TimeSpan.FromMilliseconds(15500), lines[1].Timestamp);
            Assert.AreEqual(1, lines[1].Index);

            Assert.AreEqual("Dài tay em mấy thuở mắt xanh xao", lines[2].Text);
            Assert.AreEqual(TimeSpan.FromSeconds(25), lines[2].Timestamp);
            Assert.AreEqual(2, lines[2].Index);
        }

        [TestMethod]
        public void LrcParser_MultiTimestampLines_ExpandsAndOrders()
        {
            var parser = new LrcParser();
            string lrc = @"[00:10.00][00:30.00]Điệp khúc lặp lại";

            var lines = parser.Parse(lrc);

            Assert.AreEqual(2, lines.Count);
            Assert.AreEqual(TimeSpan.FromSeconds(10), lines[0].Timestamp);
            Assert.AreEqual("Điệp khúc lặp lại", lines[0].Text);
            Assert.AreEqual(TimeSpan.FromSeconds(30), lines[1].Timestamp);
            Assert.AreEqual("Điệp khúc lặp lại", lines[1].Text);
        }

        [TestMethod]
        public void LrcParser_OffsetTag_AdjustsTimestamps()
        {
            var parser = new LrcParser();
            string lrc = @"[offset:500]
[00:10.00]Câu hát có offset";

            var lines = parser.Parse(lrc);

            Assert.AreEqual(1, lines.Count);
            Assert.AreEqual(TimeSpan.FromMilliseconds(10500), lines[0].Timestamp);
        }

        [TestMethod]
        public async Task LyricsService_LoadLyricsForTrackAsync_CuratedTrack_ReturnsLyrics()
        {
            var service = new LyricsService();
            var track = new TrackModel
            {
                Id = "vn_track_01",
                Title = "Diễm Xưa"
            };

            var lyrics = await service.LoadLyricsForTrackAsync(track);

            Assert.IsTrue(lyrics.Count > 5);
            Assert.IsTrue(lyrics.Any(l => l.Text.Contains("Mưa vẫn mưa bay")));
        }

        [TestMethod]
        public async Task LyricsService_LoadLyricsForTrackAsync_DiskCompanionLrc_LoadsSuccessfully()
        {
            var service = new LyricsService();
            string audioPath = Path.Combine(_testDirectory, "MySong.mp3");
            File.WriteAllText(audioPath, "fake mp3 data");

            string lrcPath = Path.Combine(_testDirectory, "MySong.lrc");
            File.WriteAllText(lrcPath, "[00:02.00]Line from disk companion\r\n[00:08.00]Second disk line");

            var track = new TrackModel
            {
                Id = "local_test_1",
                Title = "MySong",
                StreamUrl = audioPath
            };

            var lyrics = await service.LoadLyricsForTrackAsync(track);

            Assert.AreEqual(2, lyrics.Count);
            Assert.AreEqual("Line from disk companion", lyrics[0].Text);
            Assert.AreEqual("Second disk line", lyrics[1].Text);
        }

        [TestMethod]
        public void LyricsViewModel_UpdatePosition_GatingAndActiveLineTransitions_Gate4Verification()
        {
            // Gate 4: Nạp 1 file .lrc mẫu, phát nhạc, UI tự cuộn và highlight chính xác từng câu theo thời gian thực
            TimeSpan seekTarget = TimeSpan.Zero;
            var service = new LyricsService();
            var vm = new LyricsViewModel(service, pos => seekTarget = pos);

            string sampleLrc = @"[00:05.00]Câu 1
[00:10.00]Câu 2
[00:15.00]Câu 3";

            var parsed = service.ParseLrc(sampleLrc);
            foreach (var line in parsed)
            {
                vm.Lines.Add(new LyricLineViewModel(line, pos => seekTarget = pos));
            }

            int transitionCount = 0;
            vm.ActiveLineChanged += (s, idx) => transitionCount++;

            // Position 0s: Before first line
            vm.UpdatePosition(TimeSpan.FromSeconds(2));
            Assert.AreEqual(-1, vm.ActiveLineIndex);
            Assert.AreEqual(0, transitionCount);

            // Position 5s: Transitions to line 0
            vm.UpdatePosition(TimeSpan.FromSeconds(5.1));
            Assert.AreEqual(0, vm.ActiveLineIndex);
            Assert.IsTrue(vm.Lines[0].IsActive);
            Assert.AreEqual(1, transitionCount);

            // Position 6s, 7s, 8s: Still within line 0 -> MUST NOT trigger transition repeatedly!
            vm.UpdatePosition(TimeSpan.FromSeconds(6));
            vm.UpdatePosition(TimeSpan.FromSeconds(7));
            vm.UpdatePosition(TimeSpan.FromSeconds(8));
            Assert.AreEqual(0, vm.ActiveLineIndex);
            Assert.AreEqual(1, transitionCount); // Transition count must remain 1

            // Position 10.5s: Transitions to line 1
            vm.UpdatePosition(TimeSpan.FromSeconds(10.5));
            Assert.AreEqual(1, vm.ActiveLineIndex);
            Assert.IsFalse(vm.Lines[0].IsActive);
            Assert.IsTrue(vm.Lines[1].IsActive);
            Assert.AreEqual(2, transitionCount);

            // Test line click seek
            vm.Lines[2].SeekCommand.Execute(null);
            Assert.AreEqual(TimeSpan.FromSeconds(15), seekTarget);
        }
    }
}

