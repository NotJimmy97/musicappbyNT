using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MusicApp.Core.Models;
using MusicApp.ViewModels;

namespace MusicApp.Views
{
    public partial class NowPlayingCardView : UserControl
    {
        private Storyboard _spinStoryboard;
        private bool _isStoryboardActive = false;

        public NowPlayingCardView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += (s, e) => _spinStoryboard = (Storyboard)Resources["VinylSpinStoryboard"];
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is NowPlayingViewModel oldVm)
            {
                oldVm.PropertyChanged -= OnViewModelPropertyChanged;
            }

            if (e.NewValue is NowPlayingViewModel newVm)
            {
                newVm.PropertyChanged += OnViewModelPropertyChanged;
                UpdateAnimationState(newVm);
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is NowPlayingViewModel vm)
            {
                if (e.PropertyName == nameof(NowPlayingViewModel.PlaybackState) ||
                    e.PropertyName == nameof(NowPlayingViewModel.IsSpinEnabled))
                {
                    UpdateAnimationState(vm);
                }
            }
        }

        private void UpdateAnimationState(NowPlayingViewModel vm)
        {
            if (_spinStoryboard == null)
            {
                _spinStoryboard = (Storyboard)Resources["VinylSpinStoryboard"];
            }

            if (_spinStoryboard == null || vm == null) return;

            if (!vm.IsSpinEnabled)
            {
                if (_isStoryboardActive)
                {
                    _spinStoryboard.Stop(this);
                    _isStoryboardActive = false;
                }
                VinylRotation.Angle = 0;
                return;
            }

            switch (vm.PlaybackState)
            {
                case PlaybackState.Playing:
                    if (!_isStoryboardActive)
                    {
                        _spinStoryboard.Begin(this, true);
                        _isStoryboardActive = true;
                    }
                    else
                    {
                        _spinStoryboard.Resume(this);
                    }
                    break;

                case PlaybackState.Paused:
                    if (_isStoryboardActive)
                    {
                        _spinStoryboard.Pause(this);
                    }
                    break;

                case PlaybackState.Stopped:
                case PlaybackState.Faulted:
                    if (_isStoryboardActive)
                    {
                        _spinStoryboard.Stop(this);
                        _isStoryboardActive = false;
                        VinylRotation.Angle = 0;
                    }
                    break;
            }
        }

        private void TimelineSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is NowPlayingViewModel vm)
            {
                vm.SetUserSeeking(true);
            }
        }

        private void TimelineSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is NowPlayingViewModel vm)
            {
                vm.SetUserSeeking(false, TimelineSlider.Value);
            }
        }
    }
}

