using System.Buffers.Binary;
using System.Collections.ObjectModel;

namespace InkRecognition;

public sealed record LegacyFirstMaterializedPlainWord(LegacyFreshFirstPlainResult Result,
    LegacySmoothedRowCutResult Cuts,LegacyOrdinaryCursiveCandidateGraph Graph, LegacyNativePublishedWordNode PublishedWord, IReadOnlyList<ulong> GlyphReferences,
    ulong DetachedTrailingReference, ulong DetachedLeadingReference,
    IReadOnlyDictionary<ulong,float> OwnerFirstScores1C, float MinimumOwnerFirstScore80,
    LegacyFirstTerminalChoice Selection);

public readonly record struct LegacyFirstPlainWordScalars(float LogMass10,float Calibration14,
    float NormalizedUnadjustedCost18,float NormalizedAcousticCost1C)
{
    /// <summary>Native root word publication uses the unadjusted cost carried
    /// in local45C/44C, independently from TOP273's relative-cost anchor.</summary>
    public static LegacyFirstPlainWordScalars ReadUnadjusted(int totalCost,int acousticCost,int sourceNodeCount,int sourceKind)
    {
        if(sourceNodeCount is <1 or >4096)throw new ArgumentOutOfRangeException(nameof(sourceNodeCount));
        float multiplier=sourceKind==0x135?5f:1f;
        return new(-multiplier*(float)totalCost*0.00012207031f,1,(float)totalCost/(float)(sourceNodeCount<<13),
            (float)acousticCost*0.00012207031f);
    }
}

/// <summary>Fresh first23f single plain word subset of1f42d0. Consumes actual
/// owned first NN/cache groups and prepared B8/primary98 tables; it is not the
/// whole controller, main240 language decoder, source720 or final SDK result.
/// Both zero-span endpoint records remain pool-owned but reset out of the
/// selected word. Owner64.1C scores are separate from individual glyph40 rows.</summary>
public sealed class LegacyFirstPlainWordMaterializer
{
    private readonly LegacyFreshFirstResultOwnershipFactory factory;
    private readonly LegacyFirstOrdinaryGlyphScoreProducer confidence;
    private readonly LegacyFirstTransitionGeometryProfile profile;
    private readonly LegacyCursiveSeparationProfile separation;
    private readonly LegacyCursiveSeparationTable table;
    private readonly ushort[] bypass;
    private readonly ushort blank;

    public LegacyFirstPlainWordMaterializer(ReadOnlySpan<byte> actualResource,int sourceOffset,
        LegacyFreshFirstResultOwnershipFactory factory,LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(factory);ArgumentNullException.ThrowIfNull(table);
        cancellationToken.ThrowIfCancellationRequested();
        var model=LegacyFirstAcousticModel.ReadSource(actualResource,sourceOffset,cancellationToken);
        if(model.SourceFingerprint!=factory.SourceFingerprint||sourceOffset!=factory.InvocationPolicy.Binding.SourceOffset)
            throw new ArgumentException("Materializer resource and fresh factory must own the same selected first source.");
        _=LegacyOrdinaryAcousticCacheProfile.ReadFirstSource(actualResource,sourceOffset,out _,cancellationToken);
        confidence=LegacyFirstOrdinaryGlyphScoreProducer.ReadFreshSelectedSource(actualResource,sourceOffset,cancellationToken);
        profile=LegacyFirstTransitionGeometryProfile.ReadSource(actualResource,sourceOffset,cancellationToken);
        separation=LegacyCursiveSeparationProfile.ReadV106Node(actualResource,profile.SamplerOffset);
        var alphabet=factory.InvocationPolicy.Binding.Alphabet;
        int a=sourceOffset+20+4+56;int marker=BinaryPrimitives.ReadInt32LittleEndian(actualResource[a..]);a+=4;if(marker==-1)a+=4;a+=11;
        var header=actualResource.Slice(a,16);blank=BinaryPrimitives.ReadUInt16LittleEndian(header);
        bypass=new ushort[8];int i=0;
        foreach(string symbol in new[]{"\U000f0001","\U000f0002","\U000f0003","\U000f0004","\U000f0008"})
            bypass[i++]=alphabet.TryGetLabel(symbol,out ushort label)?label:ushort.MaxValue;
        for(int slot=4;slot<=6;slot++)bypass[i++]=BinaryPrimitives.ReadUInt16LittleEndian(header[(slot*2)..]);
        this.factory=factory;this.table=table;
    }

    public LegacyFirstMaterializedPlainWord Materialize(LegacySmoothedRowCutResult cuts,
        LegacyOrdinaryCursiveCandidateGraph graph,LegacyFirstOrdinaryAcousticRecurrence primary98,
        LegacyFirstOrdinaryContextPasses merged,IReadOnlyList<LegacyFirstOrdinaryCacheScores> actualGroupScores,
        CancellationToken cancellationToken=default)
        =>MaterializeCore(cuts,graph,primary98,merged,actualGroupScores,null,cancellationToken);

    /// <summary>Fresh6C literal single-word extension, including actual primitive
    /// labels. Uses the caller's matching source bytes to bind literal policy;
    /// the strict original API keeps its existing alphabet/symbol restrictions.
    /// This still consumes prepared selected B8, not the complete controller.</summary>
    public LegacyFirstMaterializedPlainWord MaterializeLiteralSymbols(ReadOnlySpan<byte> actualResource,int sourceOffset,
        LegacySmoothedRowCutResult cuts,LegacyOrdinaryCursiveCandidateGraph graph,
        LegacyFirstOrdinaryAcousticRecurrence primary98,LegacyFirstOrdinaryContextPasses merged,
        IReadOnlyList<LegacyFirstOrdinaryCacheScores> actualGroupScores,CancellationToken cancellationToken=default)
        =>MaterializeCore(cuts,graph,primary98,merged,actualGroupScores,
            LegacyFreshFirstGlyphSymbolPlan.ReadSource(actualResource,sourceOffset,factory,cancellationToken),cancellationToken);

    private LegacyFirstMaterializedPlainWord MaterializeCore(LegacySmoothedRowCutResult cuts,
        LegacyOrdinaryCursiveCandidateGraph graph,LegacyFirstOrdinaryAcousticRecurrence primary98,
        LegacyFirstOrdinaryContextPasses merged,IReadOnlyList<LegacyFirstOrdinaryCacheScores> actualGroupScores,
        LegacyFreshFirstGlyphSymbolPlan? literalSymbols,CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cuts);ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(primary98);ArgumentNullException.ThrowIfNull(merged);ArgumentNullException.ThrowIfNull(actualGroupScores);
        cancellationToken.ThrowIfCancellationRequested();
        if(!ReferenceEquals(cuts.Owner,factory.Frame.Row)||graph.NodeCount!=primary98.NodeCount||graph.NodeCount!=merged.NodeCount||
            primary98.BeamWidth!=merged.BeamWidth||actualGroupScores.Count!=graph.Groups.Count||merged.ColumnCountB8!=1||
            !factory.Configuration6C||factory.InvocationPolicy.Binding.NativeModeE0)
            throw new NotSupportedException("Matching fresh first source, actual primary/B8 and one-column/no-E0 plain word required.");
        var grid=new LegacyFirstMergedRecordGrid(merged.ColumnCountB8,merged.BeamWidth,merged.CopyB8Storage(),cancellationToken);
        //All finite cells must have no traits or trailing shadda. Therefore the
        //ctorF4 penalty is unused. Fresh invocation99/A8 and actual sourcekind
        //come from the owned binder. Reference78 is overwritten by the first
        //surviving choice under this fresh branch, rather than being inferred.
        var binding=factory.InvocationPolicy.Binding;var invocation=factory.InvocationPolicy.Invocation;
        for(int n=0;n<grid.NodeCount;n++)for(int r=0;r<grid.RowCountF8;r++)
        {
            cancellationToken.ThrowIfCancellationRequested();var cell=grid.Record(n,r,0);
            if(I32(cell,0)<0x4000000&&(cell[32]!=0||U16(cell,12)==binding.TrailingSymbolE6))
                throw new NotSupportedException("Trait/shadda terminal materialization requires its separate native branch.");
        }
        var selection=LegacyFirstTerminalSelection.Select(grid,new(binding.TrailingSymbolE6,0xa000,
            invocation.Configuration99,invocation.ConfigurationA8,binding.SourceKind,0),cancellationToken);
        if(selection.Choices.Count!=1)throw new NotSupportedException("This fresh single-word endpoint requires one surviving terminal choice.");
        var choice=selection.Choices[0];var trace=LegacyFirstSelectedGlyphTrace.Read(grid,choice,cancellationToken);
        if(trace.Count==0||trace.Count>4094)throw new ArgumentException("Plain materialization requires1..4094 selected glyphs plus two detached endpoints.");
        var ownedConfidence=new LegacyFirstOwnedGlyphConfidence(confidence,factory,cuts,graph,actualGroupScores,cancellationToken);
        var scalarCaches=new LegacyNativeGlyphScoreCache?[graph.Groups.Count];
        LegacyNativeGlyphScoreCache ScoreCache(int group)
        {
            if(scalarCaches[group] is {} existing)return existing;
            var scores=actualGroupScores[group];factory.ValidateScores(scores.Glyph.Scores,cancellationToken);
            var geometry=scores.Glyph.Geometry;
            if(!ReferenceEquals(geometry.PreparedInk,cuts.Cuts.PreparedInk))throw new ArgumentException("Group NN geometry belongs to another prepared cut source.");
            var ranges=graph.Groups[group].Ranges.ToArray();int[] order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges,cancellationToken);
            if(!geometry.Ranges.Span.SequenceEqual(order.Select(i=>ranges[i]).ToArray()))throw new ArgumentException("Graph group and NN ranges differ.");
            int cost=LegacyCursiveGroupSeparationCost.Evaluate(cuts.Cuts.PreparedInk,ranges,factory.Frame.Row.Frame.Line.Height,
                separation.GapOffset,separation.Slope,table.Values.Span,separation.MaximumCost,cancellationToken:cancellationToken);
            byte[] rows=new byte[scores.Cache.Candidates.Length*24];var vectors=new ReadOnlyMemory<float>[scores.Cache.Candidates.Length];
            for(int i=0;i<vectors.Length;i++)
            {
                cancellationToken.ThrowIfCancellationRequested();var candidate=scores.Cache.Candidates.Span[i];
                BinaryPrimitives.WriteInt32LittleEndian(rows.AsSpan(i*24+4),candidate.Cost);
                BinaryPrimitives.WriteUInt16LittleEndian(rows.AsSpan(i*24+8),candidate.Symbol);
                vectors[i]=new float[]{ownedConfidence.ReadSymbolScore(group,candidate.Symbol,cancellationToken)};
            }
            if(vectors.Length==0)throw new NotSupportedException("Owner first-candidate scalar requires an actual retained candidate.");
            //Validated first profile raw100=0 selects native group1C=1;
            //195520 writes the same exact separation cost for each prototype.
            return scalarCaches[group]=new(rows,vectors,Enumerable.Repeat(cost,profile.Categories.Prototypes.Length).ToArray(),
                blank,scores.Cache.FallbackCost,1,bypass,cancellationToken);
        }
        int First(int node,ushort arc)=>unchecked((int)graph.GetNode(node)[arc].StartContextId);
        int Last(int node,ushort arc)=>unchecked((int)graph.GetNode(node)[arc].EndContextId);
        var delimiters=new LegacyFirstPlainWordDelimiters(graph);var result=factory.CreatePlainResult(cancellationToken);
        var word=LegacyNativeWordBuilder.CreateEmpty(result.GlyphPool,new byte[120],cancellationToken);
        var ownerScores=new Dictionary<ulong,float>();var references=new List<ulong>();
        ulong Endpoint(int node,ushort arc,bool leading)
        {
            int first=First(node,arc),last=Last(node,arc);ulong key=(uint)last&0xffffff;
            key|=(ulong)(ushort)(first-last)<<24;if(leading)key|=1UL<<56;
            var endpoint=word.AppendGlyph(key,-2,!leading,new byte[40],cancellationToken);
            var delimiter=leading?delimiters.ReadEndpoint(-1,ushort.MaxValue,node,arc,256,cancellationToken):
                delimiters.ReadEndpoint(node,arc,graph.NodeCount,ushort.MaxValue,256,cancellationToken);
            _=LegacyNativeGlyphSymbolUpdate.Apply(result.GlyphPool,endpoint.Reference,ushort.MaxValue,delimiter.EmittedSymbolCount,
                delimiter.ComponentFlag,delimiter.ExternalVariant!=0,cancellationToken);
            byte[] row=result.GlyphPool.CopyRecord(endpoint.Reference,cancellationToken);W16(row,6,0x40);WF(row,12,1);WF(row,16,1);
            result.GlyphPool.ReplaceRecord(endpoint.Reference,row,cancellationToken);ownerScores[key]=1;return endpoint.Reference;
        }
        var terminal=trace[^1];ulong trailing=Endpoint(terminal.Node,terminal.Arc,false);
        if(word.GetSymbolSpan(cancellationToken)!=0)throw new InvalidDataException("Fresh trailing endpoint unexpectedly contains text.");
        //Actual1f42d0 immediately resets276 after that span0 check.
        word=LegacyNativeWordBuilder.CreateEmpty(result.GlyphPool,new byte[120],cancellationToken);
        ulong previous=ulong.MaxValue;var categories=new LegacyCursiveCategoryMapper(profile.Categories);
        for(int i=trace.Count-1;i>=0;i--)
        {
            cancellationToken.ThrowIfCancellationRequested();var cell=trace[i];var arcs=graph.GetNode(cell.Node);
            if(cell.Arc>=arcs.Count||arcs[cell.Arc].Span!=cell.Span||i>0&&cell.WordBoundaryBit)
                throw new NotSupportedException("Only the actual single-word path is materialized; split-word/interior delimiter policy is separate.");
            if(literalSymbols is null&&(
                cell.Symbol>=confidence.NetworkClassCount||!binding.Alphabet.TryGetText(cell.Symbol,out string text)||text.Length!=1||
                !(char.IsAsciiLetterOrDigit(text[0])||text[0] is >= '\u0400' and <= '\u04ff')))
                throw new NotSupportedException("Only the actual single plain Latin/Cyrillic/digit word path is materialized.");
            var primary=primary98.GetRowCopy(cell.Node);if(cell.Row>=primary.Count)throw new InvalidDataException("B8 row escapes its actual primary98 owner.");
            var literal=literalSymbols?.Create(cell.Symbol,primary.Get(cell.Row).CompositeCategory,cancellationToken);
            ushort symbol=literal?.PublishedLabel??cell.Symbol;
            int composite=literal?.CompositeCategory??categories.GetSymbolCompositeCategory(symbol);
            if(composite<0&&literalSymbols is null)throw new NotSupportedException("Plain symbol lacks native category mapping.");
            int geometryIndex=literal?.GeometryIndex??profile.Categories.CompositeCategories.Span[composite].GeometryIndex;
            ulong key=((ulong)cell.Arc<<24)|(uint)cell.Node;
            var added=word.AppendGlyph(key,checked(LegacyFirstPlainWordDelimiters.FreshDescriptorIdBase+grid.ColumnCountA4*cell.Row+cell.Column),false,new byte[40],cancellationToken);
            if(previous!=ulong.MaxValue)word.LinkGlyph(previous,added.Reference,cancellationToken);
            var cache=ScoreCache(arcs[cell.Arc].GroupId);
            if(literal is {} plan)literalSymbols!.Apply(result.GlyphPool,added.Reference,plan,primary.Get(cell.Row).CompositeCategory,cancellationToken);
            else _=LegacyNativeGlyphSymbolUpdate.Apply(result.GlyphPool,added.Reference,symbol,1,0,false,cancellationToken);
            byte[] glyph=result.GlyphPool.CopyRecord(added.Reference,cancellationToken);ushort mismatch=primary.Get(cell.Row).CompositeCategory==composite?(ushort)0:(ushort)8;
            if(literalSymbols is null){W16(glyph,4,mismatch);W16(glyph,6,0x40);}
            WF(glyph,12,primary.GetLikelihoodByte(cell.Row)/255f);WF(glyph,16,cache.ReadSymbolScore(symbol,0,cancellationToken));
            byte[] request=new byte[16];W16(request,6,ushort.MaxValue);W16(request,8,symbol);request[10]=checked((byte)geometryIndex);request[11]=1;
            BinaryPrimitives.WriteInt32LittleEndian(glyph.AsSpan(20),cache.ReadRequestCost(request,cancellationToken));
            result.GlyphPool.ReplaceRecord(added.Reference,glyph,cancellationToken);ownerScores[key]=cache.ReadFirstCandidateScore(cancellationToken);
            references.Add(added.Reference);previous=added.Reference;
        }
        var scalars=LegacyFirstPlainWordScalars.ReadUnadjusted(choice.TotalCost,choice.AcousticCost,grid.NodeCount,binding.SourceKind);
        word.SetRawScalars(scalars.LogMass10,scalars.Calibration14,scalars.NormalizedUnadjustedCost18,scalars.NormalizedAcousticCost1C,cancellationToken);
        int nativeParam7=Last(terminal.Node,terminal.Arc);
        var published=result.WordPublisher.Publish(word,First,Last,nativeParam7,-1,cancellationToken);
        //The root publication is followed by another reset and a zero-span
        //leading endpoint; it remains in the pool and does not form a word.
        word=LegacyNativeWordBuilder.CreateEmpty(result.GlyphPool,new byte[120],cancellationToken);
        var root=trace[0];ulong leading=Endpoint(root.Node,root.Arc,true);
        if(word.GetSymbolSpan(cancellationToken)!=0)throw new InvalidDataException("Fresh leading endpoint unexpectedly contains text.");
        float minimum=1;foreach(ulong reference in published.Word.FlatReferences)
        {cancellationToken.ThrowIfCancellationRequested();float value=ownerScores[LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference)];if(value<minimum)minimum=value;}
        references.Reverse();return new(result,cuts,graph,published,references.AsReadOnly(),trailing,leading,
            new ReadOnlyDictionary<ulong,float>(ownerScores),minimum,choice);
    }
    private static int I32(ReadOnlySpan<byte> b,int p)=>BinaryPrimitives.ReadInt32LittleEndian(b[p..]);
    private static ushort U16(ReadOnlySpan<byte> b,int p)=>BinaryPrimitives.ReadUInt16LittleEndian(b[p..]);
    private static void W16(byte[]b,int p,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(p),value);
    private static void WF(byte[]b,int p,float value)=>BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(p),value);
}
