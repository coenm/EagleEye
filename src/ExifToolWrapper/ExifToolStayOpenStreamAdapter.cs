using System;
using System.IO;
using System.Text;

namespace ExifToolWrapper
{
    public class ExifToolStayOpenStreamAdapter : Stream
    {
        private readonly Encoding _encoding;
        private readonly byte[] _cache;
        private const int OneMb = 1024 * 1024;
        private int _index;
//        private TaskCompletionSource<string> _tcs;

        public ExifToolStayOpenStreamAdapter(Encoding encoding)
        {
            _encoding = encoding ?? new UTF8Encoding();
            _cache = new byte[OneMb];
            _index = 0;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                return;
            if (count == 0)
                return;
            if (offset + count > buffer.Length)
                return;

            if (count > OneMb - _index)
                throw new ArgumentOutOfRangeException();

            for (var i = offset; i < offset+count; i++)
            {
                _cache[_index] = buffer[i];
                _index++;
            }

            // NOT FINAL, NOT SAFE, NEEDS TESTS
            var s = _encoding.GetString(_cache, 0, _index - 1);

            var prefix = "\r\n{ready";
            var index2 = s.IndexOf(prefix);
            while (index2 > -1)
            {
                _index = 0; // this is not good!!
                var data = s.Substring(0, index2);
                var key = "unknown";

                s = s.Substring(index2 + prefix.Length);
                var closeBracket = s.IndexOf("}");
                if (closeBracket >= 0 && closeBracket < 10)
                {
                    key = s.Substring(0, closeBracket);
                    s = s.Substring(closeBracket+1);
                }
                while (s.Length > 0 && (s[0] == '\r' || s[0] == '\n'))
                    s= s.Substring(1);

                Update(this, new DataCapturedArgs(key, data));

                index2 = s.IndexOf(prefix);
            }
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _index;

        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public event EventHandler<DataCapturedArgs> Update = delegate { };

//        public async Task<string> StartCapture(int sequenceNumber)
//        {
//            _tcs = new TaskCompletionSource<string>();
//            _sb.Clear();
//
//            var result = await _tcs.Task;
//            return result;
//        }
    }

    public class DataCapturedArgs : EventArgs
    {
        public DataCapturedArgs(string key, string data)
        {
            Key = key;
            Data = data;
        }

        public string Key { get; }

        public string Data { get; }
    }
}