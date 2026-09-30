using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using MusicApp.AudioEngine;
using MusicApp.Bff;
using MusicApp.Core.Persistence;
using MusicApp.Core.Persistence.Repositories;
using MusicApp.Core.Services;
using MusicApp.Services;
using MusicApp.ViewModels;

namespace MusicApp
{
    /// <summary>
    /// Lop khoi tao va dieu phoi vong doi toan bo ung dung WPF (Application Bootstrapper &amp; Lifecycle).
    /// 
    /// Tac dung:
    /// - Khoi tao he thong bao mat mang toan cuc (TLS 1.2 / TLS 1.1) tren nen tang .NET Framework 4.6.1.
    /// - Khoi dong may chu web nhung Backend For Frontend (OWIN Self-Host BFF) tai cong cuc bo http://localhost:5245.
    /// - Thiet lap bo may tiem phu thuoc thu cong (Manual Dependency Injection Composition Root):
    ///   NAudioService -> MusicApiClient -> NowPlayingViewModel -> MainViewModel.
    /// - Thiet lap co che bat giu ngoai le toan cuc (Global Exception Handling) de ghi log va phong ngua ung dung bi crash bat ngo.
    /// - Giai phong toan bo tai nguyen mang, unmanaged audio hardware va tien trinh web khi nguoi dung thoat ung dung (OnExit).
    /// 
    /// Van de giai quyet:
    /// - Tuan thu chat che nguyen ly Inversion of Control (IoC): Toan bo cac dich vu cot loi duoc tao ra tai Composition Root
    ///   va truyen vao cac ViewModel thong qua Constructor Injection, khong khoi tao tuy tien ben trong View.
    /// - Tranh tinh trang ro ri tien trinh chay ngam (Zombie Process): Neu ung dung tat ma may chu OWIN hoac WaveOutEvent chua duoc Dispose,
    ///   tien trinh MusicApp.exe se tiep tuc chay ngam trong Task Manager. Phuong thuc ReleaseResources dam bao don dep triet de.
    /// 
    /// Cach thuc van hanh:
    /// - OnStartup: Thiet lap TLS -> Setup Exception Handlers -> Start OWIN Host -> Khoi tao Audio & Client -> Gan DataContext cho MainWindow -> Show().
    /// - OnExit: ReleaseResources() tuan tu MainViewModel -> AudioService -> BffHost.
    /// </summary>
    public partial class App : Application
    {
        private IDisposable _bffHost;
        private NAudioService _audioService;
        private NowPlayingViewModel _nowPlayingViewModel;
        private MainViewModel _mainViewModel;
        private SettingsRepository _settingsRepo;

        /// <summary>
        /// Phuong thuc khoi dong ung dung: Thiet lap moi truong, khoi dong may chu BFF va khoi tao giao dien chinh.
        /// </summary>
        /// <param name="e">Tham so khoi dong ung dung.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Bat buoc su dung giao thuc bao mat TLS 1.2 va TLS 1.1 tren .NET Framework 4.6.1
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            ServicePointManager.DefaultConnectionLimit = 64;

            SetupExceptionHandling();

            // 1. Khoi tao co so du lieu SQLite nhung toc do cao (Phase 6)
            try
            {
                DatabaseInitializer.Initialize();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Loi khoi tao SQLite Database: {0}", ex);
            }

            try
            {
                // Khoi dong may chu OWIN Web API nhung cuc bo tren cong 5245
                _bffHost = BffServerHost.Start("http://localhost:5245");
            }
            catch (Exception ex)
            {
                Trace.TraceError("Khong the khoi dong may chu BFF cuc bo tren http://localhost:5245: {0}", ex);
                MessageBox.Show(
                    "Khong the khoi dong may chu BFF cuc bo tren http://localhost:5245.\n" + ex.Message,
                    "Loi Khoi Dong",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(-1);
                return;
            }

            // Khoi tao cac Repositories va Core Services
            var trackRepo = new TrackRepository();
            var playlistRepo = new PlaylistRepository();
            var queueRepo = new QueueRepository();
            _settingsRepo = new SettingsRepository();
            var interactionRepo = new InteractionRepository();
            var recEngine = new RecommendationEngine(trackRepo, interactionRepo);

            // Khoi tao cac dich vu va ViewModel theo mo hinh Dependency Injection (Composition Root)
            _audioService = new NAudioService();
            var apiClient = new MusicApiClient("http://localhost:5245/api/v1");
            _nowPlayingViewModel = new NowPlayingViewModel(_audioService, recEngine, trackRepo);
            _mainViewModel = new MainViewModel(
                apiClient, 
                _nowPlayingViewModel, 
                trackRepo: trackRepo, 
                playlistRepo: playlistRepo, 
                queueRepo: queueRepo, 
                settingsRepo: _settingsRepo, 
                recEngine: recEngine);

            // Khoi tao cua so chinh va gan DataContext
            var mainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };

            // Khoi phuc cai dat nguoi dung (Volume, Theme, LastView)
            RestoreAppSettings();

            MainWindow = mainWindow;
            mainWindow.Show();
        }

        /// <summary>
        /// Khoi phuc cac cai dat nguoi dung (Volume, Theme, LastView) tu SettingsRepository.
        /// </summary>
        private void RestoreAppSettings()
        {
            if (_settingsRepo == null) return;

            try
            {
                var volTask = _settingsRepo.GetAsync<string>("Volume", "0.80");
                var themeTask = _settingsRepo.GetAsync<string>("Theme", "Dark");
                var viewTask = _settingsRepo.GetAsync<string>("LastView", "Explore");

                Task.WaitAll(new Task[] { volTask, themeTask, viewTask }, 1000);

                if (volTask.IsCompleted && float.TryParse(volTask.Result, out float vol) && _nowPlayingViewModel != null)
                {
                    _nowPlayingViewModel.Volume = Math.Max(0.0f, Math.Min(1.0f, vol));
                }

                if (themeTask.IsCompleted && themeTask.Result == "Light" && _mainViewModel != null && _mainViewModel.IsDarkTheme)
                {
                    _mainViewModel.ToggleThemeCommand?.Execute(null);
                }

                if (viewTask.IsCompleted && !string.IsNullOrWhiteSpace(viewTask.Result) && _mainViewModel != null)
                {
                    _mainViewModel.NavigationCommand?.Execute(viewTask.Result);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Khong the khoi phuc cai dat nguoi dung: {0}", ex);
            }
        }

        /// <summary>
        /// Phuong thuc xu ly khi ung dung dong hoan toan: don dep tai nguyen.
        /// </summary>
        /// <param name="e">Tham so thoat ung dung.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            ReleaseResources();
            base.OnExit(e);
        }

        /// <summary>
        /// Thiet lap he thong bat ngoai le toan cuc de ngan chan crash dot ngot va ghi log loi.
        /// </summary>
        private void SetupExceptionHandling()
        {
            // Bat ngoai le tren UI Dispatcher Thread
            DispatcherUnhandledException += (s, args) =>
            {
                Trace.TraceError("Unhandled Dispatcher Exception: {0}", args.Exception);
#if DEBUG
                args.Handled = false;
#else
                args.Handled = true;
#endif
            };

            // Bat ngoai le tren cac luong Worker Thread thuoc AppDomain
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                Trace.TraceError("Unhandled Domain Exception: {0}", args.ExceptionObject);
                ReleaseResources();
            };
        }

        /// <summary>
        /// Don dep dong bo toan bo tai nguyen unmanaged, may chu web va cac luong am thanh.
        /// </summary>
        private void ReleaseResources()
        {
            try
            {
                if (_settingsRepo != null)
                {
                    var tasks = new List<Task>();
                    if (_nowPlayingViewModel != null)
                    {
                        tasks.Add(_settingsRepo.SetAsync("Volume", _nowPlayingViewModel.Volume.ToString("F2")));
                    }
                    if (_mainViewModel != null)
                    {
                        tasks.Add(_settingsRepo.SetAsync("Theme", _mainViewModel.IsDarkTheme ? "Dark" : "Light"));
                        tasks.Add(_settingsRepo.SetAsync("LastView", _mainViewModel.CurrentViewName));
                        if (_mainViewModel.PlayQueue != null)
                        {
                            tasks.Add(_mainViewModel.PlayQueue.PersistQueueToDatabaseAsync());
                        }
                    }
                    if (tasks.Count > 0)
                    {
                        Task.WaitAll(tasks.ToArray(), 2500);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Loi khi luu Settings truoc khi thoat: {0}", ex);
            }

            try
            {
                _mainViewModel?.Dispose();
                _mainViewModel = null;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Loi khi giai phong MainViewModel: {0}", ex);
            }

            try
            {
                _audioService?.Dispose();
                _audioService = null;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Loi khi giai phong AudioService: {0}", ex);
            }

            try
            {
                _bffHost?.Dispose();
                _bffHost = null;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Loi khi giai phong BffHost: {0}", ex);
            }
        }
    }
}
