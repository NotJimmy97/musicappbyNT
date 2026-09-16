namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thuc the mieu ta thong tin mot bai hat trong mien nghiep vu (Core Domain Entity).
    /// 
    /// Tac dung:
    /// - Luu tru day du sieu du lieu (Metadata) cua mot ban nhac, bao gom tieu de, nghe si, thoi luong, anh bia va duong dan phat.
    /// - Duoc su dung lam du lieu chuan trao doi giua cac module: Local Library Scanner, BFF Service, Audio Engine va UI Queue.
    /// 
    /// Van de giai quyet:
    /// - Thong nhat cau truc ban ghi giua nguon nhac Online (Jamendo API, Cloud Stream) va nguon nhac Offline (Local File ID3 tag).
    /// - Dong nhat hoa giao dien phat nhac, giup Audio Engine chi can tiep nhan StreamUrl ma khong can quan tam nguon goc du lieu.
    /// 
    /// Cach thuc van hanh:
    /// - Duoc tao ra boi LocalLibraryService khi doc file tren o cung hoac duoc chuyen doi tu TrackDto khi nhan ket qua tu BFF.
    /// - Duoc luu tru trong danh sach phat (PlayQueueViewModel) va gan vao NowPlayingViewModel de hien thi thong tin hien tai.
    /// </summary>
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
