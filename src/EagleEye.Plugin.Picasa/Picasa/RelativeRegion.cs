namespace EagleEye.Picasa.Picasa
{
    /// <summary>
    /// From Relative to absolute region, multiply the left and right with the width of the picture, and the top and bottom with the height.
    /// </summary>
    public struct RelativeRegion
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
    }
}
