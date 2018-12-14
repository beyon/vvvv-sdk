
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
    [PluginInfo(Name = "Reverse",
        Category = "ClipperPath",
        Version = "Vector",
        Help = "Reverse/change the orientation of a path",
        Tags = "")]
    #endregion PluginInfo
    public class ClipperPathVector2dReverseNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public IDiffSpread<Path> FInput;

        [Output("Output")]
        public ISpread<Path> FOutput;

        #endregion fields & pins
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsChanged && FInput != null)
            {
                FOutput.SliceCount = FInput.SliceCount;
                for (int p = 0; p < FInput.SliceCount; p++)
                {
                    FInput[p].Reverse();
                    FOutput[p] = FInput[p];
                }
            }
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "Offset",
        Category = "ClipperPath",
        Version = "Vector",
        Help = "Create a new path that is offset from the input path",
        Tags = "")]
    #endregion PluginInfo
    public class ClipperPathVector2dOffsetNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public IDiffSpread<Path> FInput;

        [Input("Delta")]
        public IDiffSpread<double> FDelta;

        [Input("Join type")]
        public IDiffSpread<JoinType> FJoinType;

        [Input("End type")]
        public IDiffSpread<EndType> FEndType;

        [Input("Miter Limit", DefaultValue = 2.0)]
        public IDiffSpread<double> FMiterLimit;

        [Input("Arc tolerance", DefaultValue = 0.25)]
        public IDiffSpread<double> FArcTolerance;

        [Output("Output")]
        public ISpread<Path> FOutput;

        #endregion fields & pins
        private bool InputsChanged()
        {
            bool changed =
                FInput.IsChanged
                || FDelta.IsChanged
                || FJoinType.IsChanged
                || FEndType.IsChanged
                || FMiterLimit.IsChanged
                || FArcTolerance.IsChanged;

            return changed;

        }

        private int CalcSpreadMax()
        {
            int max = Math.Max(FInput.SliceCount, FDelta.SliceCount);
            max = Math.Max(max, FJoinType.SliceCount);
            max = Math.Max(max, FEndType.SliceCount);
            max = Math.Max(max, FMiterLimit.SliceCount);
            max = Math.Max(max, FArcTolerance.SliceCount);
            return max;
        }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if ( InputsChanged() && FInput != null)
            {
                if (FInput.SliceCount > 0)
                {
                    // Todo : bin size / spread logic
                    //SpreadMax = CalcSpreadMax();
                    //for(int slice = 0; slice < SpreadMax; slice++)
                    //{

                    //}
                    ClipperOffset offsetWorker = new ClipperOffset(FMiterLimit[0], FArcTolerance[0]);
                    // Todo : fix null reference
                    offsetWorker.AddPaths(new Paths(FInput), FJoinType[0], FEndType[0]);
                    Paths solution = new Paths();
                    offsetWorker.Execute(ref solution, CoordConverter.ToClipper(FDelta[0]));
                    FOutput.AssignFrom(solution);
                }
            }
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "Simplify",
        Category = "ClipperPath",
        Version = "Vector",
        Help = "Simplify a path",
        Tags = "")]
    #endregion PluginInfo
    public class ClipperPathVector2dSimplifyNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public IDiffSpread<Path> FInput;

        [Input("Join type")]
        public IDiffSpread<PolyFillType> FFillType;

        [Output("Output")]
        public ISpread<Path> FOutput;

        #endregion fields & pins
        private bool InputsChanged()
        {
            return FInput.IsChanged || FFillType.IsChanged;
        }
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (InputsChanged() && FInput != null)
            {
                FOutput.AssignFrom(
                    Clipper.SimplifyPolygons(
                        new Paths(FInput),
                        FFillType[0]));
            }
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "Clean",
        Category = "ClipperPath",
        Version = "Vector",
        Help = "Clean a path",
        Tags = "")]
    #endregion PluginInfo
    public class ClipperPathVector2dCleanNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public IDiffSpread<Path> FInput;

        [Input("Distance", DefaultValue = 1.415)]
        public IDiffSpread<double> FDistance;

        [Output("Output")]
        public ISpread<Path> FOutput;

        #endregion fields & pins

        private bool InputsChanged()
        {
            return FInput.IsChanged || FDistance.IsChanged;
        }
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (InputsChanged() && FInput != null)
            {
                FOutput.AssignFrom(
                    Clipper.CleanPolygons(
                        new Paths(FInput),
                        FDistance[0]));
            }
        }
    }
}
