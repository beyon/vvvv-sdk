
#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes;

using ClipperLib;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
    using Path = List<IntPoint>;
    using Paths = List<List<IntPoint>>;

    public abstract class BooleanShapeCombinationNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Subject Paths")]
        public IDiffSpread<Path> FInput1;

        [Input("Clip Paths")]
        public IDiffSpread<Path> FInput2;

        [Input("Subject FillType")]
        public IDiffSpread<PolyFillType> FSubjFillType;

        [Input("Clip FillType")]
        public IDiffSpread<PolyFillType> FClipFillType;

        [Output("Output")]
        public ISpread<Path> FOutput;

        [Import()]
        public ILogger FLogger;

        #endregion fields & pins

        protected abstract ClipType GetClipType();
        //called when data for any output pin is requested
        private bool InputsChanged()
        {
            bool changed =
                FInput1.IsChanged
                || FInput2.IsChanged
                || FSubjFillType.IsChanged
                || FClipFillType.IsChanged;
            return changed;
        }
        public void Evaluate(int SpreadMax)
        {

            if ( InputsChanged() )
            {
                //TODO: fix broken binsize logic
                // ?? To past self: what exactly is broken ??
                Paths subj = new Paths(FInput1.SliceCount);

                foreach (Path p in FInput1)
                {
                    if (p != null)
                    {
                        subj.Add(p);
                    }
                }

                Paths clip = new Paths(FInput2.SliceCount);

                foreach (Path p in FInput2)
                {
                    if (p != null)
                    {
                        clip.Add(p);
                    }
                }

                if ((subj.Count >= 1) && (clip.Count >= 1))
                {
                    Paths solution = new Paths();

                    Clipper c = new Clipper();
                    c.AddPaths(subj, PolyType.ptSubject, true);
                    c.AddPaths(clip, PolyType.ptClip, true);

                    c.Execute(GetClipType(), solution,
                        FSubjFillType[0], FClipFillType[0]);

                    FOutput.AssignFrom(solution);
                }
                else if (subj.Count >= 1)
                {
                    FOutput.AssignFrom(subj);
                }
                else if (clip.Count >= 1)
                {
                    FOutput.AssignFrom(clip);
                }
                else {
                    // FOutput empty because no valid inputs
                    FOutput.SliceCount = 0;
                }

            }
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "Union",
        Category = "ClipperPath",
        Version = "2D",
        Help = "Union of paths",
        Tags = "boolean")]
    #endregion PluginInfo
    public class ClipperPath2DValueUnionNode : BooleanShapeCombinationNode
    {
        protected override ClipType GetClipType()
        {
            return ClipType.ctUnion;
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "Difference",
        Category = "ClipperPath",
        Version = "2D",
        Help = "Difference of paths",
        Tags = "boolean")]
    #endregion PluginInfo
    public class ClipperPath2DValueDifferenceNode : BooleanShapeCombinationNode
    {
        protected override ClipType GetClipType()
        {
            return ClipType.ctDifference;
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "Intersection",
        Category = "ClipperPath",
        Version = "2D",
        Help = "Intersection of paths",
        Tags = "boolean")]
    #endregion PluginInfo
    public class ClipperPath2DValueIntersectionNode : BooleanShapeCombinationNode
    {
        protected override ClipType GetClipType()
        {
            return ClipType.ctIntersection;
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "Xor",
        Category = "ClipperPath",
        Version = "2D",
        Help = "XOr of paths",
        Tags = "boolean")]
    #endregion PluginInfo
    public class ClipperPath2DValueXorNode : BooleanShapeCombinationNode
    {
        protected override ClipType GetClipType()
        {
            return ClipType.ctXor;
        }
    }

}