namespace EagleEye.Picasa.Picasa
{
    using System;

    /// <summary>
    /// From Relative to absolute region, multiply the left and right with the width of the picture, and the top and bottom with the height.
    /// </summary>
    public readonly struct RelativeRegion : IEquatable<RelativeRegion>
    {
        public RelativeRegion(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public float Left { get; }

        public float Top { get; }

        public float Right { get; }

        public float Bottom { get; }

        public bool Equals(RelativeRegion other)
        {
            return Left.Equals(other.Left) &&
                   Top.Equals(other.Top) &&
                   Right.Equals(other.Right) &&
                   Bottom.Equals(other.Bottom);
        }

        public override bool Equals(object obj)
        {
            return obj is RelativeRegion other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Left.GetHashCode();
                hashCode = (hashCode * 397) ^ Top.GetHashCode();
                hashCode = (hashCode * 397) ^ Right.GetHashCode();
                hashCode = (hashCode * 397) ^ Bottom.GetHashCode();
                return hashCode;
            }
        }
    }
}
