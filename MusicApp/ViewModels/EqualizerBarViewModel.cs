using MusicApp.Core.Common;

namespace MusicApp.ViewModels
{
    public class EqualizerBarViewModel : ObservableObject
    {
        private double _value;
        public double Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private string _colorHex;
        public string ColorHex
        {
            get => _colorHex;
            set => SetProperty(ref _colorHex, value);
        }

        public EqualizerBarViewModel(double initialValue, string colorHex)
        {
            _value = initialValue;
            _colorHex = colorHex;
        }
    }
}
