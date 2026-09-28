namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thực thể miêu tả thông tin một bài hát trong miền nghiệp vụ (Core Domain).
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Lưu trữ Metadata và đường dẫn phát để dùng trong ViewModel và AudioEngine.
    /// KHÔNG chịu trách nhiệm: Tương tác với Database hay thực thi API.
    /// Vòng đời: Tồn tại khi thêm vào PlayQueue hoặc NowPlaying.
    /// Ràng buộc: Id không đổi, StreamUrl có thể là FilePath cục bộ hoặc HTTP url.
    /// </remarks>
    public class TrackModel
    {
        /// <summary>
        /// Dinh danh duy nhat cua bai hat trong he thong (vi du: ID Jamendo hoac ma bam duong dan file local).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Tieu de bai hat duoc trich xuat tu ID3 tag hoac API.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Ten nghe si bieu dien hoac sang tac.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Ten album chua bai hat nay.
        /// </summary>
        public string Album { get; set; }

        /// <summary>
        /// Tong thoi luong bai hat tinh theo giay.
        /// </summary>
        public int DurationSeconds { get; set; }

        /// <summary>
        /// Duong dan URL web hoac chuoi base64 data URI cua anh bia album.
        /// </summary>
        public string CoverImageUrl { get; set; }

        /// <summary>
        /// Duong dan phat truc tiep: co the la URL HTTP/HTTPS (Online) hoac duong dan tuyet doi tren he thong file (Offline).
        /// </summary>
        public string StreamUrl { get; set; }

        /// <summary>
        /// Thong tin giay phep ban quyen (Creative Commons, Ban quyen thuong mai, hoac Offline Local Media).
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// The loai am nhac (Pop, Rock, Classical, Hoa tau guitar,...).
        /// </summary>
        public string Genre { get; set; }
    }
}
