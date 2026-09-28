namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thuc the quan ly cac thu muc da duoc nguoi dung quet tren may.
    /// </summary>
    public class ScannedFolderEntity
    {
        public string FolderPath { get; set; }
        public string LastScannedAt { get; set; }
        public int TotalFiles { get; set; }
    }
}
