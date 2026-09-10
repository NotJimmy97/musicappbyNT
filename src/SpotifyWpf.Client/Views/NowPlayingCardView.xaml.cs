using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using SpotifyWpf.Client.ViewModels;
using SpotifyWpf.Core.Models;

namespace SpotifyWpf.Client.Views
{
    public partial class NowPlayingCardView : UserControl
    {
        private Storyboard _spinStoryboard;
        private NowPlayingViewModel _viewModel;
        private bool _isStoryboardStarted = false;

        public NowPlayingCardView()
        {
            InitializeComponent();
            _spinStoryboard = (Storyboard)Resources["VinylSpinStoryboard"];
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            _viewModel = DataContext as NowPlayingViewModel;

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
                UpdateAnimationState(_viewModel.PlaybackState);
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NowPlayingViewModel.PlaybackState))
            {
                UpdateAnimationState(_viewModel.PlaybackState);
            }
        }

        private void UpdateAnimationState(PlaybackState state)
        {
            if (_spinStoryboard == null) return;

            switch (state)
            {
                case PlaybackState.Playing:
                    if (!_isStoryboardStarted)
                    {
                        _spinStoryboard.Begin(this, isControllable: true);
                        _isStoryboardStarted = true;
                    }
                    else
                    {
                        _spinStoryboard.Resume(this);
                    }
                    break;

                case PlaybackState.Paused:
                    if (_isStoryboardStarted)
                    {
                        _spinStoryboard.Pause(this);
                    }
                    break;

                case PlaybackState.Stopped:
                case PlaybackState.Faulted:
                    if (_isStoryboardStarted)
                    {
                        _spinStoryboard.Stop(this);
                        _isStoryboardStarted = false;
                    }
                    break;
            }
        }

        private void TimelineSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel?.SetUserSeeking(true);
        }

        private void TimelineSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.SetUserSeeking(false, TimelineSlider.Value);
            }
        }
    }
}
