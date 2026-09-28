using System;
using System.Collections.Generic;

namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thuc the dai dien cho Danh sach phat ca nhan cua nguoi dung (Custom Playlist).
    /// </summary>
    public class PlaylistEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CoverUri { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }

        /// <summary>
        /// Danh sach cac bai hat thuoc playlist nay (da duoc sap xep theo thu tu position).
        /// </summary>
        public List<TrackEntity> Tracks { get; set; } = new List<TrackEntity>();
    }
}
