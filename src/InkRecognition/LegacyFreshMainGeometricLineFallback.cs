using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("LegacyMainGeometricFallback.Probe")]

namespace InkRecognition;

public readonly record struct LegacyMainFallbackLine(int DescriptorOrdinal, int LineId,
    LegacyAutomaticLineEstimate Estimate);

/// <summary>The native main24A continuation after actual249/24B publication.
/// A missing first word does not supply a replacement glyph:16E850 fits the
/// current parent's physical interval with the main source's own24A profile.</summary>
public sealed class LegacyFreshMainGeometricLineFallback
{
    public LegacyFreshMainFirstResultLineSet Input { get; }
    public LegacyFreshMainFirstResultLineSet Lines { get; }
    public int Processor24AOffset { get; }
    public IReadOnlyList<LegacyMainFallbackLine> CreatedLines { get; }

    private LegacyFreshMainGeometricLineFallback(LegacyFreshMainFirstResultLineSet input,
        LegacyFreshMainFirstResultLineSet lines, int offset, LegacyMainFallbackLine[] created)
    { Input=input; Lines=lines; Processor24AOffset=offset; CreatedLines=Array.AsReadOnly(created); }

    public static LegacyFreshMainGeometricLineFallback Apply(ReadOnlySpan<byte> resource,
        LegacyFreshMainFirstResultLineSet actualInput, CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualInput); token.ThrowIfCancellationRequested();
        var source=actualInput.Owner.Controller;
        if(source.SourceOffset!=0 || !StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(resource)),actualInput.Owner.Input.Input.Input.SourceFingerprint))
            throw new ArgumentException("Exact fresh main0 source and first-publication owner required.");
        int offset=ReadMainProcessor(resource); var profile=LegacyAutomaticLineProfile.ReadResourceNode(resource,offset);
        var output=ApplyPreparedDescriptors(profile,actualInput.CopyDescriptors(),actualInput.CopyGuides(),
            actualInput.CopyPhysicalPoints(),token);
        var lines=output.Created.Length==0 ? actualInput : new LegacyFreshMainFirstResultLineSet(actualInput.Owner,
            output.Descriptors,output.Guides,output.Points,actualInput.PublishedRows.ToArray(),
            actualInput.CurrentRows.ToArray(),actualInput.Outputs.ToArray());
        token.ThrowIfCancellationRequested(); return new(actualInput,lines,offset,output.Created);
    }

    //The fresh owner excludes reused sources, property events and explicit
    //guidelines.12D0070 forwards NEW2B3.byte10=0 to218.byte80, then secondary14.
    //The independent prepared controls exercise unknown descriptors without
    //pretending that a manually prepared row came from the first NN winner.
    internal static PreparedFallback ApplyPreparedDescriptors(LegacyAutomaticLineProfile profile,
        byte[][] descriptors, byte[][] guides, InkPoint[][] points, CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested(); profile.Validate();
        if(descriptors.Length is <1 or >4096 || guides.Length>4096 || points.Length is <1 or >256)
            throw new ArgumentException("Bounded main source storage required.");
        int total=0;
        foreach(var stroke in points)
        {
            if(stroke is null || stroke.Length is <1 or >4096 || stroke.Length>1_048_576-total)
                throw new ArgumentException("Bounded current physical point storage required.");
            total+=stroke.Length;
            foreach(var p in stroke)
            { token.ThrowIfCancellationRequested(); if(!float.IsFinite(p.X)||!float.IsFinite(p.Y))throw new ArgumentException("Finite current points required."); }
        }
        var ds=descriptors.Select(d=>d is {Length:88}?d.ToArray():throw new ArgumentException("Descriptor88 required.")).ToArray();
        var gs=guides.Select(g=>g is {Length:40}?g.ToArray():throw new ArgumentException("Guide40 required.")).ToList();
        var ps=points.Select(s=>s.ToArray()).ToArray(); var created=new List<LegacyMainFallbackLine>();
        int remainingVisits=1_048_576;
        for(int d=0;d<ds.Length;d++)
        {
            token.ThrowIfCancellationRequested(); var descriptor=ds[d]; int kind=U16(descriptor,0)&15;
            if(kind==3)continue;
            if(kind!=0)throw new NotSupportedException("Fresh ordinary kind0/line-separator kind3 descriptors required.");
            int line=I32(descriptor,16),first=I32(descriptor,36),last=I32(descriptor,40);
            if(first<0||last<first||last>=ps.Length||line>=gs.Count)
                throw new ArgumentException("Descriptor physical interval/line exceeds actual source storage.");
            if(line>=0)continue;
            if(gs.Count==4096)throw new ArgumentException("Portable main guide budget exceeded.");
            for(int s=first;s<=last;s++)
            {
                if(ps[s].Length>remainingVisits)throw new ArgumentException("Portable cumulative fallback point budget exceeded.");
                remainingVisits-=ps[s].Length;
            }
            var interval=ps[first..(last+1)]; var estimate=LegacyAutomaticLineEstimator.Estimate(interval,profile,token);
            var guide=new byte[40]; WF(guide,0,estimate.Line.Baseline); WF(guide,4,estimate.Line.Height);
            WF(guide,8,(float)estimate.Slope); WF(guide,12,estimate.Line.Baseline); WF(guide,16,estimate.Line.Height);
            //169750 always resets these identifiers before its disabled branch.
            BinaryPrimitives.WriteInt32LittleEndian(guide.AsSpan(32),-1);
            BinaryPrimitives.WriteInt32LittleEndian(guide.AsSpan(36),int.MinValue);
            line=gs.Count; BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(16),line);
            var deskew=LegacyLineDeskew.Apply(interval,estimate.Line,(float)estimate.Slope,token);
            WF(guide,0,deskew.Line.Baseline); WF(guide,4,deskew.Line.Height);
            for(int s=first;s<=last;s++)ps[s]=deskew.Strokes[s-first];
            gs.Add(guide); created.Add(new(d,line,estimate));
        }
        token.ThrowIfCancellationRequested(); return new(ds,gs.ToArray(),ps,created.ToArray());
    }

    internal sealed record PreparedFallback(byte[][] Descriptors,byte[][] Guides,InkPoint[][] Points,
        LegacyMainFallbackLine[] Created);

    internal static int ReadMainProcessor(ReadOnlySpan<byte> resource)
    {
        var source=LegacyOrdinarySourceInk.ReadV10dResourceProfile(resource,0);
        int at=checked(source.EnclosureFlagOffset+12);
        if(I32(resource,at)!=6)throw new InvalidDataException("Actual six-stage main source required.");
        at+=4; int selected=-1;
        foreach(int kind in new[]{0x249,0x24b,0x24a,0x24d,0x29b,0x265})
        {
            if(at<0||at>resource.Length-16||I32(resource,at)!=0x101fe90||I32(resource,at+8)!=kind)
                throw new InvalidDataException("Ordered main source processor ownership required.");
            int length=I32(resource,at+4);
            if(length<8||length>resource.Length-at-8)throw new InvalidDataException("Truncated main processor.");
            if(kind==0x24a)selected=at; at=checked(at+length+8);
        }
        return selected;
    }
    private static ushort U16(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
    private static int I32(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static void WF(Span<byte> b,int at,float value)=>BinaryPrimitives.WriteSingleLittleEndian(b[at..],value);
}
