using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MusicApp.Core.Models
{
    /// <summary>
    /// Danh mục đường cong Preset chuẩn cho bộ cân bằng âm thanh DSP Equalizer 10 băng tần.
    /// Là Nguồn Chân Lý Duy Nhất (Single Source of Truth) chia sẻ giữa ViewModel và Database Seeder.
    /// </summary>
    public static class EqPresetCatalog
    {
        public static readonly string[] BandLabels = new string[]
        {
            "32Hz", "64Hz", "125Hz", "250Hz", "500Hz", "1kHz", "2kHz", "4kHz", "8kHz", "16kHz"
        };

        public class PresetDefinition
        {
            public string Name { get; }
            public float[] Gains { get; }

            public PresetDefinition(string name, float[] gains)
            {
                Name = name;
                Gains = gains;
            }

            public string GainsJson => "[" + string.Join(",", Gains.Select(g => g.ToString("0.0", CultureInfo.InvariantCulture))) + "]";
        }

        private static readonly Dictionary<string, PresetDefinition> _presets =
            new Dictionary<string, PresetDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                { "Flat", new PresetDefinition("Flat", new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }) },
                { "Rock", new PresetDefinition("Rock", new float[] { 4.5f, 3.5f, 2.0f, -1.0f, -1.5f, 1.0f, 2.5f, 3.5f, 4.5f, 4.5f }) },
                { "Pop", new PresetDefinition("Pop", new float[] { -1.5f, -1.0f, 1.0f, 2.5f, 3.5f, 3.0f, 1.5f, 0.5f, -0.5f, -1.0f }) },
                { "Jazz", new PresetDefinition("Jazz", new float[] { 3.0f, 2.5f, 1.0f, 1.5f, -1.0f, -1.0f, 0.0f, 1.5f, 2.5f, 3.0f }) },
                { "Classical", new PresetDefinition("Classical", new float[] { 4.0f, 3.5f, 3.0f, 2.0f, -1.5f, -1.5f, 0.0f, 2.0f, 3.0f, 3.5f }) },
                { "Bass Boost", new PresetDefinition("Bass Boost", new float[] { 7.0f, 6.0f, 5.0f, 3.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }) },
                { "Vocal Boost", new PresetDefinition("Vocal Boost", new float[] { -2.0f, -2.0f, -1.0f, 1.5f, 3.5f, 3.5f, 2.5f, 1.0f, 0.0f, -1.0f }) },
                { "Treble Boost", new PresetDefinition("Treble Boost", new float[] { -2.0f, -2.0f, -1.5f, -1.0f, 0.0f, 1.5f, 3.0f, 4.5f, 5.5f, 6.0f }) }
            };

        public static IEnumerable<PresetDefinition> AllPresets => _presets.Values;

        public static bool TryGetGains(string presetName, out float[] gains)
        {
            if (!string.IsNullOrEmpty(presetName) && _presets.TryGetValue(presetName, out var def))
            {
                gains = (float[])def.Gains.Clone();
                return true;
            }
            gains = null;
            return false;
        }

        public static float[] GetGainsOrDefault(string presetName)
        {
            if (TryGetGains(presetName, out var gains))
            {
                return gains;
            }
            return new float[10];
        }

        public static IEnumerable<string> PresetNames => _presets.Keys;
    }
}
