using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Actual fresh15B260 descriptor publication and16AA00 physical/point
/// reordering before first249 and later15F050. Provider metadata is derived from
/// the owned unknown-line2E3 result, not caller strings or guessed row IDs.
/// No later physical insertion, line guide, first winner or main decoding is added.</summary>
public sealed class LegacyFreshMainDescriptorSplit
{
    private readonly byte[][] descriptors;
    private readonly InkStroke[] strokes;
    private readonly int[] originalPhysical;
    private readonly float[] probabilities;
    public LegacyFreshMainPhysicalGrouping Input {get;}
    public IReadOnlyList<InkStroke> Strokes {get;}
    public ReadOnlyMemory<int> OriginalPhysicalStrokeIndices=>originalPhysical;
    public ReadOnlyMemory<float> SourceStrokeProbabilities=>probabilities;
    public LegacySourcePointChronology Chronology {get;}
    public int DescriptorCount=>descriptors.Length;
    public int OrdinaryDescriptorCount=>(descriptors.Length+1)/2;
    public byte[][] CopyDescriptors()=>descriptors.Select(d=>d.ToArray()).ToArray();
    private LegacyFreshMainDescriptorSplit(LegacyFreshMainPhysicalGrouping input,byte[][] descriptors,
        InkStroke[] strokes,int[] originalPhysical,float[] probabilities,LegacySourcePointChronology chronology)
    {Input=input;this.descriptors=descriptors;this.strokes=strokes;Strokes=Array.AsReadOnly(strokes);
        this.originalPhysical=originalPhysical;this.probabilities=probabilities;Chronology=chronology;}

    /// <summary>Fresh source has no existing strings/source720/page guide/prior
    /// source. Provider98 is null and knownCount zero, so07A850 supplies exact
    /// group metadata20=INT_MIN,24=?1,28=1. No opaque provider metadata is assumed.
    /// Physical source keys survive the actual ascending-ID group reorder.</summary>
    public static LegacyFreshMainDescriptorSplit Prepare(LegacyFreshMainPhysicalGrouping actualGrouping,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualGrouping);cancellationToken.ThrowIfCancellationRequested();
        var packet=actualGrouping.Packet;var source=packet.Source;var selected=actualGrouping.Grouping.Selected;
        if(!ReferenceEquals(actualGrouping.Scale.Source,source)||!ReferenceEquals(actualGrouping.Grouping.Source,actualGrouping.Scale)||
            packet.DescriptorCount!=1||packet.Descriptor.Flags!=0x10||packet.Descriptor.CurrentLineId!=-1||
            packet.Descriptor.FirstPhysicalStroke!=0||packet.Descriptor.LastPhysicalStroke!=source.Strokes.Count-1)
            throw new ArgumentException("Actual owned fresh main direct-source grouping required.");
        var value=LegacyFreshMainPhysicalKernels.Split(packet,selected,cancellationToken);
        return new(actualGrouping,value.Descriptors,value.Strokes,value.OriginalPhysical,value.Probabilities,value.Chronology);
    }

    /// <summary>Native descriptors24/28 are current physical ranges after the
    /// source reorder. Separators are not ink owners and are never returned.</summary>
    public int GetDescriptorOrdinalForPhysicalStroke(int physicalStroke)
    {
        if((uint)physicalStroke>=(uint)strokes.Length)throw new ArgumentOutOfRangeException(nameof(physicalStroke));
        for(int d=0;d<descriptors.Length;d+=2)
            if(physicalStroke>=BinaryPrimitives.ReadInt32LittleEndian(descriptors[d].AsSpan(36))&&
                physicalStroke<=BinaryPrimitives.ReadInt32LittleEndian(descriptors[d].AsSpan(40)))return d;
        throw new InvalidDataException("Physical stroke lacks ordinary descriptor ownership.");
    }
}
