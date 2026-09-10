using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using SpotifyWpf.AudioEngine;
using SpotifyWpf.Bff;
using SpotifyWpf.Client.Services;
using SpotifyWpf.Client.ViewModels;

namespace SpotifyWpf.Client
{
    public partial class App : Application
    {
        private IDisposable _bffHost;
        private NAudioService _audioService;
        private MainViewModel _mainViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Mandatory process-wide TLS 1.2 enforcement on .NET Framework 4.6.1
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            ServicePointManager.DefaultConnectionLimit = 64;

            SetupExceptionHandling();

            try
            {
                // Initialize and bind Local OWIN Self-Host BFF to port 5245
                _bffHost = BffServerHost.Start("http://localhost:5245");
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to start local BFF service on http://localhost:5245: {0}", ex);
                MessageBox.Show(
                    "Failed to start local BFF service on http://localhost:5245.\n" + ex.Message,
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(-1);
                return;
            }

            // Dependency Injection Bootstrap
            _audioService = new NAudioService();
            var apiClient = new MusicApiClient("http://localhost:5245/api/v1");
            var nowPlayingViewModel = new NowPlayingViewModel(_audioService);
            _mainViewModel = new MainViewModel(apiClient, nowPlayingViewModel);

            var mainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };

            MainWindow = mainWindow;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ReleaseResources();
            base.OnExit(e);
        }

        private void SetupExceptionHandling()
        {
            DispatcherUnhandledException += (s, args) =>
            {
                Trace.TraceError("Unhandled Dispatcher Exception: {0}", args.Exception);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                Trace.TraceError("Unhandled Domain Exception: {0}", args.ExceptionObject);
                ReleaseResources();
            };
        }

        private void ReleaseResources()
        {
            try
            {
                _mainViewModel?.Dispose();
                _mainViewModel = null;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Error disposing MainViewModel: {0}", ex);
            }

            try
            {
                _audioService?.Dispose();
                _audioService = null;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Error disposing AudioService: {0}", ex);
            }

            try
            {
                _bffHost?.Dispose();
                _bffHost = null;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Error disposing BffHost: {0}", ex);
            }
        }
    }
}


