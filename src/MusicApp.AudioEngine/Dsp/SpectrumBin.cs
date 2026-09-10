namespace MusicApp.AudioEngine.Dsp
{
    public struct SpectrumBin
    {
        public int Index { get; set; }
        public float FrequencyHz { get; set; }
        public float Value { get; set; }

        public SpectrumBin(int index, float frequencyHz, float value)
        {
            Index = index;
            FrequencyHz = frequencyHz;
            Value = value;
        }
    }
}
