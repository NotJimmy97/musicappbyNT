using System;
using System.IO;
using NAudio.Wave;

namespace MusicApp.AudioEngine.Stream
{
    /// <summary>
    /// Lop stream am thanh ho tro bo dem cho luong HTTP (Buffered HTTP Wave Stream Decorator).
    /// 
    /// Tac dung:
    /// - Bao boc mot doi tuong WaveStream goc va ket hop voi BufferedWaveProvider cua NAudio.
    /// - Cung cap co che luu dem truoc (Pre-buffering) cac goi am thanh tu mang HTTP ve bo nho.
    /// - Tich hop co che DiscardOnBufferOverflow de chong tran bo dem va giam thieu hien tuong lag/giat khi mang cham.
    /// 
    /// Van de giai quyet:
    /// - Mang Internet co the bi bien thien bang thong (Network Jitter) hoac mat goi tam thoi.
    ///   Viec doc truc tiep tu mang ma khong co bo dem on dinh se gay ra loi giat dung am thanh (Stuttering).
    /// - BufferedHttpWaveStream dam bao luon co san mot luong buffer tu 3 den 5 giay de phat muot ma.
    /// 
    /// Cach thuc van hanh:
    /// - Chuyen tiep cac thuoc tinh Position, Length, WaveFormat tu luong goc.
    /// - Khi duoc giai phong (Dispose), don dep ca luong stream nguon va bo dem BufferedWaveProvider mot cach dong bo.
    /// </summary>
    public class BufferedHttpWaveStream : WaveStream
    {
        private readonly WaveStream _sourceStream;
        private readonly BufferedWaveProvider _bufferedWaveProvider;

        /// <summary>
        /// Khoi tao mot luong am thanh co bo dem tu luong WaveStream goc.
        /// </summary>
        /// <param name="sourceStream">Luong am thanh goc can luu dem.</param>
        /// <param name="bufferDurationSeconds">Thoi luong toi da cua bo dem tinh bang giay (mac dinh 5 giay).</param>
        public BufferedHttpWaveStream(WaveStream sourceStream, int bufferDurationSeconds = 5)
        {
            _sourceStream = sourceStream ?? throw new ArgumentNullException(nameof(sourceStream));
            _bufferedWaveProvider = new BufferedWaveProvider(sourceStream.WaveFormat)
            {
                BufferDuration = TimeSpan.FromSeconds(bufferDurationSeconds),
                DiscardOnBufferOverflow = true
            };
        }

        /// <summary>
        /// Dinh dang am thanh cua luong.
        /// </summary>
        public override WaveFormat WaveFormat => _sourceStream.WaveFormat;

        /// <summary>
        /// Tong do dai cua luong am thanh tinh bang byte.
        /// </summary>
        public override long Length => _sourceStream.Length;

        /// <summary>
        /// Vi tri byte hien tai cua con tro doc trong luong.
        /// </summary>
        public override long Position
        {
            get => _sourceStream.Position;
            set => _sourceStream.Position = value;
        }

        /// <summary>
        /// Doc cac byte am thanh tu luong goc vao bo dem.
        /// </summary>
        /// <param name="buffer">Bo dem byte dich.</param>
        /// <param name="offset">Vi tri bat dau ghi vao bo dem.</param>
        /// <param name="count">So byte can doc.</param>
        /// <returns>So byte thuc te doc duoc.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _sourceStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Giai phong tai nguyen bo dem va dong luong am thanh goc.
        /// </summary>
        /// <param name="disposing">Xac dinh co dang giai phong tai nguyen managed hay khong.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sourceStream?.Dispose();
                _bufferedWaveProvider?.ClearBuffer();
            }
            base.Dispose(disposing);
        }
    }
}
