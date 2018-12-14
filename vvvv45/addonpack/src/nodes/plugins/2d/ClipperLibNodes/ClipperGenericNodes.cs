
#region usings
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes;
using VVVV.Nodes.Generic;
using VVVV.Utils.Streams;

using ClipperLib;

#endregion usings

namespace VVVV.Nodes
{
    using Path = List<IntPoint>;
    using Paths = List<List<IntPoint>>;

    #region SingleValue

    [PluginInfo(Name = "Cast",
                Category = "ClipperPath",
                Help = "Casts any type to a type of this category, so be sure the input is of the required type",
                Tags = "convert, as, generic"
                )]
    public class ClipperPathCastNode : Cons<Path> { }

    #endregion SingleValue

    #region SpreadOps

    [PluginInfo(Name = "Cons",
                Category = "ClipperPath",
                Help = "Concatenates all Input spreads.",
                Tags = "generic, spreadop"
                )]
    public class ClipperPathConsNode : Cons<Path> { }

    [PluginInfo(Name = "CAR",
                Category = "ClipperPath",
                Version = "Bin",
                Help = "Splits a given spread into first slice and remainder.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class ClipperPathCARBinNode : CARBin<Path> { }

    [PluginInfo(Name = "CDR",
                Category = "ClipperPath",
                Version = "Bin",
                Help = "Splits a given spread into remainder and last slice.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class ClipperPathCDRBinNode : CDRBin<Path> { }

    [PluginInfo(Name = "Reverse",
                Category = "ClipperPath",
                Version = "Bin",
                Help = "Reverses the order of the slices in the Spread.",
                Tags = "invert, generic, spreadop",
                Author = "woei"
               )]
    public class ClipperPathReverseBinNode : ReverseBin<Path> { }

    [PluginInfo(Name = "Shift",
                Category = "ClipperPath",
                Version = "Bin",
                Help = "Shifts the slices in the Spread by the given Phase.",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class ClipperPathShiftBinNode : ShiftBin<Path> { }

    [PluginInfo(Name = "SetSlice",
                Category = "ClipperPath",
                Version = "Bin",
                Help = "Replaces individual slices of a spread with the given input",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class ClipperPathSetSliceNode : SetSlice<Path> { }

    [PluginInfo(Name = "DeleteSlice",
                Category = "ClipperPath",
                Help = "Removes slices from the Spread at the positions addressed by the Index pin.",
                Tags = "remove, generic, spreadop",
                Author = "woei"
               )]
    public class ClipperPathDeleteSliceNode : DeleteSlice<Path> { }

    [PluginInfo(Name = "Select",
                Category = "ClipperPath",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ",
                Tags = "repeat, resample, duplicate, spreadop"
               )]
    public class ClipperPathSelectNode : Select<Path> { }

    [PluginInfo(Name = "Select",
                Category = "ClipperPath",
                Version = "Bin",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ",
                Tags = "repeat, resample, duplicate, spreadop",
                Author = "woei"
            )]
    public class ClipperPathSelectBinNode : SelectBin<Path> { }

    [PluginInfo(Name = "Unzip",
                Category = "ClipperPath",
                Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it.",
                Tags = "split, generic, spreadop"
               )]
    public class ClipperPathUnzipNode : Unzip<Path> { }

    [PluginInfo(Name = "Unzip",
                Category = "ClipperPath",
                Version = "Bin",
                Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it. With Bin Size.",
                Tags = "split, generic, spreadop"
               )]
    public class ClipperPathUnzipBinNode : Unzip<IInStream<Path>> { }

    [PluginInfo(Name = "Zip",
                Category = "ClipperPath",
                Help = "Interleaves all Input spreads.",
                Tags = "interleave, join, generic, spreadop"
               )]
    public class ClipperPathZipNode : Zip<Path> { }

    [PluginInfo(Name = "Zip",
                Category = "ClipperPath",
                Version = "Bin",
                Help = "Interleaves all Input spreads.",
                Tags = "interleave, join, generic, spreadop"
               )]
    public class ClipperPathZipBinNode : Zip<IInStream<Path>> { }

    [PluginInfo(Name = "GetSpread",
                Category = "ClipperPath",
                Version = "Bin",
                Help = "Returns sub-spreads from the input specified via offset and count",
                Tags = "generic, spreadop",
                Author = "woei")]
    public class ClipperPathGetSpreadNode : GetSpreadAdvanced<Path> { }

    [PluginInfo(Name = "SetSpread",
                Category = "ClipperPath",
                Version = "Bin",
                Help = "Allows to set sub-spreads into a given spread.",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class ClipperPathSetSpreadNode : SetSpread<Path> { }

    [PluginInfo(Name = "Pairwise",
                Category = "ClipperPath",
                Help = "Returns all combinations of pairs of successive slices. From an input ABCD returns AB, BC, CD.",
                Tags = "generic, spreadop"
                )]
    public class ClipperPathPairwiseNode : Pairwise<Path> { }

    [PluginInfo(Name = "SplitAt",
                Category = "ClipperPath",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class ClipperPathSplitAtNode : SplitAtNode<Path> { }

    #endregion SpreadOps

    #region CloneNodes
    [PluginInfo(Name = "FrameDelay",
                Category = "ClipperPath",
                Help = "Delays the input value one calculation frame.",
                Tags = "generic"
               )]
    public class ClipperPathFrameDelayNode : FrameDelayNode<Path>
    {
        public ClipperPathFrameDelayNode() : base(ClipperPathCopier.Default) { }
    }

    class ClipperPathCopier : Copier<Path>
    {
        public static readonly ClipperPathCopier Default = new ClipperPathCopier();

        public override Path Copy(Path value)
        {
            return new Path(value);
        }
    }

    #endregion

    #region Collections

    [PluginInfo(Name = "Buffer",
                Category = "ClipperPath",
                Help = "Inserts the input at the given index.",
                Tags = "generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class ClipperPathBufferNode : BufferNode<Path>
    {
        public ClipperPathBufferNode() : base(ClipperPathCopier.Default) { }
    }

    [PluginInfo(Name = "Queue",
                Category = "ClipperPath",
                Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO (First In First Out) fashion.",
                Tags = "generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class ClipperPathQueueNode : QueueNode<Path>
    {
        public ClipperPathQueueNode() : base(ClipperPathCopier.Default) { }
    }

    [PluginInfo(Name = "RingBuffer",
                Category = "ClipperPath",
                Help = "Inserts the input at the ringbuffer position.",
                Tags = "generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class ClipperPathRingBufferNode : RingBufferNode<Path>
    {
        public ClipperPathRingBufferNode() : base(ClipperPathCopier.Default) { }
    }

    [PluginInfo(Name = "Store",
                Category = "ClipperPath",
                Help = "Stores a spread and sets/removes/inserts slices.",
                Tags = "add, insert, remove, generic, spreadop, collection",
                Author = "woei",
                AutoEvaluate = true
               )]
    public class ClipperPathStoreNode : Store<Path> { }

    [PluginInfo(Name = "Stack",
        Category = "ClipperPath",
                Help = "Stack data structure implementation using the LIFO (Last In First Out) paradigm.",
                Tags = "generic, spreadop, collection",
                Author = "vux"
                )]
    public class ClipperPathStackNode : StackNode<Path> { }

    [PluginInfo(
           Name = "QueueStore",
           Category = "ClipperPath",
           Help = "Stores a series of queues",
           Tags = "append, remove, generic, spreadop, collection",
           Author = "motzi"
    )]
    public class ClipperPathQueueStoreNodes : QueueStore<Path>
    {
        ClipperPathQueueStoreNodes() : base(ClipperPathCopier.Default) { }
    }

    #endregion Collections


}
