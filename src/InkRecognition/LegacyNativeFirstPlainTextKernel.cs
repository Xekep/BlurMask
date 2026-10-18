using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglish119PlainText.Probe")]
namespace InkRecognition;

internal delegate int LegacyFirstPlainGlyphRequestCost(ReadOnlySpan<byte> request,CancellationToken token);
internal sealed record LegacyFirstPlainGlyphCache(
    Func<ushort,int,CancellationToken,float> ReadSymbolScore,
    LegacyFirstPlainGlyphRequestCost ReadRequestCost,
    Func<CancellationToken,float> ReadFirstCandidateScore);
internal sealed record LegacyFirstPlainTextOperations(
    Func<ushort,CancellationToken,bool> ShouldAllocate,
    Func<ushort,byte,CancellationToken,LegacyFirstLiteralGlyphSymbolPlan> CreatePlan,
    Action<LegacyNativeGlyphRecordPool,ulong,LegacyFirstLiteralGlyphSymbolPlan,byte,CancellationToken> ApplySymbol,
    Func<int,LegacyFirstPlainGlyphCache> Cache,
    Func<int,ushort,ushort,int,ushort,ushort,CancellationToken,LegacyFirstInteriorDelimiterResult> ReadInterior);
internal sealed record LegacyFirstPlainTextStorage(LegacyNativeGlyphRecordPool GlyphPool,LegacyNativePlainWordPublisher WordPublisher);
internal sealed record LegacyNativeFirstPlainTextKernelResult(LegacyFirstPlainTextStorage Storage,
    LegacyFirstTerminalChoice Selection,LegacyFirstPublishedTextWord[] Words,ulong[] Glyphs,ulong[] Detached,
    Dictionary<ulong,float> OwnerFirstScores,float MinimumFirstScore);

/// <summary>Model-independent native1F42D0 plain WORD loop. Only private
/// source-bound adapters supply operations. Allocator residue is explicit.</summary>
internal static class LegacyNativeFirstPlainTextKernel
{
    internal static LegacyNativeFirstPlainTextKernelResult Run(LegacyFirstPlainTextStorage storage,
        LegacyOrdinaryCursiveCandidateGraph graph,LegacyFirstOrdinaryAcousticRecurrence primary,
        LegacyFirstTerminalChoice choice,IReadOnlyList<LegacyFirstSelectedGlyphCell> trace,int columnCount,int sourceKind,
        LegacyFirstPlainTextOperations operations,ReadOnlyMemory<byte> wordAllocatorStorage120,
        ReadOnlyMemory<byte> glyphAllocatorStorage40,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(trace.Count is <1 or >2047||columnCount!=1||wordAllocatorStorage120.Length!=120||glyphAllocatorStorage40.Length!=40)
            throw new NotSupportedException("Bounded native plain first path and exact allocation storage required.");
        ReadOnlyMemory<byte> wordStorage=wordAllocatorStorage120.ToArray(),glyphStorage=glyphAllocatorStorage40.ToArray();
        var endpoints=new LegacyFirstPlainWordDelimiters(graph);
        var grid=(NodeCount:graph.NodeCount,ColumnCountA4:columnCount);
        int First(int node,ushort arc)=>unchecked((int)graph.GetNode(node)[arc].StartContextId);
        int Last(int node,ushort arc)=>unchecked((int)graph.GetNode(node)[arc].EndContextId);
        var result=storage;var word=Empty();var scores1C=new Dictionary<ulong,float>();
        var allocated=new List<ulong>();var detached=new List<ulong>();var words=new Dictionary<LegacyNativePublishedWordNode,LegacyFirstPublishedTextWord>();
        LegacyNativeWordBuilder Empty()=>LegacyNativeWordBuilder.CreateEmpty(result.GlyphPool,wordStorage.Span,token);
        ulong AppendDelimiter(ulong key,bool force,LegacyFirstInteriorDelimiterResult symbol)
        {
            var row=word.AppendGlyph(key,-2,force,glyphStorage.Span,token);
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
            if(!operations.ShouldAllocate(cell.Symbol,token))continue;
            var source=primary.GetRowCopy(cell.Node);if(cell.Row>=source.Count)throw new InvalidDataException("Selected primary row is unavailable.");
            byte composite=source.Get(cell.Row).CompositeCategory;var plan=operations.CreatePlan(cell.Symbol,composite,token);
            ulong key=((ulong)cell.Arc<<24)|(uint)cell.Node;
            var insertion=word.AppendGlyph(key,checked(LegacyFirstPlainWordDelimiters.FreshDescriptorIdBase+grid.ColumnCountA4*cell.Row+cell.Column),false,glyphStorage.Span,token);
            if(previous!=ulong.MaxValue)word.LinkGlyph(previous,insertion.Reference,token);previous=insertion.Reference;
            operations.ApplySymbol(result.GlyphPool,insertion.Reference,plan,composite,token);var cache=operations.Cache(arcs[cell.Arc].GroupId);
            byte[] glyph=result.GlyphPool.CopyRecord(insertion.Reference,token);WF(glyph,12,source.GetLikelihoodByte(cell.Row)/255f);WF(glyph,16,cache.ReadSymbolScore(plan.PublishedLabel,0,token));
            byte[] request=new byte[16];W16(request,6,ushort.MaxValue);W16(request,8,plan.PublishedLabel);request[10]=plan.GeometryIndex;request[11]=1;
            BinaryPrimitives.WriteInt32LittleEndian(glyph.AsSpan(20),cache.ReadRequestCost(request,token));result.GlyphPool.ReplaceRecord(insertion.Reference,glyph,token);
            scores1C[key]=cache.ReadFirstCandidateScore(token);allocated.Add(insertion.Reference);
            if(i>0&&!cell.WordBoundaryBit)continue;
            int priorNode=i==0?-1:trace[i-1].Node;
            int priorCost=i==0?0:I(trace[i-1].CopyRecord().Span,0),priorAcoustic=i==0?0:I(trace[i-1].CopyRecord().Span,4);
            int delta=unchecked(endCost-priorCost),acousticDelta=unchecked(endAcoustic-priorAcoustic),span=endNode-priorNode;
            var scalars=LegacyFirstPlainWordScalars.ReadUnadjusted(delta,acousticDelta,span,sourceKind);
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
                var prior=trace[i-1];var separator=operations.ReadInterior(prior.Node,prior.Arc,prior.Symbol,cell.Node,cell.Arc,cell.Symbol,token);
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
        allocated.Reverse();token.ThrowIfCancellationRequested();return new(result,choice,ordered.ToArray(),allocated.ToArray(),detached.ToArray(),scores1C,minimum);
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static ushort U(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
    private static void W16(byte[]b,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(at),value);
    private static void WF(byte[]b,int at,float value)=>BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(at),value);
}
