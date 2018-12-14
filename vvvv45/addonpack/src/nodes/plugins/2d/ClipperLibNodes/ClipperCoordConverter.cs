
#region usings
using System;

using ClipperLib;
using System.Drawing;
using VVVV.Utils.VMath;

#endregion usings

namespace ClipperHelper
{
    public static class CoordConverter
    {
        public const int CONVERSION_SCALE = 65536;

        public static double ToClipper(double value)
        {
            return value * CONVERSION_SCALE;
        }

        // Rename to ToIntPoint or something?
        /// <summary>
        /// Convert Vector2D to Clipper point (Scaled IntPoint)
        /// </summary>
        /// <param name="v">VMath.Vector2D</param>
        /// <returns>IntPoint with coordinates scaled by conversion factor</returns>
        public static IntPoint ToClipperPoint(Vector2D v)
        {
            return new IntPoint(v.x * CONVERSION_SCALE, v.y * CONVERSION_SCALE);
        }

        public static IntPoint ToClipper( PointF pt)
        {
            return new IntPoint(pt.X * CONVERSION_SCALE, pt.Y * CONVERSION_SCALE);
        }

        public static double FromClipper(double cCoord)
        {
            return cCoord / (double)CONVERSION_SCALE;
        }

        public static float FromClipper(float cCoord)
        {
            return cCoord / (float)CONVERSION_SCALE;
        }

        public static double FromClipperArea(double area)
        {
            //TODO: verify and express better
            double a = area * FromClipperArea(1) * FromClipperArea(1);
            return a;
        }
    }
}