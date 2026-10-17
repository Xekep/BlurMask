using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("LegacyMainPostResultCounts.Probe")]

namespace InkRecognition;

public enum LegacyNativeGapAllocationBranch
{
    ZeroShortcut,
    BoundedObservationBuffers,
    RefusedOversizedAllocation,
    RefusedUnderCapacity
}

/// <summary>177280/175350 scalar counts, before any observation allocation.
/// These values do not execute shear geometry, EM or main text selection.</summary>
public sealed class LegacyNativeMainPostResultCounts
{
    public uint ShearGapCount { get; }
    public float ShearInitialBlend { get; }
    public uint GapAllocationCount { get; }
    public uint QualifiedAllocationCount { get; }
    public float GapInitialBlend { get; }
    public uint ProducedGapCount { get; }
    public uint ProducedQualifiedGapCount { get; }
    public int EmptyDescriptorCount { get; }
    public LegacyNativeGapAllocationBranch GapBranch { get; }
    private readonly float zeroGap;

    internal LegacyNativeMainPostResultCounts(uint total,uint qualified,uint produced,uint producedQualified,
        int empty,float shearBlend,float gapBlend,float zeroGap,LegacyNativeGapAllocationBranch branch)
    {
        ShearGapCount=GapAllocationCount=total;QualifiedAllocationCount=qualified;
        ProducedGapCount=produced;ProducedQualifiedGapCount=producedQualified;EmptyDescriptorCount=empty;
        ShearInitialBlend=shearBlend;GapInitialBlend=gapBlend;this.zeroGap=zeroGap;GapBranch=branch;
    }

    /// <summary>Reject before allocating an oversized buffer or a buffer whose
    /// declared count cannot hold the observations. No reset/skip is invented.</summary>
    public void RequireBoundedGapAllocation()
    {
        if(GapBranch==LegacyNativeGapAllocationBranch.RefusedOversizedAllocation)
            throw new NotSupportedException("Native unsigned gap allocation exceeds the portable4096-observation budget.");
        if(GapBranch==LegacyNativeGapAllocationBranch.RefusedUnderCapacity)
            throw new NotSupportedException("Native wrapped gap buffer counts do not cover the produced observations.");
    }

    /// <summary>175491 tests the wrapped total for zero before either buffer
    /// allocation, including when the qualified counter has wrapped.</summary>
    public float ReadNativeZeroGap()
    {
        if(GapBranch!=LegacyNativeGapAllocationBranch.ZeroShortcut)
            throw new InvalidOperationException("Native total-zero gap shortcut required.");
        return zeroGap;
    }
}

/// <summary>Count policy over the actual current main0 result owner. Preserves
/// unsigned empty-interval arithmetic; it does not widen existing24D/29B APIs.</summary>
public sealed class LegacyFreshMainPostResultCountPolicy
{
    public LegacyFreshMainFirstResultLineSet Source { get; }
    public LegacyNativeMainPostResultCounts Counts { get; }
    public int Processor24DOffset { get; }
    public int Processor29BOffset { get; }
    private LegacyFreshMainPostResultCountPolicy(LegacyFreshMainFirstResultLineSet source,
        LegacyNativeMainPostResultCounts counts,int shear,int gap)
    {Source=source;Counts=counts;Processor24DOffset=shear;Processor29BOffset=gap;}

    public static LegacyFreshMainPostResultCountPolicy Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainFirstResultLineSet actualLines,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualLines);token.ThrowIfCancellationRequested();
        if(actualLines.Owner.Controller.SourceOffset!=0 || !StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(resource)),actualLines.Owner.Input.Input.Input.SourceFingerprint))
            throw new ArgumentException("Exact fresh main0 result owner and resource required.");
        int shearOffset=checked(LegacyFreshMainGeometricLineFallback.ReadMainProcessor(resource)+36),gapOffset=checked(shearOffset+38);
        var shear=LegacyOrdinaryShearProfile.ReadV100Node(resource,shearOffset);
        var gap=LegacyOrdinaryGapProfile.ReadV100Node(resource,gapOffset);
        var descriptors=actualLines.CopyDescriptors();var guides=actualLines.CopyGuides();
        foreach(var descriptor in descriptors)
        {
            token.ThrowIfCancellationRequested();int kind=U16(descriptor,0)&15;if(kind==3)continue;
            int line=I32(descriptor,16);
            if(kind!=0 || line<0 || line>=guides.Length || guides[line].Length!=40)
                throw new NotSupportedException("Actual fresh assigned kind0 descriptors required after24A.");
            float baseline=BinaryPrimitives.ReadSingleLittleEndian(guides[line]),height=BinaryPrimitives.ReadSingleLittleEndian(guides[line].AsSpan(4));
            if(!float.IsFinite(baseline)||!float.IsFinite(height)||height<=0)
                throw new ArgumentException("Actual finite positive source guide required.");
        }
        foreach(var row in actualLines.PublishedRows)
        {
            token.ThrowIfCancellationRequested();
            if(I32(row.CopyStorage().Span,16)!=-1)
                throw new NotSupportedException("Fresh156950 unknown-source720 items required for qualified observations.");
        }
        var counts=EvaluateDescriptors(descriptors,actualLines.PublishedRows.Count,guides.Length,shear,gap,token);
        token.ThrowIfCancellationRequested();return new(actualLines,counts,shearOffset,gapOffset);
    }

    internal static LegacyNativeMainPostResultCounts EvaluateDescriptors(IReadOnlyList<byte[]> descriptors,
        int sourceRowCount,int guideCount,LegacyOrdinaryShearProfile shear,LegacyOrdinaryGapProfile gap,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(descriptors);ArgumentNullException.ThrowIfNull(shear);ArgumentNullException.ThrowIfNull(gap);
        token.ThrowIfCancellationRequested();shear.Validate();gap.Validate();
        if(descriptors.Count is <1 or >512 || sourceRowCount is <0 or >4096 || guideCount is <0 or >512)
            throw new ArgumentException("Bounded descriptor/source720/guide counts required.");
        //Snapshot before examining the native count fields. No count is used
        //as an array length; the only allocation scales with bounded input.
        var snapshot=descriptors.Select(d=>d is {Length:88}?d.ToArray():throw new ArgumentException("Descriptor88 required.")).ToArray();
        uint total=0,qualified=0,produced=0,producedQualified=0;int empty=0;
        foreach(var d in snapshot)
        {
            token.ThrowIfCancellationRequested();int kind=U16(d,0)&15;
            if(kind==3)continue;
            if(kind is not (0 or 6))throw new NotSupportedException("Ordinary/separator count branch required.");
            int line=I32(d,16);if(line<0)continue;
            int first=I32(d,52),end=I32(d,56);
            if(line>=guideCount || first<0 || end<first || end>sourceRowCount)
                throw new ArgumentException("Assigned source interval exceeds owned source storage.");
            uint count=(uint)(end-first),contribution=unchecked(count-1);
            total=unchecked(total+contribution);
            if(contribution>gap.MinimumGaps)qualified=unchecked(qualified+contribution);
            if(count==0)empty++;
            uint emitted=count==0?0:count-1;produced=checked(produced+emitted);
            if(contribution>gap.MinimumGaps)producedQualified=checked(producedQualified+emitted);
        }
        //Native total-zero return precedes BOTH allocations; qualified may be
        //UINT_MAX here and must not cause an invented refusal of this branch.
        var branch=total==0?LegacyNativeGapAllocationBranch.ZeroShortcut:
            total>4096 || qualified>4096?LegacyNativeGapAllocationBranch.RefusedOversizedAllocation:
            total!=produced || qualified!=producedQualified?LegacyNativeGapAllocationBranch.RefusedUnderCapacity:
            LegacyNativeGapAllocationBranch.BoundedObservationBuffers;
        return new(total,qualified,produced,producedQualified,empty,
            Blend(total,shear.MinimumGaps,shear.MaximumGaps),Blend(qualified,gap.MinimumGaps,gap.MaximumGaps),gap.ZeroGap,branch);
    }
    private static float Blend(uint count,byte minimum,byte maximum)=>Math.Clamp(((float)count-(float)minimum)/((float)maximum-(float)minimum),0f,1f);
    private static ushort U16(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
    private static int I32(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
}
