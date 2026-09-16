using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces
{
    /// <summary>
    /// Bao cao tien do quet thu muc am thanh cuc bo (Scan Progress Report DTO).
    /// 
    /// Tac dung:
    /// - Mang thong tin tien do theo thoi gian thuc tu background thread sang UI thread:
    ///   so file da quet, so ban nhac da nhan dien va ten file hien tai dang duoc xu ly.
    /// </summary>
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
    /// Giao dien dich vu quet va quan ly thu vien nhac offline cuc bo (Local Library Service Interface).
    /// 
    /// Tac dung:
    /// - Cung cap phuong thuc quet de quy cac thu muc tren o dia nguoi dung de tim kiem tap tin am thanh.
    /// - Cung cap phuong thuc boc tach sieu du lieu (ID3 Metadata) tu tap tin am thanh don le.
    /// 
    /// Van de giai quyet:
    /// - Xu ly quet thu muc tren luong nen (Background Worker), khong gay treo hoac giat lag giao dien WPF.
    /// - Co lap toan bo cac loi phan quyen truy cap (UnauthorizedAccessException), duong dan qua dai (PathTooLongException),
    ///   hoac file bi khoa boi tien trinh khac, dam bao qua trinh quet khong bi crash giua chung.
    /// 
    /// Cach thuc van hanh:
    /// - Duoc thuc thi boi LocalLibraryService su dung thu vien TagLibSharp va thuat toan duyet theo chieu rong BFS an toan.
    /// </summary>
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
