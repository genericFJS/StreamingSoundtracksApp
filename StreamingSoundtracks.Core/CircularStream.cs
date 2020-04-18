using System;
using System.ComponentModel;
using System.IO;

namespace StreamingSoundtracks.Core
{
    public class CircularStream : Stream, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return true; } }
        private long length;
        public override long Length { get { return length; } }
        private long position;
        public override long Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReadPosition)));
            }
        }
        public long ReadPosition
        {
            get { return Position % BufferLength; }
        }
        public long WritePosition
        {
            get { return Length % BufferLength; }
        }
        public override void Flush() { throw new NotImplementedException(); }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
        public override void SetLength(long value) { throw new NotImplementedException(); }
        private void UpdateLength(long value)
        {
            length = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Length)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WritePosition)));
        }

        private byte[] ByteBuffer { get; }
        public int BufferLength { get { return ByteBuffer.Length; } }

        public CircularStream(int bufferSize)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            UpdateLength(0);
            Position = 0;
            ByteBuffer = new byte[bufferSize];
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (count < 0 || count > BufferLength)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (offset < 0 || buffer.Length - offset < count)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (Position >= Length)
                return 0;

            var countRead = 0;
            while (count > 0)
            {
                var readPosition = (int)ReadPosition;
                var byteCountToRead = BufferLength - readPosition;
                if (count < byteCountToRead)
                    byteCountToRead = count;
                if (byteCountToRead > Length - Position)
                    byteCountToRead = (int)(Length - Position);
                Buffer.BlockCopy(ByteBuffer, readPosition, buffer, offset, byteCountToRead);

                count -= byteCountToRead;
                countRead += byteCountToRead;
                offset += byteCountToRead;
                Position += byteCountToRead;

                if (Position >= Length)
                    return countRead;
            }
            return countRead;

            //var bytesToEnd = Length - Position;
            //var countFromPosition = bytesToEnd >= count ? count : bytesToEnd;
            //var countFromStart = bytesToEnd >= count ? 0 : count - countFromPosition;
            //Buffer.BlockCopy(ByteBuffer, (int)Position, buffer, offset, (int)countFromPosition);
            //if (countFromStart > 0)
            //    Buffer.BlockCopy(ByteBuffer, 0, buffer, offset + (int)countFromPosition, (int)countFromStart);
            //Position += count;
            //return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (offset < 0 || buffer.Length - offset < count)
                throw new ArgumentOutOfRangeException(nameof(offset));

            while (count > 0)
            {
                var writePosition = (int)WritePosition;
                var byteCountToWrite = BufferLength - writePosition;
                if (count < byteCountToWrite)
                    byteCountToWrite = count;

                Buffer.BlockCopy(buffer, offset, ByteBuffer, writePosition, byteCountToWrite);
                count -= byteCountToWrite;
                offset += byteCountToWrite;
                UpdateLength(Length + byteCountToWrite);
                //Trace.WriteLine($"{Length} / {BufferLength}");
            }
            //Trace.WriteLine($"{Length} / {BufferLength}");
            //Trace.WriteLine($"==========================");
            //var countFromWritePosition = bytesToEnd >= count ? count : bytesToEnd;
            //var countFromStart = bytesToEnd >= count ? 0 : count - countFromWritePosition;
            //Buffer.BlockCopy(buffer, offset, ByteBuffer, (int)WritePosition, (int)countFromWritePosition);
            //if (countFromStart > 0)
            //    Buffer.BlockCopy(buffer, offset + (int)countFromWritePosition, ByteBuffer, 0, (int)countFromStart);

            //WritePosition = (WritePosition + count) % Length;
        }
    }
}
