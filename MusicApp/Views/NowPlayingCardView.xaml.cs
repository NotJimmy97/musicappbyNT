using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MusicApp.Core.Models;
using MusicApp.ViewModels;

namespace MusicApp.Views
{
    /// <summary>
    /// Code-behind cua the phat nhac phong cach Spotify-Readme (Now Playing Card View Code-Behind).
    /// 
    /// Tac dung:
    /// - Dieu khien hieu ung hoat hoa dia than xoay tron (Vinyl Spin Storyboard Animation) dua tren trang thai phat nhac.
    /// - Xu ly tuong tac keo tha truc tiep tren thanh thoi gian TimelineSlider (PreviewMouseDown / PreviewMouseUp).
    /// - Quan ly viec dang ky va huy dang ky su kien PropertyChanged cua NowPlayingViewModel de tranh ro ri bo nho.
    /// 
    /// Van de giai quyet:
    /// - Nguyen ly MVVM cho tuong tac Visual Tree: WPF Storyboard va chuot keo Slider la cac hanh vi truc tiep tren cay giao dien
    ///   (Visual Tree Interactions). Thay vi viet logic nghiep vu vao day, code-behind chi dong vai tro la cau noi
    ///   (Bridge) dieu khien Storyboard va chuyen giao su kien keo chuot xuong ViewModel bang phuong thuc vm.SetUserSeeking().
    /// - Hieu ung xoay dia thong minh (Smart Animation State):
    ///   + Khi Playing: Khoi dong Storyboard.Begin neu chua chay, hoac Storyboard.Resume neu truoc do tam dung.
    ///   + Khi Paused: Tam dung Storyboard.Pause de dia than dung yen tai goc quay hien tai.
    ///   + Khi Stopped/Faulted hoac IsSpinEnabled = false: Dung Storyboard.Stop va dat goc quay VinylRotation.Angle ve 0 do.
    /// - Chong giat thanh truot (Anti-Jitter Seeking): Khi nguoi dung bam chuot vao TimelineSlider, PreviewMouseDown
    ///   thong bao cho ViewModel tam ngung bo dem DispatcherTimer, giup thanh truot khong bi nhay nguoc ve vi tri cu khi dang keo.
    /// 
    /// Cach thuc van hanh:
    /// - DataContextChanged lang nghe khi DataContext duoc gan NowPlayingViewModel.
    /// - UpdateAnimationState phan tich vm.PlaybackState va vm.IsSpinEnabled de ra lenh cho _spinStoryboard.
    /// </summary>
    public partial class NowPlayingCardView : UserControl
    {
        private Storyboard _spinStoryboard;
        private bool _isStoryboardActive = false;

        /// <summary>
        /// Khoi tao UserControl va nap Storyboard hoat hoa dia than tu Resources.
        /// </summary>
        public NowPlayingCardView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += (s, e) => _spinStoryboard = (Storyboard)Resources["VinylSpinStoryboard"];
        }

        /// <summary>
        /// Xu ly khi DataContext thay doi: huy dang ky ViewModel cu va dang ky theo doi ViewModel moi.
        /// </summary>
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

        /// <summary>
        /// Theo doi su thay doi thuoc tinh tu ViewModel de cap nhat trang thai hoat hoa Storyboard.
        /// </summary>
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

        /// <summary>
        /// Cap nhat trang thai hoat hoa dia than dua tren may trang thai PlaybackState va cau hinh IsSpinEnabled.
        /// </summary>
        private void UpdateAnimationState(NowPlayingViewModel vm)
        {
            if (_spinStoryboard == null)
            {
                _spinStoryboard = (Storyboard)Resources["VinylSpinStoryboard"];
            }

            if (_spinStoryboard == null || vm == null) return;

            // Neu nguoi dung tat hieu ung xoay dia
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

            // Dieu khien Storyboard dua theo trang thai phat am thanh
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

        /// <summary>
        /// Xu ly khi nguoi dung an chuot bat dau keo thanh Slider thoi gian: tam dung bo dem timer cap nhat vi tri.
        /// </summary>
        private void TimelineSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is NowPlayingViewModel vm)
            {
                vm.SetUserSeeking(true);
            }
        }

        /// <summary>
        /// Xu ly khi nguoi dung nha chuot ket thuc thao tac tua nhac: thuc hien lenh tua den moc thoi gian moi.
        /// </summary>
        private void TimelineSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is NowPlayingViewModel vm)
            {
                vm.SetUserSeeking(false, TimelineSlider.Value);
            }
        }
    }
}
