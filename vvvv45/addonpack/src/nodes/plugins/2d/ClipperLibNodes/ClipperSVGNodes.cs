
#region usings
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes;

using SlimDX;
using Svg;
using Svg.Transforms;
using ClipperLib;
using ClipperHelper;

#endregion usings

namespace VVVV.Nodes
{
    using Path = List<IntPoint>;
    using Paths = List<List<IntPoint>>;

    #region PluginInfo
    [PluginInfo(Name = "AsSVG",
                Category = "ClipperPath",
                Help = "Renders a path to a Renderer (SVG)",
                Tags = "SVG, 2d, vector")]
    #endregion PluginInfo
    public class PathAsSVGNode : SVGVisualElementFillNode<SvgPath>
    {
#pragma warning disable 649
        [Input("Paths", Order = -50)]
        IDiffSpread<Path> FPathsIn;

#pragma warning restore

        protected override SvgPath CreateElement()
        {
            var p = new SvgPath();
            return p;
        }

        protected override int CalcSpreadMax(int max)
        {
            max = Math.Max(FTransformIn.SliceCount, FStrokeIn.SliceCount);
            max = Math.Max(max, FStrokeWidthIn.SliceCount);
            max = Math.Max(max, FEnabledIn.SliceCount);
            max = Math.Max(max, FFillIn.SliceCount);
            max = Math.Max(max, FFillModeIn.SliceCount);
            max = Math.Max(max, 1);
            return max;
        }

        protected override bool PinsChanged()
        {
            return base.PinsChanged() || FPathsIn.IsChanged;
        }

        protected override void CalcGeometry(SvgPath elem, Vector2 trans, Vector2 scale, int slice)
        {
            elem.PathData.Clear();
            var coords = new List<float>(7);
            if (FPathsIn.SliceCount >= 1)
            {
                foreach (Path subPath in FPathsIn)
                {
                    if (subPath != null)
                    {
                        char c = 'M';
                        for (int i = 0; i < subPath.Count; i++)
                        {
                            coords.Clear();

                            //make sure the first and last command fits the specification
                            if (i == 0)
                            {
                                c = 'M';
                            }
                            else {
                                c = 'L';
                            }
                            IntPoint vertex = subPath[i];
                            coords.Add(CoordConverter.FromClipper(vertex.X));
                            coords.Add(CoordConverter.FromClipper(vertex.Y));
                            SvgPathBuilder.CreatePathSegment(c, elem.PathData, coords, char.IsLower(c));
                        }
                        // Close Path
                        // dummy points for closing path
                        coords.Clear();
                        coords.Add(0.0f); // not needed ? check svg src?
                        coords.Add(0.0f); // not needed ? check svg src?				
                        c = 'Z';
                        SvgPathBuilder.CreatePathSegment(c, elem.PathData, coords, char.IsLower(c));
                    }
                }
            }

        }
    }

    #region PluginInfo
    [PluginInfo(Name = "AsClipperPath",
    Category = "2d",
    Version = "SVG",
    Help = "Returns the path data from SVG layers",
    Tags = "")]
    #endregion PluginInfo

    public class CSVG2dAsClipperPathNode : IPluginEvaluate
    {
#pragma warning disable 649
        [Input("Layer")]
        ISpread<SvgElement> FInput;
        [Input("Flatten", DefaultValue = 1)]
        ISpread<bool> FFlattenInput;
        [Input("Max Flatten Error", DefaultValue = 0.25)]
        ISpread<float> FMaxFlattenInput;
        [Input("Update", IsBang = true, IsSingle = true)]
        ISpread<bool> FUpdateInput;
        [Output("ClipperPath")]
        ISpread<Path> FPathOutput;
        [Output("Path Type")]
        ISpread<ISpread<int>> FPathTypeOutput;
#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (FUpdateInput[0])
            {
                FPathOutput.SliceCount = SpreadMax;
                FPathTypeOutput.SliceCount = SpreadMax;

                Paths svgPaths = new Paths();

                for (int i = 0; i < SpreadMax; i++)
                {
                    var elem = FInput[i];
                    var pto = FPathTypeOutput[i];

                    if (elem is SvgVisualElement || elem is SvgFragment)
                    {
                        GraphicsPath p;
                        if (elem is SvgGroup)
                        {
                            p = ((SvgGroup)elem).Path;
                            p.Transform(elem.Transforms.GetMatrix());
                        }
                        else if (elem is SvgVisualElement)
                        {
                            p = (GraphicsPath)((SvgVisualElement)elem).Path.Clone();
                        }
                        else
                        {
                            p = ((SvgFragment)elem).Path;
                        }


                        p.Transform(elem.Transforms.GetMatrix());

                        if (FFlattenInput[i])
                        {
                            p.Flatten(new System.Drawing.Drawing2D.Matrix(), FMaxFlattenInput[i] * 0.1f);
                        }

                        pto.SliceCount = p.PointCount;

                        Path svgPath = new Path();
                        for (int j = 0; j < p.PointCount; j++)
                        {
                            svgPath.Add(new IntPoint(CoordConverter.ToClipper(p.PathPoints[j].X),
                                                     CoordConverter.ToClipper(p.PathPoints[j].Y)));

                            pto[j] = p.PathTypes[j];
                        }
                        svgPaths.Add(svgPath);
                    }
                    else
                    {
                        pto.SliceCount = 0;
                    }
                }
                FPathOutput.AssignFrom(svgPaths);
            }
        }
    }
}
