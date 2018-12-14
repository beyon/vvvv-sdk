// Wrapper for Clipper library by Angus Johnson: http://www.angusj.com/delphi/clipper.php
// Refer to http://www.angusj.com/delphi/clipper/documentation/Docs/Overview/_Body.htm for details
// of the library functionality

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
    [PluginInfo(Name = "AsClipperPath",
        Category = "ClipperPath",
        Version = "Vector",
        Help = "Convert 2d vector to ClipperPath",
        Tags = "")]
    #endregion PluginInfo
    public class ClipperPathVector2dAsClipperPathNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public IDiffSpread<ISpread<Vector2D>> FInput;

        [Output("Output")]
        public ISpread<Path> FOutput;
        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsChanged)
            {
                Paths inputPaths = new Paths();
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    Path inputPath = new Path();
                    foreach (Vector2D v in FInput[i])
                    {
                        inputPath.Add( CoordConverter.ToClipperPoint(v) );
                    }

                    inputPaths.Add(inputPath);
                }
                FOutput.AssignFrom(inputPaths);
            }
        }
    }


    #region PluginInfo
    [PluginInfo(Name = "AsValue",
        Category = "ClipperPath",
        Version = "Vector",
        Help = "Convert ClipperPath to 2d vector",
        Tags = "")]
    #endregion PluginInfo
    public class CVector2dAsValueNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public IDiffSpread<Path> FInput;

        [Output("Output")]
        public ISpread<ISpread<Vector2D>> FOutput;

        #endregion fields & pins
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {

            if (FInput.IsChanged && FInput != null)
            {
                FOutput.SliceCount = FInput.SliceCount;
                Vector2D v;

                int index = 0;
                foreach (Path p in FInput)
                {
                    if (p != null)
                    {
                        List<Vector2D> innerSpread = new List<Vector2D>();
                        foreach (IntPoint vertex in p)
                        {
                            v.x = CoordConverter.FromClipper(vertex.X);
                            v.y = CoordConverter.FromClipper(vertex.Y);

                            innerSpread.Add(v);
                        }
                        if (innerSpread.Count > 0)
                        {
                            FOutput[index] = new Spread<Vector2D>();
                            FOutput[index].AssignFrom(innerSpread);
                            index++;
                        }
                    }

                }
            }

        }
    }

}
