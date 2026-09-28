using System;
using System.Collections.Generic;

namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thực thể đại diện cho danh sách phát cá nhân của người dùng (Playlist).
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Chứa metadata của Playlist.
    /// KHÔNG chịu trách nhiệm: Thực thi thêm/xóa bài hát vật lý.
    /// Vòng đời: Tồn tại trong bộ nhớ khi được query.
    /// Ràng buộc: Name không được null hay rỗng.
    /// </remarks>
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
