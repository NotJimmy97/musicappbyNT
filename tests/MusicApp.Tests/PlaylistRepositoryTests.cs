using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Core.Models;
using MusicApp.Core.Persistence;
using MusicApp.Core.Persistence.Repositories;
using MusicApp.Core.Utilities;

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
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"playlist_repo_test_{Guid.NewGuid():N}.db");
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
                try
                {
                    File.Delete(_tempDbFile);
                }
                catch
                {
                    // Ignore SQLite file lock on immediate cleanup
                }
            }
        }

        [TestMethod]
        public async Task CreatePlaylist_And_GetById_ReturnsPlaylist()
        {
            // Act
            var playlist = await _playlistRepo.CreatePlaylistAsync("My Chill Mix", "Acoustic and lo-fi vibes");
            var retrieved = await _playlistRepo.GetByIdAsync(playlist.Id);

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("My Chill Mix", retrieved.Name);
            Assert.AreEqual("Acoustic and lo-fi vibes", retrieved.Description);
        }

        [TestMethod]
        public async Task AddTrackToPlaylist_AddsTrackInOrder()
        {
            // Arrange
            var playlist = await _playlistRepo.CreatePlaylistAsync("Rock Classics");
            int track1Id = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Bohemian Rhapsody", "Queen"),
                Title = "Bohemian Rhapsody",
                Artist = "Queen",
                SourceType = "local",
                SourceId = @"C:\Music\Queen.mp3"
            });
            int track2Id = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Hotel California", "Eagles"),
                Title = "Hotel California",
                Artist = "Eagles",
                SourceType = "local",
                SourceId = @"C:\Music\Eagles.mp3"
            });

            // Act
            await _playlistRepo.AddTrackToPlaylistAsync(playlist.Id, track1Id);
            await _playlistRepo.AddTrackToPlaylistAsync(playlist.Id, track2Id);

            var tracks = (await _playlistRepo.GetTracksInPlaylistAsync(playlist.Id)).ToList();

            // Assert
            Assert.AreEqual(2, tracks.Count);
            Assert.AreEqual("Bohemian Rhapsody", tracks[0].Title);
            Assert.AreEqual("Hotel California", tracks[1].Title);
        }

        [TestMethod]
        public async Task RemoveTrackFromPlaylist_RemovesCorrectTrack()
        {
            // Arrange
            var playlist = await _playlistRepo.CreatePlaylistAsync("Favorites 2026");
            int track1Id = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Song 1", "Artist 1"),
                Title = "Song 1",
                Artist = "Artist 1",
                SourceType = "local",
                SourceId = @"C:\Music\1.mp3"
            });
            int track2Id = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Song 2", "Artist 2"),
                Title = "Song 2",
                Artist = "Artist 2",
                SourceType = "local",
                SourceId = @"C:\Music\2.mp3"
            });

            await _playlistRepo.AddTrackToPlaylistAsync(playlist.Id, track1Id);
            await _playlistRepo.AddTrackToPlaylistAsync(playlist.Id, track2Id);

            // Act
            await _playlistRepo.RemoveTrackFromPlaylistAsync(playlist.Id, track1Id);
            var remaining = (await _playlistRepo.GetTracksInPlaylistAsync(playlist.Id)).ToList();

            // Assert
            Assert.AreEqual(1, remaining.Count);
            Assert.AreEqual("Song 2", remaining[0].Title);
        }

        [TestMethod]
        public async Task ReorderTrack_UpdatesPositions()
        {
            // Arrange
            var playlist = await _playlistRepo.CreatePlaylistAsync("Reorder Playlist");
            int track1Id = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("First", "A"),
                Title = "First",
                Artist = "A",
                SourceType = "local",
                SourceId = @"C:\Music\1.mp3"
            });
            int track2Id = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Second", "B"),
                Title = "Second",
                Artist = "B",
                SourceType = "local",
                SourceId = @"C:\Music\2.mp3"
            });

            await _playlistRepo.AddTrackToPlaylistAsync(playlist.Id, track1Id);
            await _playlistRepo.AddTrackToPlaylistAsync(playlist.Id, track2Id);

            // Act - Move track2 (index 1) to position 0
            await _playlistRepo.ReorderTrackAsync(playlist.Id, track2Id, 0);
            var reordered = (await _playlistRepo.GetTracksInPlaylistAsync(playlist.Id)).ToList();

            // Assert
            Assert.AreEqual(2, reordered.Count);
            Assert.AreEqual("Second", reordered[0].Title);
            Assert.AreEqual("First", reordered[1].Title);
        }

        [TestMethod]
        public async Task DeletePlaylist_RemovesPlaylistAndTracks()
        {
            // Arrange
            var playlist = await _playlistRepo.CreatePlaylistAsync("Temp Playlist");
            int trackId = await _trackRepo.InsertOrUpdateAsync(new TrackEntity
            {
                TrackKey = TrackIdentityHelper.GenerateTrackKey("Temp Song", "Temp"),
                Title = "Temp Song",
                Artist = "Temp",
                SourceType = "local",
                SourceId = @"C:\Music\temp.mp3"
            });
            await _playlistRepo.AddTrackToPlaylistAsync(playlist.Id, trackId);

            // Act
            await _playlistRepo.DeletePlaylistAsync(playlist.Id);
            var retrieved = await _playlistRepo.GetByIdAsync(playlist.Id);
            var allPlaylists = (await _playlistRepo.GetAllPlaylistsAsync()).ToList();

            // Assert
            Assert.IsNull(retrieved);
            Assert.IsFalse(allPlaylists.Any(p => p.Id == playlist.Id));
        }
    }
}
