
#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes;
using VVVV.Utils.VMath;

using ClipperLib;
using ClipperHelper;

#endregion usings

namespace VVVV.Nodes
{
    using Path = List<IntPoint>;
    using Paths = List<List<IntPoint>>;

    #region PluginInfo
    [PluginInfo(Name = "Orientation",
        Category = "ClipperPath",
        Version = "Vector",
        Help = "Get the orientation (winding) of the path",
        Tags = "")]
    #endregion PluginInfo
    public class ClipperPathVector2dOrientationNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public IDiffSpread<Path> FInput;

        [Output("Orientation")]
        public ISpread<bool> FOrientations;

        #endregion fields & pins
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsChanged && FInput != null)
            {
                List<bool> orientations = new List<bool>();
                FOrientations.SliceCount = FInput.SliceCount;
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FInput[i] != null)
                    {
                        if (FInput[i].Count > 0)
                        {
                            FOrientations[i] = Clipper.Orientation(FInput[i]);
                        }
                    }
                }
            }
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "Area",
        Category = "ClipperPath",
        Version = "Vector",
        Help = "Calculate the area of a path",
        Tags = "")]
    #endregion PluginInfo
    public class ClipperPathVector2dAreaNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public IDiffSpread<Path> FInput;

        [Output("Output")]
        public ISpread<double> FOutput;

        #endregion fields & pins
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsChanged && FInput != null)
            {
                FOutput.SliceCount = FInput.SliceCount;
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    FOutput[i] = CoordConverter.FromClipperArea(Clipper.Area(FInput[i]));
                }
            }
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "Bounds",
        Category = "ClipperPath",
        Version = "Vector",
        Help = "Get the bounds of a path",
        Tags = "")]
    #endregion PluginInfo
    public class ClipperPathVector2dBoundsNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public IDiffSpread<Path> FInput;

        [Output("Bottom")]
        public ISpread<double> FBottom;

        [Output("Top")]
        public ISpread<double> FTop;

        [Output("Left")]
        public ISpread<double> FLeft;

        [Output("Right")]
        public ISpread<double> FRight;

        [Output("Height")]
        public ISpread<double> FHeight;

        [Output("Width")]
        public ISpread<double> FWidth;

        [Output("Center")]
        public ISpread<Vector2D> FCenter;


        #endregion fields & pins
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsChanged && FInput != null)
            {
                FBottom.SliceCount = FTop.SliceCount = FLeft.SliceCount = FRight.SliceCount = FInput.SliceCount;
                FHeight.SliceCount = FWidth.SliceCount = FCenter.SliceCount = FInput.SliceCount;

                double bottom, top, left, right, width, height;
                Paths subPath = new Paths();
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FInput[i] != null)
                    {
                        subPath.Add(FInput[i]);
                        IntRect boundingRect = Clipper.GetBounds(subPath);
                        // ?? bottom/top reversed due to difference in coordinate system ??
                        bottom = CoordConverter.FromClipper(boundingRect.top);
                        top = CoordConverter.FromClipper(boundingRect.bottom);
                        left = CoordConverter.FromClipper(boundingRect.left);
                        right = CoordConverter.FromClipper(boundingRect.right);
                        height = top - bottom;
                        width = right - left;
                    }
                    else {
                        bottom = top = left = right = height = width = 0.0;
                    }
                    FBottom[i] = bottom;
                    FTop[i] = top;
                    FLeft[i] = left;
                    FRight[i] = right;
                    FHeight[i] = height;
                    FWidth[i] = width;
                    FCenter[i] = new Vector2D(left + width / 2.0, bottom + height / 2.0);
                    subPath.Clear();
                }
            }
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "HitTest",
        Category = "ClipperPath",
        Version = "Vector",
        Help = "Test if a point is inside a path or not",
        Tags = "")]
    #endregion PluginInfo
    public class ClipperPathVector2dHitTestNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public IDiffSpread<Path> FInput;

        [Input("x")]
        public IDiffSpread<double> FX;

        [Input("y")]
        public IDiffSpread<double> FY;

        [Output("HitTest")]
        public ISpread<double> FResult;

        #endregion fields & pins
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if ((FInput.IsChanged || FX.IsChanged || FY.IsChanged) && FInput != null)
            {
                List<bool> result = new List<bool>();
                FResult.SliceCount = FInput.SliceCount;
                //TODO: spreading of FX/FY
                IntPoint testPoint = new IntPoint(CoordConverter.ToClipper(FX[0]), 
                                                  CoordConverter.ToClipper(FY[0]));
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    FResult[i] = Clipper.PointInPolygon(testPoint, FInput[i]);
                }
            }
        }
    }

}