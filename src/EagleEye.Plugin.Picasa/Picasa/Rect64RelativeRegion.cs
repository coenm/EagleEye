namespace EagleEye.Picasa.Picasa
{
    using System;
    using System.Linq;

    /// <summary>
    /// From Relative to absolute region, multiply the left and right with the width of the picture, and the top and bottom with the height.
    /// </summary>
    public readonly struct Rect64RelativeRegion : IEquatable<Rect64RelativeRegion>
    {
        public Rect64RelativeRegion(string rect64Region)
        {
            var result = DecodeRect64ToRelativeCoordinates(ref rect64Region);
            if (result.HasValue == false)
                throw new ArgumentException($"Cannot decode {rect64Region} as a relative region.");

            Rect64 = rect64Region;
            Left = result.Value.left;
            Top = result.Value.top;
            Right = result.Value.right;
            Bottom = result.Value.bottom;
        }

        public string Rect64 { get; }

        public float Left { get; }

        public float Top { get; }

        public float Right { get; }

        public float Bottom { get; }

        public bool Equals(Rect64RelativeRegion other)
        {
            // Other properties are calculated from Rect64
            return Rect64.Equals(other.Rect64);
        }

        public override bool Equals(object obj)
        {
            return obj is Rect64RelativeRegion other && Equals(other);
        }

        public override int GetHashCode()
        {
            // Other properties are calculated from Rect64.
            return Rect64.GetHashCode();
        }

        private static (float left, float top, float right, float bottom)? DecodeRect64ToRelativeCoordinates(ref string rect64)
        {
            const int rect64StringLength = 7; // length of "rect64("
            const int minLength = rect64StringLength + 1 + 1;
            const int maxLength = rect64StringLength + 16 + 1;
            if (rect64 == null)
                return null;
            if (rect64.Length < minLength)
                return null;
            if (rect64.Length > maxLength)
                return null;
            if (!rect64.StartsWith("rect64("))
                return null;
            if (!rect64.EndsWith(")"))
                return null;

            if (rect64.Length == maxLength)
                return DecomposeStringToRectangle(ref rect64, rect64StringLength);

            var data = new string('0', maxLength - rect64.Length) + rect64.Substring(rect64StringLength);
            return DecomposeStringToRectangle(ref data, 0);
        }

        private static (float left, float top, float right, float bottom)? DecomposeStringToRectangle(ref string rect64, int startIndex)
        {
            var left = FromString(ref rect64, 0 + startIndex);
            var top = FromString(ref rect64, 4 + startIndex);
            var right = FromString(ref rect64, 8 + startIndex);
            var bottom = FromString(ref rect64, 12 + startIndex);

            var relativeLeft = (float) left / (float) ushort.MaxValue;
            var relativeTop = (float) top / (float) ushort.MaxValue;
            var relativeRight = (float) right / (float) ushort.MaxValue;
            var relativeBottom = (float) bottom / (float) ushort.MaxValue;

            return (relativeLeft, relativeTop, relativeRight, relativeBottom);
        }

        private static ushort FromString(ref string input, int startIndex)
        {
            var bytes = StringToByteArray(input.Substring(startIndex, 4));

            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();

            return BitConverter.ToUInt16(bytes, 0);
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
