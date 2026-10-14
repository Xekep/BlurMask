using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

public readonly record struct LegacyFreshMainPhysicalReceiverCall(int DescriptorOrdinal,
    float StartingNeuralScale,float EndingNeuralScale,int LocalPhysicalCount,int ProviderGroupCount,int VerticalStripCount);

/// <summary>Fresh direct-source15F050 after owned15B260 splitting, with native
/// constructor E8 null and no external setter/page/prior source. Preserves the
/// provider neural-scale state across descriptor resets and original point keys.
/// This does not represent arbitrary SDK callback overrides or completed249/main decoding.</summary>
public sealed class LegacyFreshMainPhysicalReordering
{
    private readonly InkStroke[] strokes;
    private readonly int[] originalPhysical;
    private readonly float[] probabilities;
    private readonly byte[][] descriptors;
    public LegacyFreshMainDescriptorSplit Input {get;}
    public IReadOnlyList<InkStroke> Strokes {get;}
    public ReadOnlyMemory<int> OriginalPhysicalStrokeIndices=>originalPhysical;
    public ReadOnlyMemory<float> SourceStrokeProbabilities=>probabilities;
    public LegacySourcePointChronology Chronology {get;}
    public IReadOnlyList<LegacyFreshMainPhysicalReceiverCall> ReceiverCalls {get;}
    public bool HasE8Callback=>false;
    public bool HasExistingGuides=>false;
    public byte[][] CopyDescriptors()=>descriptors.Select(d=>d.ToArray()).ToArray();
    private LegacyFreshMainPhysicalReordering(LegacyFreshMainDescriptorSplit input,InkStroke[] strokes,
        int[] originalPhysical,float[] probabilities,byte[][] descriptors,LegacySourcePointChronology chronology,
        LegacyFreshMainPhysicalReceiverCall[] calls)
    {Input=input;this.strokes=strokes;Strokes=Array.AsReadOnly(strokes);this.originalPhysical=originalPhysical;
        this.probabilities=probabilities;this.descriptors=descriptors;Chronology=chronology;ReceiverCalls=Array.AsReadOnly(calls);}

    /// <summary>Owner is specifically fresh direct main0 without property events
    /// or source219 metadata setters.163970 initializes E8 null, so1680D0's E8
    /// callback is genuinely absent here. Caller-supplied guides, source flags,
    /// physical memberships or carry scales are not accepted. The model byte
    /// snapshots must be the exact portable embedded2E3 resources.</summary>
    public static LegacyFreshMainPhysicalReordering Prepare(LegacyFreshMainDescriptorSplit actualSplit,
        ReadOnlySpan<byte> spatial30Resource,ReadOnlySpan<byte> physical50Resource,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualSplit);cancellationToken.ThrowIfCancellationRequested();
        CheckHash(spatial30Resource,85812,"10f4b43cd02873f1995f543dece70e70c1f22cbdb7dcbf63e1c5c205966ac70e");
        CheckHash(physical50Resource,161332,"01f9fd0bf4b5c0c3c1acc170ee82938e24a132696acbfbcfb34573a9d0550c7e");
        var owner=actualSplit.Input;var descriptors=actualSplit.CopyDescriptors();
        using var stream30=new MemoryStream(spatial30Resource.ToArray(),false);
        using var stream50=new MemoryStream(physical50Resource.ToArray(),false);
        var spatial=PreparedLineBoundaryClassifier.Load(stream30);var physical=PreparedLineBoundaryClassifier.Load(stream50);
        var order=new List<int>(actualSplit.Strokes.Count);var calls=new List<LegacyFreshMainPhysicalReceiverCall>();
        float carry=owner.Grouping.NeuralScale;
        for(int d=0;d<descriptors.Length;d++)
        {
            cancellationToken.ThrowIfCancellationRequested();var descriptor=descriptors[d];
            int kind=BinaryPrimitives.ReadUInt16LittleEndian(descriptor)&15;
            if(kind==3)continue;
            if(kind!=0||BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(16))!=-1||
                BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(32))!=-1||descriptor[72]!=1)
                throw new InvalidDataException("Actual fresh unknown horizontal descriptor owner required.");
            int first=BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(36)),last=BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(40));
            if(first<0||last<first||last>=actualSplit.Strokes.Count||order.Count!=first)
                throw new InvalidDataException("Actual contiguous split physical ranges required.");
            var points=Enumerable.Range(first,last-first+1).Select(i=>actualSplit.Strokes[i]).ToArray();
            if(first==last){order.Add(first);continue;}
            //A checked numeric receiver view: its metadata/probabilities are
            //NOT exported as the parent source. Native15F050 feeds this retained
            //buffer to2E3, without constructing a new source219 or source keys.
            //Reject if the reusable ingress helper would change any current point.
            var numeric=LegacyOrdinarySourceInk.Prepare(points,owner.Packet.Source.Profile,cancellationToken);
            for(int p=0;p<points.Length;p++)
                if(!numeric.Strokes[p].Points.SequenceEqual(points[p].Points))
                    throw new NotSupportedException("Current retained buffer requires a direct2E3 receiver view; secondary source filtering would change it.");
            var scale=new LegacyOrdinaryLineScaleSnapshot(numeric,owner.Scale.Profile,ReadOnlyMemory<float>.Empty,
                new LegacyLineScales(carry,owner.Scale.Scales.Secondary));
            float initial=carry;
            var groups=LegacyOrdinaryNeuralLineInferenceFactory.InferGrouping(scale,owner.PhysicalResolution,
                new bool[points.Length],owner.QuarterVerticalWhenHalfHorizontal,owner.InternalScaleQuantile,
                spatial,physical,owner.WindowWidthMultiplier,owner.WindowOverlap,cancellationToken);
            carry=groups.NeuralScale;
            //15F050 uses source+B0 primary scale for insertion tolerance,
            //not the carried/possibly re-estimated receiver neural scale.
            int[] local=LegacyNativeUnknownPhysicalIntervalOrder.Order(points,groups.Selected.Groups,
                owner.Scale.Scales.Primary,owner.HorizontalOrderTolerance,cancellationToken);
            order.AddRange(local.Select(i=>first+i));
            calls.Add(new(d,initial,carry,points.Length,groups.Selected.Groups.Count,groups.VerticalStripCount));
        }
        if(order.Count!=actualSplit.Strokes.Count||order.Distinct().Count()!=order.Count)
            throw new InvalidDataException("Native physical reorder lost/duplicated source membership.");
        int[] original=order.Select(i=>actualSplit.OriginalPhysicalStrokeIndices.Span[i]).ToArray();
        var strokes=order.Select(i=>new InkStroke(actualSplit.Strokes[i].Points.ToArray())).ToArray();
        var probabilities=order.Select(i=>actualSplit.SourceStrokeProbabilities.Span[i]).ToArray();
        int[] counts=actualSplit.Strokes.Select(s=>s.Points.Count).ToArray();int[] starts=new int[counts.Length];
        for(int i=1;i<counts.Length;i++)starts[i]=starts[i-1]+counts[i-1];
        uint[] incoming=order.SelectMany(i=>Enumerable.Range(starts[i],counts[i]).Select(p=>(uint)p)).ToArray();
        var chronology=actualSplit.Chronology.RemapPreparedPoints(strokes.Select(s=>s.Points.Count).ToArray(),incoming,1,cancellationToken);
        return new(actualSplit,strokes,original,probabilities,descriptors,chronology,calls.ToArray());
    }
    private static void CheckHash(ReadOnlySpan<byte> data,int size,string expected)
    {
        if(data.Length!=size||!Convert.ToHexString(SHA256.HashData(data)).Equals(expected,StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Actual checked embedded2E3 model required.");
    }
}
