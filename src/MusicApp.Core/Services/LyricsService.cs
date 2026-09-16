using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Interfaces;
using MusicApp.Core.Models;

namespace MusicApp.Core.Services
{
    /// <summary>
    /// Dich vu quan ly va dieu phoi loi bai hat (Lyrics Management Service).
    /// 
    /// Tac dung:
    /// - Cung cap phuong thuc tim kiem, nap va phan tich loi bai hat cho bat ky ban nhac nao dang phat tren he thong.
    /// - Tu dong phoi hop giua nguon file .lrc vat ly tren o cung (Offline) va danh muc loi tich hop san (Embedded Catalog).
    /// 
    /// Van de giai quyet:
    /// - Cho phep nguoi dung nghe nhac offline co the kem theo file .lrc cung thu muc (Companion File Pattern)
    ///   ma khong can cau hinh phuc tap.
    /// - Cung cap san loi bai hat dong bo cho cac ca khuc pho bien (nhac Viet Nam Trinh Cong Son, nhac Jamendo Creative Commons)
    ///   giup tinh nang cuon loi luon hoat dong ngay ca khi khong co ket noi Internet.
    /// 
    /// Cach thuc van hanh:
    /// - Khi nhan duoc TrackModel, tien hanh kiem tra 3 ung vien duong dan file .lrc tren o cung:
    ///   1. File cung ten thay doi phan mo rong thanh .lrc.
    ///   2. File trong thu muc co ten tap tin goc + .lrc.
    ///   3. File trong thu muc co ten trung voi tieu de bai hat + .lrc.
    /// - Neu khong tim thay file dia phuong hoac day la ban nhac stream online, tra cuu trong tu dien embedded theo Track ID hoac Title.
    /// - Su dung LrcParser de chuyen doi noi dung van ban thanh danh sach LyricLine dong bo thoi gian.
    /// </summary>
    public class LyricsService : ILyricsService
    {
        private readonly LrcParser _parser = new LrcParser();
        private readonly Dictionary<string, string> _embeddedLyrics = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Khoi tao dich vu va nap danh muc loi bai hat tich hop san trong bo nho.
        /// </summary>
        public LyricsService()
        {
            InitializeEmbeddedCatalogLyrics();
        }

        /// <summary>
        /// Phan tich truc tiep chuoi noi dung LRC thanh danh sach dong loi.
        /// </summary>
        /// <param name="lrcContent">Noi dung van ban LRC.</param>
        /// <returns>Danh sach cac dong LyricLine da duoc sap xep.</returns>
        public IReadOnlyList<LyricLine> ParseLrc(string lrcContent)
        {
            return _parser.Parse(lrcContent);
        }

        /// <summary>
        /// Nap loi bai hat tu dong cho ban nhac chi dinh.
        /// </summary>
        /// <param name="track">Ban nhac can hien thi loi.</param>
        /// <param name="cancellationToken">Token huy tac vu doc bat dong bo.</param>
        /// <returns>Danh sach cac dong loi dong bo thoi gian.</returns>
        public async Task<IReadOnlyList<LyricLine>> LoadLyricsForTrackAsync(
            TrackModel track, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (track == null)
            {
                return new List<LyricLine>();
            }

            // 1. Kiem tra xem co tap tin .lrc di kem tren o dia cuc bo hay khong
            if (!string.IsNullOrWhiteSpace(track.StreamUrl) && File.Exists(track.StreamUrl))
            {
                try
                {
                    string dir = Path.GetDirectoryName(track.StreamUrl);
                    string nameNoExt = Path.GetFileNameWithoutExtension(track.StreamUrl);

                    // Thu nghiem 3 quy tac dat ten file .lrc pho bien
                    string candidate1 = Path.ChangeExtension(track.StreamUrl, ".lrc");
                    string candidate2 = Path.Combine(dir, nameNoExt + ".lrc");
                    string candidate3 = Path.Combine(dir, track.Title + ".lrc");

                    string targetLrcPath = null;
                    if (File.Exists(candidate1)) targetLrcPath = candidate1;
                    else if (File.Exists(candidate2)) targetLrcPath = candidate2;
                    else if (File.Exists(candidate3)) targetLrcPath = candidate3;

                    if (targetLrcPath != null)
                    {
                        using (var reader = new StreamReader(targetLrcPath))
                        {
                            string content = await reader.ReadToEndAsync().ConfigureAwait(false);
                            return _parser.Parse(content);
                        }
                    }
                }
                catch (Exception)
                {
                    // Bo qua ngoai le ve quyen truy cap file hoac file lock, chuyen xuong co che tiep theo
                }
            }

            // 2. Tra cuu trong danh muc loi tich hop san theo Track ID hoac Tieu de bai hat
            if (!string.IsNullOrWhiteSpace(track.Id) && _embeddedLyrics.ContainsKey(track.Id))
            {
                return _parser.Parse(_embeddedLyrics[track.Id]);
            }

            if (!string.IsNullOrWhiteSpace(track.Title) && _embeddedLyrics.ContainsKey(track.Title))
            {
                return _parser.Parse(_embeddedLyrics[track.Title]);
            }

            return new List<LyricLine>();
        }

        /// <summary>
        /// Khoi tao danh muc loi bai hat mac dinh duoc tich hop san vao ma nguon ung dung.
        /// </summary>
        private void InitializeEmbeddedCatalogLyrics()
        {
            // Diem Xua (vn_track_01)
            _embeddedLyrics["vn_track_01"] = 
@"[00:00.00]Diễm Xưa - Kim Tuấn (Hòa Tấu Guitar)
[00:08.50]Sáng tác: Trịnh Công Sơn
[00:15.00]Mưa vẫn mưa bay trên tầng tháp cổ
[00:23.50]Dài tay em mấy thuở mắt xanh xao
[00:32.00]Nghe thu mưa rơi suối mềm dạt dào
[00:40.00]Nửa đêm nghe lá hát ru bên đời
[00:48.50]Mưa vẫn hay mưa trên hàng lá nhỏ
[00:57.00]Buổi chiều ngồi ngóng những chuyến mưa qua
[01:05.50]Trên bước chân em âm thầm lá đổ
[01:14.00]Chợt hồn xanh buốt suốt cơn đau vùi
[01:23.00]Mưa vẫn hay mưa cho đời biển động
[01:31.50]Làm sao em nhớ những vết chim di
[01:40.00]Xin hãy cho mưa qua miền đất rộng
[01:48.50]Để người phiêu lãng quên mình lãng du
[01:57.00]Ngày sau sỏi đá cũng cần có nhau
[02:08.00]Giai điệu guitar êm đềm...
[02:30.00]Mưa vẫn mưa bay trên tầng tháp cổ
[02:40.00]Dài tay em mấy thuở mắt xanh xao
[02:50.00]Xin hãy cho mưa qua miền đất rộng
[03:00.00]Ngày sau sỏi đá cũng cần có nhau...";

            // Ha Trang (vn_track_02)
            _embeddedLyrics["vn_track_02"] = 
@"[00:00.00]Hạ Trắng - Kim Tuấn (Hòa Tấu Guitar)
[00:10.00]Sáng tác: Trịnh Công Sơn
[00:18.00]Gọi nắng trên vai em gầy đường xa áo bay
[00:27.00]Nắng qua mắt buồn lòng hoa bướm say
[00:36.00]Lối em đi về trời không có mây
[00:45.00]Đường đi suốt mùa nắng đong đầy
[00:54.00]Áo xưa dù nhàu cũng xin gọi tên
[01:03.00]Cho một lần nhìn thấy nhau trong đời
[01:12.00]Cho nhau lời hẹn ngút ngàn trùng xa
[01:21.00]Đôi môi còn nồng khúc ca ngọt ngào...";

            // Con Tuoi Nao Cho Em (vn_track_03)
            _embeddedLyrics["vn_track_03"] = 
@"[00:00.00]Còn Tuổi Nào Cho Em - Kim Tuấn
[00:10.00]Sáng tác: Trịnh Công Sơn
[00:20.00]Tuổi nào nhìn thoáng như làn mây bay
[00:29.00]Tay măng trôi trên vùng tóc dài
[00:38.00]Bao nhiêu năm rồi làm kiếp con người
[00:47.00]Chợt một chiều tóc trắng như vôi...";

            // Tokyo Neon Night (jamendo_track_1855932)
            _embeddedLyrics["jamendo_track_1855932"] = 
@"[00:00.00]Tokyo Neon Night - Future Retro
[00:12.00]Driving through the glowing city lights
[00:24.00]Shinjuku highways shining in the night
[00:36.00]Digital dreams in a cybernetic sound
[00:48.00]Neon reflections on the wet ground
[01:00.00]Feel the analog pulse of the bassline
[01:15.00]Lost in the echoes of space and time...";
        }
    }
}
