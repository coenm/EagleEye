namespace EagleEye.ExifToolWrapper.ExifTool
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public class ExifToolStayOpenStream : Stream
    {
        private const int ONE_MB = 1024 * 1024;
        private const string PREFIX = "\r\n{ready";
        private const string SUFFIX = "}\r\n";
        private readonly Encoding _encoding;
        private readonly byte[] _cache;
        private readonly byte[] _endOfMessageSequenceStart;
        private readonly byte[] _endOfMessageSequenceEnd;
        private readonly int _buferSize;
        private int _index;

        public ExifToolStayOpenStream(Encoding encoding, int bufferSize = ONE_MB)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            _buferSize = bufferSize;
            _encoding = encoding ?? new UTF8Encoding();
            _cache = new byte[_buferSize];
            _index = 0;
            _endOfMessageSequenceStart = _encoding.GetBytes(PREFIX);
            _endOfMessageSequenceEnd = _encoding.GetBytes(SUFFIX);
        }

        public event EventHandler<DataCapturedArgs> Update = delegate { };

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _index;

        public override long Position
        {
            get => _index;
            set
            {
                // do nothing but also don't throw an excpetion.
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                return;
            if (count == 0)
                return;
            if (offset + count > buffer.Length)
                return;

            if (count > _buferSize - _index)
                throw new ArgumentOutOfRangeException();

            Array.Copy(buffer, 0, _cache, _index, count);
            _index += count;

            var lastEndIndex = 0;

            for (var i = 0; i < _index - 1; i++)
            {
                var j = 0;
                while (j < _endOfMessageSequenceStart.Length && _cache[i + j] == _endOfMessageSequenceStart[j])
                    j++;

                if (j != _endOfMessageSequenceStart.Length)
                    continue;

                j = j + i;

                // expect numbers as key.
                var keyStartIndex = j;
                while (j < _index && _cache[j] >= '0' && _cache[j] <= '9')
                {
                    j++;
                }

                if (keyStartIndex == j)
                {
                    // no key found.
                    continue;
                }

                var keyBytes = _cache.AsSpan().Slice(start: keyStartIndex, length: j - keyStartIndex);

                var k = 0;
                while (k < _endOfMessageSequenceEnd.Length && _cache[j + k] == _endOfMessageSequenceEnd[k])
                    k++;

                if (k != _endOfMessageSequenceEnd.Length)
                    continue;

                j += k;

                var content = _encoding.GetString(_cache, lastEndIndex, i - lastEndIndex);
                var key = _encoding.GetString(keyBytes.ToArray());
                Update(this, new DataCapturedArgs(key, content));

                i = j;
                lastEndIndex = j;
            }

            Debug.Assert(lastEndIndex <= _index, "Expect that lastEndindex is less then index");

            if (lastEndIndex == 0)
                return;

            if (_index > lastEndIndex)
                Array.Copy(_cache, lastEndIndex, _cache, 0, _index - lastEndIndex);

            _index -= lastEndIndex;
        }

        public override void Flush()
        {
            // intentionally do nothing.
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }

        public override void SetLength(long value)
        {
            // intentionally do nothing.
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException($"{CanRead} is set to false");
        }
    }
}