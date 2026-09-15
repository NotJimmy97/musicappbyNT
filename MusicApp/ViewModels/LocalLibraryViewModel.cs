using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Common;
using MusicApp.Core.Interfaces;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    public class LocalLibraryViewModel : ObservableObject
    {
        private readonly ILocalLibraryService _localLibraryService;
        private readonly Action<TrackModel> _onPlayTrack;
        private CancellationTokenSource _scanCts;

        private string _selectedFolderPath;
        public string SelectedFolderPath
        {
            get => _selectedFolderPath;
            set
            {
                if (SetProperty(ref _selectedFolderPath, value))
                {
                    ScanCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isScanning;
        public bool IsScanning
        {
            get => _isScanning;
            set
            {
                if (SetProperty(ref _isScanning, value))
                {
                    ScanCommand?.RaiseCanExecuteChanged();
                    CancelScanCommand?.RaiseCanExecuteChanged();
                    BrowseCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _statusMessage = "S\u1EB5n s\u00E0ng qu\u00E9t th\u01B0 vi\u1EC7n c\u00E1 nh\u00E2n.";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private int _filesScannedCount;
        public int FilesScannedCount
        {
            get => _filesScannedCount;
            set => SetProperty(ref _filesScannedCount, value);
        }

        private string _searchFilter = string.Empty;
        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                if (SetProperty(ref _searchFilter, value))
                {
                    ApplyFilter();
                }
            }
        }

        private TrackItemViewModel _selectedTrack;
        public TrackItemViewModel SelectedTrack
        {
            get => _selectedTrack;
            set => SetProperty(ref _selectedTrack, value);
        }

        public ObservableCollection<TrackItemViewModel> AllTracks { get; } = new ObservableCollection<TrackItemViewModel>();
        public ObservableCollection<TrackItemViewModel> FilteredTracks { get; } = new ObservableCollection<TrackItemViewModel>();

        public RelayCommand BrowseCommand { get; }
        public AsyncRelayCommand ScanCommand { get; }
        public RelayCommand CancelScanCommand { get; }
        public RelayCommand ClearFilterCommand { get; }
        public RelayCommand PlayTrackCommand { get; }

        public LocalLibraryViewModel(ILocalLibraryService localLibraryService, Action<TrackModel> onPlayTrack)
        {
            _localLibraryService = localLibraryService ?? throw new ArgumentNullException(nameof(localLibraryService));
            _onPlayTrack = onPlayTrack;

            // Default to Windows MyMusic standard user folder
            string defaultMusicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            if (Directory.Exists(defaultMusicFolder))
            {
                _selectedFolderPath = defaultMusicFolder;
            }

            BrowseCommand = new RelayCommand(_ => ExecuteBrowse(), _ => !IsScanning);
            ScanCommand = new AsyncRelayCommand(async _ => await ExecuteScanAsync(), _ => !IsScanning && !string.IsNullOrWhiteSpace(SelectedFolderPath));
            CancelScanCommand = new RelayCommand(_ => ExecuteCancelScan(), _ => IsScanning);
            ClearFilterCommand = new RelayCommand(_ => SearchFilter = string.Empty);
            PlayTrackCommand = new RelayCommand(p =>
            {
                if (p is TrackItemViewModel trackVm)
                {
                    SelectedTrack = trackVm;
                    _onPlayTrack?.Invoke(trackVm.Track);
                }
            });
        }

        private void ExecuteBrowse()
        {
            try
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = "Ch\u1ECDn th\u01B0 m\u1EE5c ch\u1EE9a c\u00E1c t\u1EC7p \u00E2m thanh (MP3, FLAC, M4A, WAV):";
                    dialog.ShowNewFolderButton = false;

                    if (!string.IsNullOrWhiteSpace(SelectedFolderPath) && Directory.Exists(SelectedFolderPath))
                    {
                        dialog.SelectedPath = SelectedFolderPath;
                    }

                    var result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                    {
                        SelectedFolderPath = dialog.SelectedPath;
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Kh\u00F4ng th\u1EC3 m\u1EDF h\u1ED9p tho\u1EA1i ch\u1ECDn th\u01B0 m\u1EE5c: " + ex.Message;
            }
        }

        public async Task ExecuteScanAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedFolderPath) || !Directory.Exists(SelectedFolderPath))
            {
                StatusMessage = "\u0110\u01B0\u1EDDng d\u1EABn th\u01B0 m\u1EE5c kh\u00F4ng h\u1EE3p l\u1EC7 ho\u1EB7c kh\u00F4ng t\u1ED3n t\u1EA1i.";
                return;
            }

            _scanCts?.Cancel();
            _scanCts = new CancellationTokenSource();
            var token = _scanCts.Token;

            IsScanning = true;
            FilesScannedCount = 0;
            StatusMessage = "\u0110ang qu\u00E9t th\u01B0 vi\u1EC7n c\u1EE5c b\u1ED9...";

            var progress = new Progress<ScanProgressReport>(report =>
            {
                FilesScannedCount = report.FilesScanned;
                StatusMessage = string.Format("\u0110\u00E3 duy\u1EC7t: {0} t\u1EC7p | T\u00ECm th\u1EA5y: {1} b\u00E0i h\u00E1t ({2})",
                    report.FilesScanned, report.TracksFound, report.CurrentFile);
            });

            try
            {
                var scannedTracks = await _localLibraryService.ScanDirectoryAsync(SelectedFolderPath, progress, token);

                AllTracks.Clear();
                foreach (var track in scannedTracks)
                {
                    var itemVm = new TrackItemViewModel(track, t => _onPlayTrack?.Invoke(t));
                    AllTracks.Add(itemVm);
                }

                ApplyFilter();
                StatusMessage = string.Format("Ho\u00E0n t\u1EA5t qu\u00E9t. \u0110\u00E3 nh\u1EADn di\u1EC7n {0} b\u00E0i h\u00E1t h\u1EE3p l\u1EC7.", AllTracks.Count);
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "\u0110\u00E3 d\u1EEBng qu\u00E9t th\u01B0 vi\u1EC7n theo y\u00EAu c\u1EA7u.";
            }
            catch (Exception ex)
            {
                StatusMessage = "L\u1ED7i trong qu\u00E1 tr\u00ECnh qu\u00E9t: " + ex.Message;
            }
            finally
            {
                IsScanning = false;
                _scanCts = null;
            }
        }

        public void ExecuteCancelScan()
        {
            if (_scanCts != null && !_scanCts.IsCancellationRequested)
            {
                _scanCts.Cancel();
            }
        }

        public void ApplyFilter()
        {
            FilteredTracks.Clear();
            string filter = SearchFilter?.Trim();

            if (string.IsNullOrWhiteSpace(filter))
            {
                foreach (var track in AllTracks)
                {
                    FilteredTracks.Add(track);
                }
                return;
            }

            for (int i = 0; i < AllTracks.Count; i++)
            {
                var track = AllTracks[i];
                if ((track.Title != null && track.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (track.Artist != null && track.Artist.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (track.Album != null && track.Album.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    FilteredTracks.Add(track);
                }
            }
        }
    }
}
