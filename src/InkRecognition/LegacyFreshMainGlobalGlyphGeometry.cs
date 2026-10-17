using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

public readonly record struct LegacyMainGlyphSourceFrame(int DescriptorOrdinal,int LineId,float Baseline,float Height,
    int? SecondaryDescriptorOrdinal=null,int? SecondaryLineId=null);

/// <summary>Actual global ordinary group→source guide/chronology→FF/FF short
/// normalization. Retains the primary descriptor and combines both assigned
/// guides' vertical intervals for a cross-source normalization frame.
/// No dictionary owner, seed membership or classifier result is manufactured.</summary>
public sealed class LegacyFreshMainGlobalGlyphGeometry
{
    private readonly InkStroke[] strokes;
    private readonly int[] counts;
    private readonly LegacySourcePointChronology chronology;
    private readonly LegacyOrdinarySourceInk original;
    public LegacyFreshMainGlobalContext Context {get;}
    public LegacyFreshMainCompactOrdinaryGraph Graph=>Context.Graph;
    public LegacySourceSamplerCategoryOwner CategoryOwner=>Graph.Source.Source.CategoryOwner;
    public short TargetHeight {get;}
    private LegacyFreshMainGlobalGlyphGeometry(LegacyFreshMainGlobalContext context,InkStroke[] strokes,
        LegacySourcePointChronology chronology,LegacyOrdinarySourceInk original,short target)
    {Context=context;this.strokes=strokes;counts=strokes.Select(s=>s.Points.Count).ToArray();this.chronology=chronology;this.original=original;TargetHeight=target;}

    public static LegacyFreshMainGlobalGlyphGeometry Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalContext context,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(context);token.ThrowIfCancellationRequested();var source=context.Graph.Source.Source;
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(source.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            source.AcousticOffset!=189734||source.CategoryOwner.SelectedSourceOffset!=0||source.SpanProfile.MaximumAtoms!=8)
            throw new ArgumentException("Same actual fresh global main source/geometry resource required.");
        int at=source.AcousticOffset;
        if(BinaryPrimitives.ReadInt32LittleEndian(resource[at..])!=0x101fe90||BinaryPrimitives.ReadInt32LittleEndian(resource[(at+8)..])!=0x21d||
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(at+14)..])!=0x110||resource[at+16]!=255||resource[at+17]!=255)
            throw new InvalidDataException("Actual main21D/v110 FF/FF normalization required.");
        short target=BinaryPrimitives.ReadInt16LittleEndian(resource[(at+18)..]);if(target<=0)throw new InvalidDataException("Actual positive main normalization target required.");
        var strokes=source.CopyStrokes().Select(s=>new InkStroke(s.Points.ToArray())).ToArray();
        var original=source.Preparation.Input.Input.Input.Owner.Controller.Source;
        return new(context,strokes,source.Chronology,original,target);
    }

    public LegacyMainGlyphSourceFrame ReadFrame(int group,CancellationToken token=default)
    {
        var sources=Graph.ResolveGroupSource(group,token);var primary=sources.Primary;var secondary=sources.Secondary;
        if(secondary is null)return new(primary.DescriptorOrdinal,primary.LineId,primary.Baseline,primary.Height);
        float baseline=Math.Max(primary.Baseline,secondary.Baseline);
        float top=Math.Min(primary.Baseline-primary.Height,secondary.Baseline-secondary.Height);
        return new(primary.DescriptorOrdinal,primary.LineId,baseline,baseline-top,secondary.DescriptorOrdinal,secondary.LineId);
    }
    public LegacyInkRange[] CopyChronologicalRanges(int group,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if((uint)group>=(uint)Graph.State.Groups.Count)throw new ArgumentOutOfRangeException(nameof(group));
        var ranges=Graph.State.Groups[group].CopyRanges();var order=chronology.OrderRangeIndicesForNormalization(ranges,token);
        return order.Select(i=>ranges[i]).ToArray();
    }
    public float ReadMinimumX(int group,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if((uint)group>=(uint)Graph.State.Groups.Count)throw new ArgumentOutOfRangeException(nameof(group));
        return Graph.State.Groups[group].Bounds.MinimumX;
    }
    public LegacyNormalizedPreparedGlyph NormalizeGroup(int group,CancellationToken token=default)
    {
        var frame=ReadFrame(group,token);var ranges=CopyChronologicalRanges(group,token);
        return LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(CopyPreparedInk(token),ranges,ReadMinimumX(group,token),
            frame.Baseline,frame.Height,TargetHeight,Graph.State.MaximumAtoms,token);
    }
    public LegacyCursiveGlyphBoundarySigns ReadBoundarySigns(int group,CancellationToken token=default)
        =>LegacyCursiveGlyphBoundaryPolicy.Resolve(CopyChronologicalRanges(group,token),counts,0,false,false,token);
    public LegacySourcePrimitiveScope ReadPrimitiveScope(int group,CancellationToken token=default)
    {
        var ranges=CopyChronologicalRanges(group,token);bool whole=false,zero=false;int physical=-1;float? probability=null;
        if(ranges.Length==1)
        {
            var range=ranges[0];whole=range.StartPoint==0&&range.EndPointExclusive==counts[range.StrokeIndex];
            if(whole)
            {
                physical=range.StrokeIndex;probability=original.SourceStrokeProbabilities.Span[physical];
                zero=chronology.Resolve(physical,0).Key.Third==0&&original.ResolveEndingMetadata(chronology.Resolve(physical,range.EndPointExclusive-1),token).Key.Third==0;
            }
        }
        return new(whole,zero,physical,probability);
    }
    /// <summary>The later confidence producer reads source probability from
    /// the first STORED range, separately from whole-normalized-stroke scope.</summary>
    public float ReadFirstStoredRangeProbability(int group,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if((uint)group>=(uint)Graph.State.Groups.Count)throw new ArgumentOutOfRangeException(nameof(group));
        var ranges=Graph.State.Groups[group].CopyRanges();if(ranges.Length==0)throw new InvalidDataException("Actual ordinary group has no stored range.");
        return original.SourceStrokeProbabilities.Span[ranges[0].StrokeIndex];
    }
    //Fresh copies keep public normalized output from exposing this source's
    //retained coordinate buffers to mutable-memory recovery by a consumer.
    internal LegacyCursivePreparedInk CopyPreparedInk(CancellationToken token=default)=>LegacyCursivePreparedInk.Create(strokes,token);
}
