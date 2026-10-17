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
        int First(int node,ushort arc)=>unchecked((int)graph.GetNode(node)[arc].StartContextId);
        int Last(int node,ushort arc)=>unchecked((int)graph.GetNode(node)[arc].EndContextId);
        var result=factory.CreatePlainResult(token);var word=Empty();var scores1C=new Dictionary<ulong,float>();
        var allocated=new List<ulong>();var detached=new List<ulong>();var words=new Dictionary<LegacyNativePublishedWordNode,LegacyFirstPublishedTextWord>();
        LegacyNativeWordBuilder Empty()=>LegacyNativeWordBuilder.CreateEmpty(result.GlyphPool,new byte[120],token);
        ulong AppendDelimiter(ulong key,bool force,LegacyFirstInteriorDelimiterResult symbol)
        {
            var row=word.AppendGlyph(key,-2,force,new byte[40],token);
            if(!LegacyNativeGlyphSymbolUpdate.Apply(result.GlyphPool,row.Reference,symbol.Symbol,symbol.Count,symbol.ComponentFlag,symbol.ExternalVariant!=0,token))
                throw new InvalidDataException("Actual bounded delimiter symbol update failed.");
            byte[] record=result.GlyphPool.CopyRecord(row.Reference,token);W16(record,6,(ushort)(U(record,6)|0x40));WF(record,12,1);WF(record,16,1);
            W16(record,4,(ushort)(U(record,4)&0xfff7));result.GlyphPool.ReplaceRecord(row.Reference,record,token);
            scores1C[LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(row.Reference)]=1;return row.Reference;
        }
        void Publish(bool delimiter,int total,int acoustic,int nodes,int lastOverride)
        {
            if(word.GetSymbolSpan(token)==0)return;
            var node=result.WordPublisher.Publish(word,First,Last,-1,lastOverride,token);
            words.Add(node,new(node,delimiter,total,acoustic,nodes));
        }
        var terminal=trace[^1];int tf=First(terminal.Node,terminal.Arc),tl=Last(terminal.Node,terminal.Arc);
        var trailing=endpoints.ReadEndpoint(terminal.Node,terminal.Arc,graph.NodeCount,ushort.MaxValue,256,token);
        detached.Add(AppendDelimiter(((ulong)(ushort)(tf-tl)<<24)|((uint)tl&0xffffff),true,
            new(ushort.MaxValue,trailing.EmittedSymbolCount,trailing.ComponentFlag,trailing.ExternalVariant)));
        if(word.GetSymbolSpan(token)!=0)throw new InvalidDataException("Fresh terminal delimiter is not zero-span.");word=Empty();
        int endCost=choice.TotalCost,endAcoustic=choice.AcousticCost,endNode=grid.NodeCount-1,wordEnd=-1;ulong previous=ulong.MaxValue;
        for(int i=trace.Count-1;i>=0;i--)
        {
            token.ThrowIfCancellationRequested();var cell=trace[i];var arcs=graph.GetNode(cell.Node);
            if(cell.Arc>=arcs.Count||arcs[cell.Arc].Span!=cell.Span)throw new InvalidDataException("Selected first arc escaped its graph.");
            if(wordEnd==-1)wordEnd=Last(cell.Node,cell.Arc);
            //The native guard encloses allocation AND publication. Hidden cells
            //still advance the B8 source walk; they cannot create word boundaries.
            if(!allocation.ShouldAllocate(cell.Symbol,token))continue;
            var source=primary.GetRowCopy(cell.Node);if(cell.Row>=source.Count)throw new InvalidDataException("Selected primary row is unavailable.");
            byte composite=source.Get(cell.Row).CompositeCategory;var plan=literal.Create(cell.Symbol,composite,token);
            ulong key=((ulong)cell.Arc<<24)|(uint)cell.Node;
            var insertion=word.AppendGlyph(key,checked(LegacyFirstPlainWordDelimiters.FreshDescriptorIdBase+grid.ColumnCountA4*cell.Row+cell.Column),false,new byte[40],token);
            if(previous!=ulong.MaxValue)word.LinkGlyph(previous,insertion.Reference,token);previous=insertion.Reference;
            literal.Apply(result.GlyphPool,insertion.Reference,plan,composite,token);var cache=Cache(arcs[cell.Arc].GroupId);
            byte[] glyph=result.GlyphPool.CopyRecord(insertion.Reference,token);WF(glyph,12,source.GetLikelihoodByte(cell.Row)/255f);WF(glyph,16,cache.ReadSymbolScore(plan.PublishedLabel,0,token));
            byte[] request=new byte[16];W16(request,6,ushort.MaxValue);W16(request,8,plan.PublishedLabel);request[10]=plan.GeometryIndex;request[11]=1;
            BinaryPrimitives.WriteInt32LittleEndian(glyph.AsSpan(20),cache.ReadRequestCost(request,token));result.GlyphPool.ReplaceRecord(insertion.Reference,glyph,token);
            scores1C[key]=cache.ReadFirstCandidateScore(token);allocated.Add(insertion.Reference);
            if(i>0&&!cell.WordBoundaryBit)continue;
            int priorNode=i==0?-1:trace[i-1].Node;
            int priorCost=i==0?0:I(trace[i-1].CopyRecord().Span,0),priorAcoustic=i==0?0:I(trace[i-1].CopyRecord().Span,4);
            int delta=unchecked(endCost-priorCost),acousticDelta=unchecked(endAcoustic-priorAcoustic),span=endNode-priorNode;
            var scalars=LegacyFirstPlainWordScalars.ReadUnadjusted(delta,acousticDelta,span,binding.SourceKind);
            word.SetRawScalars(scalars.LogMass10,scalars.Calibration14,scalars.NormalizedUnadjustedCost18,scalars.NormalizedAcousticCost1C,token);
            Publish(false,delta,acousticDelta,span,wordEnd);word=Empty();previous=ulong.MaxValue;
            if(i==0)
            {
                int first=First(cell.Node,cell.Arc),last=Last(cell.Node,cell.Arc);
                var leading=endpoints.ReadEndpoint(-1,ushort.MaxValue,cell.Node,cell.Arc,256,token);
                detached.Add(AppendDelimiter((1UL<<56)|((ulong)(ushort)(first-last)<<24)|((uint)last&0xffffff),false,
                    new(ushort.MaxValue,leading.EmittedSymbolCount,leading.ComponentFlag,leading.ExternalVariant)));
                if(word.GetSymbolSpan(token)!=0)throw new InvalidDataException("Fresh leading delimiter is not zero-span.");
            }
            else
            {
                var prior=trace[i-1];var separator=interior.Read(prior.Node,prior.Arc,prior.Symbol,cell.Node,cell.Arc,cell.Symbol,token);
                ulong delimiterKey=((ulong)(ushort)(Last(prior.Node,prior.Arc)-First(cell.Node,cell.Arc))<<24)|((uint)First(cell.Node,cell.Arc)&0xffffff);
                ulong reference=AppendDelimiter(delimiterKey,true,separator);
                if(word.GetSymbolSpan(token)==0)detached.Add(reference);else Publish(true,0,0,0,-1);
                word=Empty();endCost=priorCost;endAcoustic=priorAcoustic;endNode=priorNode;wordEnd=-1;
            }
        }
        var ordered=new List<LegacyFirstPublishedTextWord>();float minimum=1;
        for(var node=result.WordPublisher.Head;node is not null;node=node.Next)
        {
            token.ThrowIfCancellationRequested();ordered.Add(words[node]);
            foreach(ulong reference in node.Word.FlatReferences){float value=scores1C[LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference)];if(value<minimum)minimum=value;}
        }
        allocated.Reverse();token.ThrowIfCancellationRequested();return new(owner,result,choice,ordered.ToArray(),allocated.ToArray(),detached.ToArray(),scores1C,minimum);
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static ushort U(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
    private static void W16(byte[]b,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(at),value);
    private static void WF(byte[]b,int at,float value)=>BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(at),value);
}
