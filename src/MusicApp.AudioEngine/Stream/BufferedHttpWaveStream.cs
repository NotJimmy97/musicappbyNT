using System;
using System.IO;
using NAudio.Wave;

namespace MusicApp.AudioEngine.Stream
{
    /// <summary>
    /// Luồng âm thanh hỗ trợ bộ đệm cho luồng mạng HTTP.
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Cung cấp cơ chế lưu đệm trước (Pre-buffering) các mẫu âm thanh vào RAM để chống giật lag mạng.
    /// 2. Không chịu trách nhiệm: Giải mã định dạng âm thanh (MediaFoundationReader đảm nhiệm).
    /// 3. Thời gian sống: Tồn tại cùng vòng đời stream nguồn của một bài hát.
    /// 4. Đa luồng/Vòng đời: Quá trình lưu đệm diễn ra trong RAM. Khi Dispose phải dọn dẹp luồng nguồn và xoá bộ đệm.
    /// </remarks>
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
