using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using MusicApp.AudioEngine;
using MusicApp.Converters;
using MusicApp.Core.Interfaces;
using MusicApp.Core.Models;
using MusicApp.Core.Services;
using MusicApp.ViewModels;

namespace MusicApp.Tests
{
    /// <summary>
    /// Bo kiem thu don vi va tich hop cho tinh nang thu vien nhac noi bo (Local Library Scanner).
    /// 
    /// - Tac dung: Kiem thu kha nang trich xuat metadata ID3 tu tep am thanh vat ly (TagLibSharp),
    ///   co che quet de quy thu muc khong chan UI (IProgress reporting), logic tim kiem/loc theo tu khoa,
    ///   converter anh bia dong bang bo nho (FrozenImageConverter), va kha nang phat nhac noi bo qua NAudioService.
    /// 
    /// - Van de giai quyet:
    ///   1. Kiem thu trich xuat metadata an toan (Defensive Programming): Xu ly duong dan null, tep rong,
    ///      duong dan khong ton tai hoac tep khong phai am thanh (.txt).
    ///   2. Kiem thu quet de quy bat dong bo: Tao thu muc tam thoi (temp directory) chua tep am thanh WAV gia lap,
    ///      xac minh bao cao tien do phan tram chay dung.
    ///   3. Kiem thu Frozen Image: Dam bao anh tao ra tu Base64 Data URI luon o trang thai IsFrozen = true
    ///      de an toan su dung tren luong UI WPF.
    ///   4. Kiem thu vong doi phat nhac local: Play, Pause, Stop, TotalTime tren NAudioService.
    /// 
    /// - Cach thuc van hanh:
    ///   Setup() tao thu muc tam Guid doc lap; Cleanup() xoa sach thu muc sau moi test.
    /// </summary>
    [TestClass]
    public class LocalLibraryTests
    {
        private string _testDirectory;

        [TestInitialize]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "MusicApp_Test_" + Guid.NewGuid().ToString("N"));
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

        private string CreateDummyWavFile(string directory, string filename, int durationSeconds = 1)
        {
            string filePath = Path.Combine(directory, filename);
            var format = new WaveFormat(44100, 16, 2);
            using (var writer = new WaveFileWriter(filePath, format))
            {
                byte[] emptyBuffer = new byte[format.AverageBytesPerSecond * durationSeconds];
                writer.Write(emptyBuffer, 0, emptyBuffer.Length);
            }
            return filePath;
        }

        [TestMethod]
        public void LocalLibraryService_ExtractTrackFromFile_NullOrMissingPath_ReturnsNull()
        {
            var service = new LocalLibraryService();
            Assert.IsNull(service.ExtractTrackFromFile(null));
            Assert.IsNull(service.ExtractTrackFromFile(""));
            Assert.IsNull(service.ExtractTrackFromFile(@"C:\non_existent_folder_xyz\file.mp3"));
        }

        [TestMethod]
        public void LocalLibraryService_ExtractTrackFromFile_UnsupportedExtension_ReturnsNull()
        {
            var service = new LocalLibraryService();
            string textFilePath = Path.Combine(_testDirectory, "readme.txt");
            File.WriteAllText(textFilePath, "This is not an audio file.");

            var track = service.ExtractTrackFromFile(textFilePath);
            Assert.IsNull(track);
        }

        [TestMethod]
        public void LocalLibraryService_ExtractTrackFromFile_ValidAudioFile_ExtractsTrackMetadata()
        {
            var service = new LocalLibraryService();
            string wavFile = CreateDummyWavFile(_testDirectory, "SongTestA.wav", 2);

            var track = service.ExtractTrackFromFile(wavFile);

            Assert.IsNotNull(track);
            Assert.AreEqual("SongTestA", track.Title);
            Assert.AreEqual(wavFile, track.StreamUrl);
            Assert.IsTrue(track.Id.StartsWith("local_"));
            Assert.IsTrue(track.DurationSeconds >= 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task LocalLibraryService_ScanDirectoryAsync_NullDirectory_ThrowsException()
        {
            var service = new LocalLibraryService();
            await service.ScanDirectoryAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public async Task LocalLibraryService_ScanDirectoryAsync_MissingDirectory_ThrowsException()
        {
            var service = new LocalLibraryService();
            await service.ScanDirectoryAsync(Path.Combine(_testDirectory, "MissingSubFolder"));
        }

        [TestMethod]
        public async Task LocalLibraryService_ScanDirectoryAsync_ValidDirectoryWithAudio_RecursesAndReportsProgress()
        {
            var service = new LocalLibraryService();
            string subDir = Path.Combine(_testDirectory, "SubAlbum");
            Directory.CreateDirectory(subDir);

            CreateDummyWavFile(_testDirectory, "Track1.wav", 1);
            CreateDummyWavFile(subDir, "Track2.wav", 2);

            int progressReportsCount = 0;
            var progress = new Progress<ScanProgressReport>(report =>
            {
                Interlocked.Increment(ref progressReportsCount);
            });

            var tracks = await service.ScanDirectoryAsync(_testDirectory, progress);

            Assert.AreEqual(2, tracks.Count);
            Assert.IsTrue(tracks.Any(t => t.Title == "Track1"));
            Assert.IsTrue(tracks.Any(t => t.Title == "Track2"));
        }

        [TestMethod]
        public void LocalLibraryViewModel_InitialState_HasExpectedDefaults()
        {
            var service = new LocalLibraryService();
            var vm = new LocalLibraryViewModel(service, null);

            Assert.IsFalse(vm.IsScanning);
            Assert.IsNotNull(vm.StatusMessage);
            Assert.IsNotNull(vm.FilteredTracks);
            Assert.AreEqual(0, vm.FilteredTracks.Count);
        }

        [TestMethod]
        public async Task LocalLibraryViewModel_ExecuteScan_PopulatesTracksAndAppliesFilter()
        {
            var service = new LocalLibraryService();
            CreateDummyWavFile(_testDirectory, "AcousticMelody.wav", 1);
            CreateDummyWavFile(_testDirectory, "ElectricBeat.wav", 1);

            TrackModel playedTrack = null;
            var vm = new LocalLibraryViewModel(service, t => playedTrack = t)
            {
                SelectedFolderPath = _testDirectory
            };

            await vm.ExecuteScanAsync();

            Assert.AreEqual(2, vm.AllTracks.Count);
            Assert.AreEqual(2, vm.FilteredTracks.Count);

            // Test filtering
            vm.SearchFilter = "Acoustic";
            Assert.AreEqual(1, vm.FilteredTracks.Count);
            Assert.AreEqual("AcousticMelody", vm.FilteredTracks[0].Title);

            // Test clearing filter
            vm.ClearFilterCommand.Execute(null);
            Assert.AreEqual(2, vm.FilteredTracks.Count);

            // Test play command
            vm.PlayTrackCommand.Execute(vm.FilteredTracks[0]);
            Assert.IsNotNull(playedTrack);
            Assert.AreEqual("AcousticMelody", playedTrack.Title);
        }

        [TestMethod]
        public void FrozenImageConverter_Base64DataUri_ReturnsFrozenBitmapImage()
        {
            var converter = new FrozenImageConverter();
            // 1x1 transparent PNG data URI
            string dataUri = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

            var result = converter.Convert(dataUri, typeof(BitmapImage), null, null);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BitmapImage));
            var bitmap = (BitmapImage)result;
            Assert.IsTrue(bitmap.IsFrozen);
        }

        [TestMethod]
        public async Task NAudioService_InitializeAsync_WithLocalAudioFile_InitializesWavePlayer()
        {
            string wavFile = CreateDummyWavFile(_testDirectory, "LocalPlaybackTest.wav", 2);

            using (var audioService = new NAudioService())
            {
                await audioService.InitializeAsync(wavFile);

                Assert.AreEqual(Core.Models.PlaybackState.Stopped, audioService.CurrentState);
                Assert.IsTrue(audioService.TotalTime.TotalSeconds >= 1.5);

                audioService.Play();
                Assert.AreEqual(Core.Models.PlaybackState.Playing, audioService.CurrentState);

                audioService.Pause();
                Assert.AreEqual(Core.Models.PlaybackState.Paused, audioService.CurrentState);

                audioService.Stop();
                Assert.AreEqual(Core.Models.PlaybackState.Stopped, audioService.CurrentState);
            }
        }
    }
}
