
#region usings
using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Nodes;

using System.Drawing.Drawing2D;
using ClipperLib;
using ClipperHelper;
using System.Drawing;
using VVVV.Core.Logging;
using System.ComponentModel.Composition;

#endregion usings

namespace VVVV.Nodes
{
    using Path = List<IntPoint>;
    using Paths = List<List<IntPoint>>;

    static class Geometries
    {
        public static Path Circle(int resolution, double radius)
        {
            Path circle = new Path();
            double cycleIncrement = VMath.CycToRad / resolution;
            double cyclePos = 0.0;
            for (int pt = 0; pt < resolution; pt++)
            {
                double x = Math.Cos(cyclePos) * radius;
                double y = Math.Sin(cyclePos) * radius;
                circle.Add(new IntPoint(CoordConverter.ToClipper(x), CoordConverter.ToClipper(y)));
                cyclePos += cycleIncrement;
            }

            return circle;
        }

        public static Path Rectangle(Vector2D center, double width, double height)
        {
            double top = center.y + height / 2.0;
            double right = center.x + width / 2.0;
            double bottom = center.y - height / 2.0;
            double left = center.x - width / 2.0;

            Path rectangle = new Path();
            rectangle.Add(new IntPoint(CoordConverter.ToClipper(left), CoordConverter.ToClipper(top)));
            rectangle.Add(new IntPoint(CoordConverter.ToClipper(right), CoordConverter.ToClipper(top)));
            rectangle.Add(new IntPoint(CoordConverter.ToClipper(right), CoordConverter.ToClipper(bottom)));
            rectangle.Add(new IntPoint(CoordConverter.ToClipper(left), CoordConverter.ToClipper(bottom)));

            return rectangle;
        }

        public static IntPoint TriangleCentroid(IntPoint a, IntPoint b, IntPoint c)
        {
            long centroidX = (a.X + b.X + c.X) / 3;
            long centroidY = (a.Y + b.Y + c.Y) / 3;
            IntPoint centroid = new IntPoint(centroidX, centroidY);
            return centroid;
        }

        public static IntPoint Translate(IntPoint coord, IntPoint translation)
        {
            IntPoint translatedPoint = new IntPoint(coord);
            translatedPoint.X += translation.X;
            translatedPoint.Y += translation.Y;

            return translatedPoint;
        }

        public static IntPoint Rotate(IntPoint coord, double angleRad)
        {
            double cosAngle = Math.Cos(angleRad);
            double sinAngle = Math.Sin(angleRad);
            long xRotated = (long)(coord.X * cosAngle - coord.Y * sinAngle);
            long yRotated = (long)(coord.Y * cosAngle + coord.X * sinAngle);
            IntPoint rotatedPoint = new IntPoint(xRotated, yRotated);
            return rotatedPoint;
        }

        public static Path Triangle(double angleCycles, double lengthAB, double lengthAC)
        {
            double angleRad = angleCycles * VMath.CycToRad;
            Path triangle = new Path();
            var a = new IntPoint(0.0, 0.0);
            var b = new IntPoint(
                CoordConverter.ToClipper(lengthAB * Math.Cos(angleRad)),
                CoordConverter.ToClipper(lengthAB * Math.Sin(angleRad)));
            var c = new IntPoint(CoordConverter.ToClipper(lengthAC), 0.0);
            triangle.Add(a);
            triangle.Add(b);
            triangle.Add(c);

            // translate triangle so centroid is on origo
            IntPoint centroid = Geometries.TriangleCentroid(a, b, c);
            IntPoint triangleTranslation = new IntPoint(-centroid.X, -centroid.Y);
            for (int ptIndex = 0; ptIndex < triangle.Count; ptIndex++)
            {
                triangle[ptIndex] = Translate(triangle[ptIndex], triangleTranslation);
                triangle[ptIndex] = Rotate(triangle[ptIndex], -angleRad / 2.0);
            }
            return triangle;
        }

        /// <summary>
        /// Get all sub paths in Clipper format from a System.Drawing GraphicsPath
        /// </summary>
        /// <param name="gPath">GraphicsPath to extract sub paths from</param>
        /// <param name="flatten">Determines detail(resolution) of output geometry</param>
        /// <returns>List of sub paths as Clipper paths extracted from gPath</returns>
        public static Paths ExtractPaths( GraphicsPath gPath, float flatten )
        {
            GraphicsPathIterator pIter = new GraphicsPathIterator(gPath);
            bool isClosed;

            Paths paths = new Paths();

            Path currentPath = new Path();
            GraphicsPath subPath = new GraphicsPath();
            // Loop through all sub paths terminating when NextSubpath returns 0 indicating no more sub paths
            while( pIter.NextSubpath(subPath, out isClosed) != 0 )
            {
                subPath.Flatten(new Matrix(), flatten);

                PointF[] pts = subPath.PathPoints;
                foreach (PointF p in pts)
                {
                    IntPoint convertedPt = CoordConverter.ToClipper(p);
                    currentPath.Add(convertedPt);
                }
                currentPath.Add(CoordConverter.ToClipper(subPath.GetLastPoint()));
                paths.Add(new Path(currentPath));
                currentPath.Clear();
            }

            return new Paths(paths);
        }

    }

    #region PluginInfo
    [PluginInfo(Name = "Quad",
                Category = "ClipperPath",
                Version = "",
                Help = "Create rectangular ClipperPath",
                Tags = ""
                )]
    #endregion PluginInfo
    public class ClipperPathQuadNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Position")]
        public IDiffSpread<Vector2D> FPosition;

        [Input("Width", DefaultValue = 1.0)]
        public IDiffSpread<double> FWidth;

        [Input("Height", DefaultValue = 1.0)]
        public IDiffSpread<double> FHeight;

        [Output("Output")]
        public ISpread<Path> FOutput;

        Paths FQuads = null;

        #endregion fields & pins
        private bool InputsChanged()
        {
            return FPosition.IsChanged || FWidth.IsChanged || FHeight.IsChanged;
        }
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if ( InputsChanged() )
            {
                if (FQuads == null)
                {
                    FQuads = new Paths();
                }
                else
                {
                    FQuads.Clear();
                }
                for (int slice = 0; slice < SpreadMax; slice++)
                {
                    FQuads.Add(Geometries.Rectangle( FPosition[slice], FWidth[slice], FHeight[slice]));
                }
                FOutput.AssignFrom(FQuads);
            }
        }
    }
    #region PluginInfo
    [PluginInfo(Name = "Circle",
                Category = "ClipperPath",
                Version = "FromRadius",
                Help = "Create circular ClipperPath",
                Tags = ""
                )]
    #endregion PluginInfo
    public class ClipperPathFromRadiusCircleNode : IPluginEvaluate
    {
        #region fields & pins
        //[Input("Input")]
        //public IDiffSpread<Vector2D> FInput;

        [Input("Radius", DefaultValue = 1.0)]
        public IDiffSpread<double> FRadius;

        [Input("Resolution", DefaultValue = 128)]
        public IDiffSpread<int> FResolution;

        [Output("Output")]
        public ISpread<Path> FOutput;

        Paths FCircles = null;

        #endregion fields & pins
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if ( /*FInput.IsChanged || */ FRadius.IsChanged || FResolution.IsChanged)
            {
                if (FCircles == null)
                {
                    FCircles = new Paths();
                }
                else
                {
                    FCircles.Clear();
                }
                for (int slice = 0; slice < SpreadMax; slice++)
                {
                    FCircles.Add(Geometries.Circle(FResolution[slice], FRadius[slice]));
                }
                FOutput.AssignFrom(FCircles);
            }
        }
    }

    struct FullyDefinedTriangle
    {
        IntPoint coordA;
        IntPoint coordB;
        IntPoint coordC;
        double angleA;
        double angleB;
        double angleC;
        double sideAB;
        double sideAC;
        double sideBC;
    }

    enum TrianglePositioning
    {
        CenteredA,
        CenteredB,
        CenteredC,
        CornerA,
        CornerB,
        CornerC,
        MidpointAB_Perependicular,
        MidpointAC_Perependicular,
        MidPointBC_Perependicular,
        MidpointAB_Median,
        MidpointAC_Median,
        MidpointBC_Median
    }

    #region PluginInfo
    [PluginInfo(Name = "Triangle",
                Category = "ClipperPath",
                Version = "FromAngleTwoSides",
                Help = "Create triangle ClipperPath",
                Tags = ""
                )]
    #endregion PluginInfo
    public class ClipperPathFromAngleTwoSidesTriangleNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Angle a", DefaultValue = 0.1666666667)]
        public IDiffSpread<double> FAngle;

        [Input("Side AB", DefaultValue = 1.0)]
        public IDiffSpread<double> FSideAB;

        [Input("Side AC", DefaultValue = 1.0)]
        public IDiffSpread<double> FSideAC;

        [Output("Output")]
        public ISpread<Path> FOutput;

        Paths FTriangle = null;

        #endregion fields & pins

        private bool InputsChanged()
        {
            return FAngle.IsChanged || FSideAB.IsChanged || FSideAC.IsChanged;
        }
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if( InputsChanged() )
            {
                if(FTriangle == null)
                {
                    FTriangle = new Paths();
                }
                else
                {
                    FTriangle.Clear();
                }
                for(int slice = 0; slice < SpreadMax; slice++)
                {
                    FTriangle.Add( Geometries.Triangle(FAngle[slice], FSideAB[slice], FSideAC[slice]));
                }
                FOutput.AssignFrom(FTriangle);
            }
        }
    }

    struct FontProperties
    {
        public FontFamily  family;
        public float       size;
        public int         style;
    }

    #region PluginInfo
    [PluginInfo(Name = "Text",
                Category = "ClipperPath",
                Version = "",
                Help = "Create TextClipperPath",
                Tags = ""
                )]
    #endregion PluginInfo
    public class ClipperPathTextNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Position")]
        public IDiffSpread<Vector2D> FPosition;

        [Input("Text", DefaultString = "Text")]
        public IDiffSpread<string> FText;

        [Input("Font", EnumName = "SystemFonts")]
        IDiffSpread<EnumEntry> FFontIn;

        [Input("Size", DefaultValue = 1.0)]
        IDiffSpread<float> FTextSizeIn;

        [Input("Bold", IsToggle = true, DefaultBoolean = false)]
        IDiffSpread<bool> FBold;

        [Input("Italic", IsToggle = true, DefaultBoolean = false)]
        IDiffSpread<bool> FItalic;

        [Input("Strikeout", IsToggle = true, DefaultBoolean = false)]
        IDiffSpread<bool> FStrikeout;

        [Input("Underline", IsToggle = true, DefaultBoolean = false)]
        IDiffSpread<bool> FUnderline;

        [Input("Flatness", DefaultValue = 0.25)]
        public IDiffSpread<float> FFlatness;

        [Output("Output")]
        public ISpread<Path> FOutput;

        GraphicsPath FTextPath = null;
        [Import()]
        public ILogger FLogger;
        #endregion fields & pins

        private bool InputsChanged()
        {
            bool changed =
                FText.IsChanged
                || FFlatness.IsChanged
                || FTextSizeIn.IsChanged
                || FFontIn.IsChanged
                || FBold.IsChanged
                || FItalic.IsChanged
                || FStrikeout.IsChanged
                || FUnderline.IsChanged
                || FPosition.IsChanged;
            return changed;
        }

        private FontProperties FontPropertiesFromPins()
        {
            FontProperties fp;
            fp.size = FTextSizeIn[0];
            try
            {
                fp.family = new FontFamily(FFontIn[0].Name);
            }
            catch (Exception)
            {
                fp.family = FontFamily.GenericSansSerif;
            }
            fp.style = (int)FontStyle.Regular;
            if( FBold[0] )
            {
                fp.style |= (int)FontStyle.Bold;
            }
            if( FItalic[0] )
            {
                fp.style |= (int)FontStyle.Italic;
            }
            if( FStrikeout[0] )
            {
                fp.style |= (int)FontStyle.Strikeout;
            }
            if( FUnderline[0] )
            {
                fp.style |= (int)FontStyle.Underline;
            }
            return fp;
        }

        public void Evaluate(int SpreadMax)
        {
            if( FTextPath == null || InputsChanged() )
            {
                FontProperties fp = FontPropertiesFromPins();
                PointF textPos = new PointF((float)FPosition[0].x, (float)FPosition[0].y);
                FTextPath = new GraphicsPath();

                FTextPath.AddString(
                    FText[0],
                    fp.family,
                    fp.style,
                    fp.size,
                    textPos,
                    StringFormat.GenericDefault);
                
                var outPaths = Geometries.ExtractPaths(FTextPath, FFlatness[0]);
                FOutput.AssignFrom(outPaths);
            }

        }
    }
    // Lines?
    // Gears, rack
    // .. ?
}