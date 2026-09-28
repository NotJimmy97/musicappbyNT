using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces
{
    /// <summary>
    /// Báo cáo tiến độ quét thư mục âm thanh cục bộ.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Mang thông tin tiến độ theo thời gian thực.
    /// KHÔNG chịu trách nhiệm: Thực thi logic quét.
    /// Vòng đời: Transient, tạo liên tục trong quá trình quét.
    /// Luồng: Được tạo ở background thread và gửi sang UI thread.
    /// </remarks>
    public class ScanProgressReport
    {
        /// <summary>
        /// Tong so file am thanh da duoc duyet qua.
        /// </summary>
        public int FilesScanned { get; set; }

        /// <summary>
        /// So luong ban nhac da duoc doc tag ID3 va them vao danh sach thanh cong.
        /// </summary>
        public int TracksFound { get; set; }

        /// <summary>
        /// Ten tap tin hien tai dang duoc he thong xu ly doc metadata.
        /// </summary>
        public string CurrentFile { get; set; }
    }

    /// <summary>
    /// Giao diện dịch vụ quét và quản lý thư viện nhạc offline cục bộ.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Quét đệ quy thư mục và bóc tách siêu dữ liệu (ID3 Metadata).
    /// KHÔNG chịu trách nhiệm: Quản lý playlist hay tương tác với Database.
    /// Vòng đời: Scoped hoặc Singleton.
    /// Luồng: Chạy quét trên luồng nền (Background Worker) để không gây treo giao diện WPF, xử lý biệt lập các lỗi hệ thống tệp.
    /// </remarks>
    public interface ILocalLibraryService
    {
        /// <summary>
        /// Quet toan bo tap tin am thanh trong thu muc chi dinh mot cach bat dong bo.
        /// </summary>
        /// <param name="directoryPath">Duong dan thu muc goc can quet tren he thong file.</param>
        /// <param name="progress">Doi tuong IProgress dung de phat bao cao tien do cap nhat giao dien.</param>
        /// <param name="cancellationToken">Token cho phep nguoi dung huy bo tien trinh quet.</param>
        /// <returns>Danh sach cac doi tuong TrackModel duoc doc va khoi tao thanh cong.</returns>
        Task<IReadOnlyList<TrackModel>> ScanDirectoryAsync(
            string directoryPath, 
            IProgress<ScanProgressReport> progress = null, 
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Trich xuat metadata (Title, Artist, Album, Duration, Cover Art) tu mot tap tin am thanh cu the.
        /// </summary>
        /// <param name="filePath">Duong dan day du toi tap tin am thanh tren o cung.</param>
        /// <returns>Doi tuong TrackModel hop le, hoac null neu dinh dang file khong duoc ho tro hoac loi.</returns>
        TrackModel ExtractTrackFromFile(string filePath);
    }
}
