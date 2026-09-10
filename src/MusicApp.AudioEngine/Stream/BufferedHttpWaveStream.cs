using System;
using System.IO;
using NAudio.Wave;

namespace MusicApp.AudioEngine.Stream
{
    public class BufferedHttpWaveStream : WaveStream
    {
        private readonly WaveStream _sourceStream;
        private readonly BufferedWaveProvider _bufferedWaveProvider;

        public BufferedHttpWaveStream(WaveStream sourceStream, int bufferDurationSeconds = 5)
        {
            _sourceStream = sourceStream ?? throw new ArgumentNullException(nameof(sourceStream));
            _bufferedWaveProvider = new BufferedWaveProvider(sourceStream.WaveFormat)
            {
                BufferDuration = TimeSpan.FromSeconds(bufferDurationSeconds),
                DiscardOnBufferOverflow = true
            };
        }

        public override WaveFormat WaveFormat => _sourceStream.WaveFormat;

        public override long Length => _sourceStream.Length;

        public override long Position
        {
            get => _sourceStream.Position;
            set => _sourceStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _sourceStream.Read(buffer, offset, count);
        }

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

