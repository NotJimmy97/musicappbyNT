using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Core.Common;
using MusicApp.Core.Models;
using MusicApp.Core.Persistence;
using MusicApp.Core.Persistence.Repositories;

namespace MusicApp.Tests
{
    [TestClass]
    public class PlaylistRepositoryTests
    {
        private string _tempDbFile;
        private PlaylistRepository _playlistRepo;
        private TrackRepository _trackRepo;

        [TestInitialize]
        public void Setup()
        {
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"playlist_test_db_{Guid.NewGuid():N}.db");
            string connStr = $"Data Source={_tempDbFile};Version=3;Journal Mode=WAL;Synchronous=NORMAL;Cache Size=-64000;Foreign Keys=True;Default Timeout=5;";
            DatabaseInitializer.SetCustomConnectionString(connStr);
            DatabaseInitializer.Initialize();

            _playlistRepo = new PlaylistRepository(connStr);
            _trackRepo = new TrackRepository(connStr);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DatabaseInitializer.ResetInitialization();
            if (File.Exists(_tempDbFile))
            {
                try { File.Delete(_tempDbFile); } catch { }
            }
        }

        [TestMethod]
        public async Task CreatePlaylist_AddsPlaylistAndRetrieves()
        {
            // Act
            var created = await _playlistRepo.CreatePlaylistAsync("Giai Điệu Chill", "Danh sách thư giãn cuối tuần");
            var playlists = (await _playlistRepo.GetAllPlaylistsAsync()).ToList();

            // Assert
            Assert.IsNotNull(created);
            Assert.IsTrue(created.Id > 0);
            Assert.AreEqual(1, playlists.Count);
            Assert.AreEqual("Giai Điệu Chill", playlists[0].Name);
            Assert.AreEqual("Danh sách thư giãn cuối tuần", playlists[0].Description);
        }

        [TestMethod]
        public async Task AddTrackToPlaylist_AddsAndRetrievesTracksInOrder()
        {
            // Arrange
            var created = await _playlistRepo.CreatePlaylistAsync("Rock Classics");
            int playlistId = created.Id;
            var track1 = new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Track A", "Band 1"),
                Title = "Track A",
                Artist = "Band 1",
                DurationSeconds = 200,
                SourceType = "local",
                SourceId = @"C:\Music\a.mp3"
            };
            var track2 = new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Track B", "Band 2"),
                Title = "Track B",
                Artist = "Band 2",
                DurationSeconds = 250,
                SourceType = "local",
                SourceId = @"C:\Music\b.mp3"
            };
            int t1Id = await _trackRepo.InsertOrUpdateAsync(track1);
            int t2Id = await _trackRepo.InsertOrUpdateAsync(track2);

            // Act
            await _playlistRepo.AddTrackToPlaylistAsync(playlistId, t1Id);
            await _playlistRepo.AddTrackToPlaylistAsync(playlistId, t2Id);

            var tracksInPlaylist = (await _playlistRepo.GetTracksInPlaylistAsync(playlistId)).ToList();

            // Assert
            Assert.AreEqual(2, tracksInPlaylist.Count);
            Assert.AreEqual("Track A", tracksInPlaylist[0].Title);
            Assert.AreEqual("Track B", tracksInPlaylist[1].Title);
        }
    }
}
