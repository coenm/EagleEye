﻿namespace EagleEye.Picasa.Picasa
{
    public class RelativeRegion
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
