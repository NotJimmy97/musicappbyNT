namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thuc the luu tru cac preset can bang am thanh 10 bang tan (EQ Presets).
    /// </summary>
    public class EqPresetEntity
    {
        public string Name { get; set; }
        public string GainsJson { get; set; }
        public bool IsCustom { get; set; }
    }

    /// <summary>
    /// Thuc the luu tru trang thai phien lam viec de phuc hoi khi mo lai app (Session Resume).
    /// </summary>
    public class AppSessionStateEntity
    {
        public int Id { get; set; } = 1;
        public int? LastTrackId { get; set; }
        public double LastPositionSeconds { get; set; }
        public double Volume { get; set; } = 1.0;
        public bool IsMuted { get; set; }
        public int RepeatMode { get; set; }
        public bool IsShuffleEnabled { get; set; }
    }
}
