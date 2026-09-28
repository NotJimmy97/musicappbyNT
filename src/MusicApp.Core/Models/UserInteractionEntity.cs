namespace MusicApp.Core.Models
{
    /// <summary>
    /// Nhat ky ghi nhan hanh vi tuong tac cua nguoi dung phuc vu mo hinh hoc so thich ngam (Implicit Feedback Matrix).
    /// ActionType: 'click' | 'play_start' | 'play_complete' | 'skip' | 'favorite' | 'unfavorite' | 'add_playlist'
    /// </summary>
    public class UserInteractionEntity
    {
        public int Id { get; set; }
        public int TrackId { get; set; }
        public string ActionType { get; set; }
        public int DurationPlayedSeconds { get; set; }
        public string CreatedAt { get; set; }
    }
}
