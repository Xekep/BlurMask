using System.Buffers.Binary;
using System.Collections.ObjectModel;

namespace InkRecognition;

/// <summary>One actual published word or separator in the selected first path.
/// Numerical deltas belong to1F42D0; a delimiter word uses ctor scalars1/1/0/0.</summary>
public sealed record LegacyFirstPublishedTextWord(LegacyNativePublishedWordNode Published,
    bool IsDelimiter,int TotalCostDelta,int AcousticCostDelta,int SourceNodeSpan);

/// <summary>Fresh literal selected first TEXT. All words and zero-span detached
/// records share one actual273 pool. This is not the main language result.</summary>
public sealed class LegacyFirstMaterializedPlainText
{
    public LegacyFreshFirstOwnedRecognition Owner {get;}
    public LegacyFreshFirstPlainResult Result {get;}
    public LegacySmoothedRowCutResult Cuts=>Owner.Cuts;
    public LegacyOrdinaryCursiveCandidateGraph Graph=>Owner.Prepared.SourceRanges.Graph;
    public LegacyFirstTerminalChoice Selection {get;}
    public IReadOnlyList<LegacyFirstPublishedTextWord> Words {get;}
    public IReadOnlyList<ulong> AllocatedGlyphReferences {get;}
    public IReadOnlyList<ulong> DetachedDelimiterReferences {get;}
    public IReadOnlyDictionary<ulong,float> OwnerFirstScores1C {get;}
    public float MinimumOwnerFirstScore80 {get;}
    internal LegacyFirstMaterializedPlainText(LegacyFreshFirstOwnedRecognition owner,LegacyFreshFirstPlainResult result,
        LegacyFirstTerminalChoice selection,LegacyFirstPublishedTextWord[] words,ulong[] glyphs,ulong[] detached,
        Dictionary<ulong,float> scores,float minimum)
    {Owner=owner;Result=result;Selection=selection;Words=Array.AsReadOnly(words);AllocatedGlyphReferences=Array.AsReadOnly(glyphs);
        DetachedDelimiterReferences=Array.AsReadOnly(detached);OwnerFirstScores1C=new ReadOnlyDictionary<ulong,float>(scores);MinimumOwnerFirstScore80=minimum;}
}

/// <summary>Fresh6C/noE0 first1F42D0 selected-path word publication. The owning
/// recognizer supplies actual primary/B8 and scores; there is no public table,
/// label, boundary, confidence or delimiter injection entry.</summary>
internal static class LegacyFirstPlainTextMaterializer
{
    internal static LegacyFirstMaterializedPlainText Materialize(ReadOnlySpan<byte> resource,
        LegacyFreshFirstOwnedRecognition owner,LegacyCursiveSeparationTable table,
        LegacyFirstOrdinaryAcousticRecurrence primary,LegacyFirstOrdinaryContextPasses merged,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var factory=owner.Factory;var graph=owner.Prepared.SourceRanges.Graph;var cuts=owner.Cuts;
        var binding=factory.InvocationPolicy.Binding;var invocation=factory.InvocationPolicy.Invocation;
        if(!factory.Configuration6C||binding.NativeModeE0||merged.ColumnCountB8!=1||primary.NodeCount!=graph.NodeCount||
            merged.NodeCount!=graph.NodeCount||primary.BeamWidth!=merged.BeamWidth)
            throw new NotSupportedException("Fresh literal one-column first tables required.");
        var grid=new LegacyFirstMergedRecordGrid(merged.ColumnCountB8,merged.BeamWidth,merged.CopyB8Storage(),token);
        for(int n=0;n<grid.NodeCount;n++)for(int r=0;r<grid.RowCountF8;r++)
        {
            token.ThrowIfCancellationRequested();var cell=grid.Record(n,r,0);
            if(I(cell,0)<0x4000000&&(cell[32]!=0||U(cell,12)==binding.TrailingSymbolE6))
                throw new NotSupportedException("Trait/shadda first TEXT branch is not restored.");
        }
        var selected=LegacyFirstTerminalSelection.Select(grid,new(binding.TrailingSymbolE6,0xa000,
            invocation.Configuration99,invocation.ConfigurationA8,binding.SourceKind,0),token);
        if(selected.Choices.Count!=1)throw new NotSupportedException("This first TEXT branch requires one actual surviving terminal choice.");
        var choice=selected.Choices[0];var trace=LegacyFirstSelectedGlyphTrace.Read(grid,choice,token);
        if(trace.Count is <1 or >2047)throw new NotSupportedException("Portable first TEXT path budget is1..2047 cells including possible interior separators.");
        var literal=LegacyFreshFirstGlyphSymbolPlan.ReadSource(resource,binding.SourceOffset,factory,token);
        var allocation=LegacyFreshFirstSelectedGlyphPublication.Create(factory,token);
        var interior=LegacyFirstOwnedInteriorDelimiter.Prepare(resource,owner,token);
        var endpoints=new LegacyFirstPlainWordDelimiters(graph);
        var confidence=LegacyFirstOrdinaryGlyphScoreProducer.ReadFreshSelectedSource(resource,binding.SourceOffset,token);
        var ownedConfidence=new LegacyFirstOwnedGlyphConfidence(confidence,factory,cuts,graph,owner.GroupScores,token);
        var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,binding.SourceOffset,token);
        var separation=LegacyCursiveSeparationProfile.ReadV106Node(resource,profile.SamplerOffset);
        ushort[] bypass=new ushort[8];int at=0;
        foreach(string text in new[]{"\U000f0001","\U000f0002","\U000f0003","\U000f0004","\U000f0008"})
            bypass[at++]=binding.Alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
        byte[] header=binding.Alphabet.CopySerializedHeader();for(int k=4;k<=6;k++)bypass[at++]=U(header,k*2);
        ushort blank=U(header,0);var caches=new LegacyNativeGlyphScoreCache?[graph.Groups.Count];
        LegacyNativeGlyphScoreCache Cache(int group)
        {
            if(caches[group] is {} existing)return existing;
            var scores=owner.GroupScores[group];factory.ValidateScores(scores.Glyph.Scores,token);
            if(!ReferenceEquals(scores.Glyph.Geometry.PreparedInk,cuts.Cuts.PreparedInk))throw new ArgumentException("Foreign scored first source.");
            var ranges=graph.Groups[group].Ranges.ToArray();int[] order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges,token);
            if(!scores.Glyph.Geometry.Ranges.Span.SequenceEqual(order.Select(i=>ranges[i]).ToArray()))throw new ArgumentException("Foreign scored first ranges.");
            int geometryCost=LegacyCursiveGroupSeparationCost.Evaluate(cuts.Cuts.PreparedInk,ranges,factory.Frame.Row.Frame.Line.Height,
                separation.GapOffset,separation.Slope,table.Values.Span,separation.MaximumCost,cancellationToken:token);
            var candidates=scores.Cache.Candidates.Span;byte[] rows=new byte[candidates.Length*24];var vectors=new ReadOnlyMemory<float>[candidates.Length];
            for(int i=0;i<candidates.Length;i++)
            {
                token.ThrowIfCancellationRequested();BinaryPrimitives.WriteInt32LittleEndian(rows.AsSpan(i*24+4),candidates[i].Cost);
                BinaryPrimitives.WriteUInt16LittleEndian(rows.AsSpan(i*24+8),candidates[i].Symbol);
                vectors[i]=new float[]{ownedConfidence.ReadSymbolScore(group,candidates[i].Symbol,token)};
            }
            if(vectors.Length==0)throw new NotSupportedException("Finite actual first candidate required.");
            return caches[group]=new(rows,vectors,Enumerable.Repeat(geometryCost,profile.Categories.Prototypes.Length).ToArray(),
                blank,scores.Cache.FallbackCost,1,bypass,token);
        }
        var cacheOperations=new LegacyFirstPlainGlyphCache?[graph.Groups.Count];
        LegacyFirstPlainGlyphCache ProjectCache(int group)
        {
            if(cacheOperations[group] is{} existing)return existing;
            var actual=Cache(group);
            return cacheOperations[group]=new((symbol,channel,ct)=>actual.ReadSymbolScore(symbol,checked((byte)channel),ct),
                actual.ReadRequestCost,actual.ReadFirstCandidateScore);
        }
        var result=factory.CreatePlainResult(token);
        var operations=new LegacyFirstPlainTextOperations(allocation.ShouldAllocate,literal.Create,literal.Apply,ProjectCache,interior.Read);
        var numeric=LegacyNativeFirstPlainTextKernel.Run(new(result.GlyphPool,result.WordPublisher),graph,primary,
            choice,trace,grid.ColumnCountA4,binding.SourceKind,operations,new byte[120],new byte[40],token);
        return new(owner,result,choice,numeric.Words,numeric.Glyphs,numeric.Detached,numeric.OwnerFirstScores,numeric.MinimumFirstScore);
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static ushort U(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
    private static void W16(byte[]b,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(at),value);
    private static void WF(byte[]b,int at,float value)=>BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(at),value);
}
