namespace MusicApp.Core.Dtos
{
    /// <summary>
    /// Doi tuong truyen tai du lieu bai hat (Data Transfer Object) qua mang HTTP.
    /// 
    /// Tac dung:
    /// - Dinh nghia hop dong du lieu mang (Network Contract) giua may chu BFF va Client giao dien WPF.
    /// - Mang day du thong tin can thiet de phat truc tuyen, bao gom endpoint stream duoc bao ve va anh bia.
    /// 
    /// Van de giai quyet:
    /// - An giau dia chi goc (Direct CDN URL) cua nha cung cap am thanh nham phong ngua rui ro bi chan CORS,
    ///   thay doi dia chi hoac loi dinh dang token xac thuc.
    /// - Cho phep BFF thuc hien proxying, chuyen tiep am thanh va ho tro cac header phuc tap nhu HTTP 206 Partial Content.
    /// 
    /// Cach thuc van hanh:
    /// - TrackController tra ve TrackDto chua StreamEndpoint tro ve chinh may chu BFF noi bo (vi du: /api/tracks/stream/{id}).
    /// - Client su dung endpoint nay khoi tao BufferedHttpWaveStream de lay am thanh theo dang chunk nho giup tiet kiem bang thong.
    /// </summary>
    public class TrackDto
    {
        /// <summary>
        /// Dinh danh duy nhat cua bai hat tai nha cung cap.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Tieu de bai hat.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Ten ca si hoac nhom nhac the hien.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Ten album phat hanh.
        /// </summary>
        public string Album { get; set; }

        /// <summary>
        /// Thoi luong bai hat tinh bang giay.
        /// </summary>
        public int DurationSeconds { get; set; }

        /// <summary>
        /// Duong dan URL den anh bia album hoac anh dai dien.
        /// </summary>
        public string CoverImageUrl { get; set; }

        /// <summary>
        /// Endpoint HTTP tren server BFF de lay luong am thanh stream.
        /// </summary>
        public string StreamEndpoint { get; set; }

        /// <summary>
        /// Thong tin ban quyen / giay phep phat hanh.
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// The loai am nhac.
        /// </summary>
        public string Genre { get; set; }
    }
}
