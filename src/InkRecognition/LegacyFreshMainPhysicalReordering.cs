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
        var owner=actualSplit.Input;
        var input=new LegacyFreshPhysicalBuffer(actualSplit.CopyDescriptors(),actualSplit.Strokes.ToArray(),
            actualSplit.OriginalPhysicalStrokeIndices.ToArray(),actualSplit.SourceStrokeProbabilities.ToArray(),actualSplit.Chronology);
        var value=LegacyFreshMainPhysicalKernels.Reorder(input,owner.Scale,owner.Grouping,owner.PhysicalResolution,
            spatial30Resource,physical50Resource,cancellationToken);var buffer=value.Buffer;
        return new(actualSplit,buffer.Strokes,buffer.OriginalPhysical,buffer.Probabilities,buffer.Descriptors,buffer.Chronology,value.Calls);
    }
    private static void CheckHash(ReadOnlySpan<byte> data,int size,string expected)
    {
        if(data.Length!=size||!Convert.ToHexString(SHA256.HashData(data)).Equals(expected,StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Actual checked embedded2E3 model required.");
    }
}
