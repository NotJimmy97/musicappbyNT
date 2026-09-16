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
    /// <summary>
    /// ViewModel quan ly quet va tim kiem thu vien am nhac cuc bo (Local Audio Library Scanner ViewModel).
    /// 
    /// Tac dung:
    /// - Cho phep nguoi dung chon thu muc tren o cung thong qua hop thoai FolderBrowserDialog.
    /// - Dieu phoi tien trinh quet de quy cac tap tin am thanh (MP3, FLAC, M4A, WAV,...) tren luong nen.
    /// - Cap nhat tien do quet theo thoi gian thuc len giao dien: so file da doc, so bai hat tim thay, ten file dang xu ly.
    /// - Cung cap chuc nang tim kiem, loc nhanh danh sach bai hat offline theo tieu de, ca si, hoac album.
    /// 
    /// Van de giai quyet:
    /// - Tranh gay treo giao dien (UI Freeze): Thao tac I/O tren dia duoc thuc thi hoan toan bat dong bo (Async/Await).
    /// - Cung cap nut dung quet (Cancel Scan): Nguoi dung co the chu dong ngat tien trinh quet bat ky luc nao
    ///   ma khong gay loi crash ung dung nho co che CancellationTokenSource.
    /// - Quan ly bo nho hieu qua: Danh sach FilteredTracks chi luu cac tham chieu TrackItemViewModel phu hop voi bo loc SearchFilter.
    /// 
    /// Cach thuc van hanh:
    /// - Mac dinh chon thu muc Windows MyMusic cua nguoi dung.
    /// - Khi nhan nut "Quét Thư Mục", goi _localLibraryService.ScanDirectoryAsync kem progress report.
    /// - Sau khi quet xong, danh sach bai hat duoc nap vao AllTracks va hien thi qua FilteredTracks.
    /// </summary>
    public class LocalLibraryViewModel : ObservableObject
    {
        private readonly ILocalLibraryService _localLibraryService;
        private readonly Action<TrackModel> _onPlayTrack;
        private CancellationTokenSource _scanCts;

        private string _selectedFolderPath;

        /// <summary>
        /// Duong dan thu muc tren o cung duoc chon de quet am thanh.
        /// </summary>
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

        /// <summary>
        /// Trang thai he thong co dang trong tien trinh quet thu muc hay khong.
        /// </summary>
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

        /// <summary>
        /// Thong bao trang thai chi tiet hien thi cho nguoi dung.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private int _filesScannedCount;

        /// <summary>
        /// So luong tap tin da duyet qua trong qua trinh quet.
        /// </summary>
        public int FilesScannedCount
        {
            get => _filesScannedCount;
            set => SetProperty(ref _filesScannedCount, value);
        }

        private string _searchFilter = string.Empty;

        /// <summary>
        /// Tu khoa loc danh sach bai hat offline hien tai.
        /// </summary>
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

        /// <summary>
        /// Bai hat dang duoc chon trong danh sach offline.
        /// </summary>
        public TrackItemViewModel SelectedTrack
        {
            get => _selectedTrack;
            set => SetProperty(ref _selectedTrack, value);
        }

        /// <summary>
        /// Danh sach goc chua toan bo cac bai hat quet duoc trong thu muc.
        /// </summary>
        public ObservableCollection<TrackItemViewModel> AllTracks { get; } = new ObservableCollection<TrackItemViewModel>();

        /// <summary>
        /// Danh sach bai hat da qua bo loc SearchFilter hien thi tren DataGrid / ListBox XAML.
        /// </summary>
        public ObservableCollection<TrackItemViewModel> FilteredTracks { get; } = new ObservableCollection<TrackItemViewModel>();

        /// <summary>
        /// Lenh mo hop thoai FolderBrowserDialog de chon thu muc am thanh.
        /// </summary>
        public RelayCommand BrowseCommand { get; }

        /// <summary>
        /// Lenh bat dau quet thu muc bat dong bo.
        /// </summary>
        public AsyncRelayCommand ScanCommand { get; }

        /// <summary>
        /// Lenh yeu cau huy bo tien trinh quet dang chay.
        /// </summary>
        public RelayCommand CancelScanCommand { get; }

        /// <summary>
        /// Lenh xoa trang o loc tim kiem.
        /// </summary>
        public RelayCommand ClearFilterCommand { get; }

        /// <summary>
        /// Lenh phat bai hat duoc chon tu thu vien offline.
        /// </summary>
        public RelayCommand PlayTrackCommand { get; }

        /// <summary>
        /// Khoi tao LocalLibraryViewModel voi dich vu quet va callback phat nhac.
        /// </summary>
        /// <param name="localLibraryService">Dich vu quet am thanh offline.</param>
        /// <param name="onPlayTrack">Callback phat ban nhac duoc chon.</param>
        public LocalLibraryViewModel(ILocalLibraryService localLibraryService, Action<TrackModel> onPlayTrack)
        {
            _localLibraryService = localLibraryService ?? throw new ArgumentNullException(nameof(localLibraryService));
            _onPlayTrack = onPlayTrack;

            // Mac dinh tro toi thu muc MyMusic cua nguoi dung Windows
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

        /// <summary>
        /// Hien thi hop thoai FolderBrowserDialog de nguoi dung chon thu muc chua nhac.
        /// </summary>
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

        /// <summary>
        /// Thuc thi quet thu muc am thanh bat dong bo, cap nhat tien do thoi gian thuc qua Progress.
        /// </summary>
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

        /// <summary>
        /// Phat tin hieu huy bo tien trinh quet thu muc dang dien ra.
        /// </summary>
        public void ExecuteCancelScan()
        {
            if (_scanCts != null && !_scanCts.IsCancellationRequested)
            {
                _scanCts.Cancel();
            }
        }

        /// <summary>
        /// Ap dung bo loc tim kiem len danh sach AllTracks de tao FilteredTracks.
        /// </summary>
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
