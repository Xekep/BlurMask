using System.Buffers.Binary;
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishCharacters.Probe")]
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishCharacters.ActualEntry")]
namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal sealed record GraphCharacterData(ulong Reference,int SourceNode,bool Inserted,bool Ordinary,ulong[] SourcePairs,
        long SelectionId,int LineId,uint Flags,byte Prior,int PriorVisits,int Marker,byte[][] Records40,byte[][] RecordKnown40,byte[][] Utf8);
    [Flags] internal enum GraphCharacterWritten{None=0,SelectionId=1,BasicScalars=2,EmptyPriorVector=4,PriorRead=8,DefaultLine=16,AssignedLine=32,ComputedFlags=64}
    internal sealed record GraphCharacterPayloadData(int SourceNode,ulong[] SourcePairs,long SelectionId,int LineId,uint Flags,
        byte Prior,int PriorVisits,GraphCharacterAssignedScalars AssignedDefaults,GraphCharacterWritten Written,byte[][] Records40,byte[][] RecordKnown40,byte[][] Utf8);
    internal sealed record GraphCharacterCommandData(long SelectionId,string Name,int CommandIdentity,int PreviousCommandIdentity,ulong[] SourcePairs);
    internal sealed record GraphCharacterAssignedScalars(int Field08,long Field40,int Field00,int Field1C,byte Field64,int Field48);
    internal sealed record GraphCharactersResult(GraphGlyphGeometryResult Geometry,GraphCharacterData[] PerReferencePrefixes,GraphCharacterPayloadData[] FinalNodes,
        int CharacterTreeRoot,LegacyMainTextSourceTreeSnapshot[] CharacterTree,GraphCharacterCommandData[] PendingCommands);
    private bool graphCharactersRequested;
    private GraphGlyphRefreshDispatch? graphGlyphDispatch;
    private LegacyEnglish119MainLocalCandidates.GraphWordRangeReceipt? graphCharacterSourceRanges;
    private GraphCharacterContext? graphCharacterContext;
    private GraphCharacterData[]? graphCharacters;
    private GraphCharacterPayloadData[]? graphCharacterNodes;
    private LegacyMainTextSourceTreeSnapshot[]? graphCharacterTree;
    private GraphCharacterCommandData[]? graphCharacterCommands;
    private int graphCharacterTreeRoot;
    internal GraphCharactersResult RunGraphFirstCharacters(LegacyNativeUnicodeNormalizationData normalization,
        CancellationToken token=default,Action<GraphMainConfidenceObservation>? observer=null,bool captureSourceInputs=false)
    {
        ArgumentNullException.ThrowIfNull(normalization);primary.RequireGraphLoop(this,token);
        if(graphCharactersRequested)throw new InvalidOperationException("One same-owner CHAR publication required.");
        graphCharactersRequested=true;
        try
        {
            var geometry=RunGraphFirstGlyphGeometry(normalization,token,observer,captureSourceInputs);
            var data=graphCharacters??throw new InvalidOperationException("No complete private CHAR publication.");
            var nodes=graphCharacterNodes??throw new InvalidOperationException("No complete private CHAR-node snapshot.");
            return new(geometry,data.Select(x=>x with{SourcePairs=x.SourcePairs.ToArray(),Records40=x.Records40.Select(b=>b.ToArray()).ToArray(),RecordKnown40=x.RecordKnown40.Select(b=>b.ToArray()).ToArray(),Utf8=x.Utf8.Select(b=>b.ToArray()).ToArray()}).ToArray(),
                nodes.Select(x=>x with{SourcePairs=x.SourcePairs.ToArray(),Records40=x.Records40.Select(b=>b.ToArray()).ToArray(),RecordKnown40=x.RecordKnown40.Select(b=>b.ToArray()).ToArray(),Utf8=x.Utf8.Select(b=>b.ToArray()).ToArray()}).ToArray(),
                graphCharacterTreeRoot,(graphCharacterTree??throw new InvalidOperationException("No completed CHAR tree.")).ToArray(),
                (graphCharacterCommands??throw new InvalidOperationException("No completed CHAR registration prefix.")).Select(x=>x with{SourcePairs=x.SourcePairs.ToArray()}).ToArray());
        }
        finally{graphCharacterContext?.Dispose();}
    }
    private void CompleteGraphCharacterPass(GraphLanguageScope stage,LegacyNativeGlyphRecordPool pool,IReadOnlyList<ulong> refs,
        List<(ulong Key,byte[] Source,uint Bits,int Visited)> progress,CancellationToken token)
    {
        RequireGraphPublication(stage,token);
        if(!GraphCharacterEntryAllowed(graphCharactersRequested,graphCharacterContext is not null,graphWordPublicationContext is not null,refs.Count==progress.Count,graphGlyphGeometryContext is not null))
            throw new InvalidOperationException("Exact completed private first-pass producer required.");
        long ownerRows=0;
        foreach(var reference in refs){token.ThrowIfCancellationRequested();ownerRows=checked(ownerRows+pool.ReadGraphRefreshOwnerCount(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference),token));}
        if(refs.Count is<1 or>128||ownerRows>65536)throw new NotSupportedException("Bounded complete owning CHAR row domain required.");
        // Same whole-row allowance as UTF8 path, before selection/emission/export.
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(65536+ownerRows*2240+refs.Count*4096L));
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(131072+ownerRows*4256+refs.Count*8192L));
        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(16384+ownerRows*8192));
        // Reused CHAR rows can grow while earlier per-reference prefixes stay
        // retained: reserve R*sum(ownerRows), not merely successful new rows.
        long prefixRows=checked((long)refs.Count*ownerRows);
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(65536+prefixRows*1024));
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(131072+prefixRows*2048));
        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(16384+prefixRows*512));
        var context=graphGlyphGeometryContext!.CreateCharacterContext(stage,pool,token);graphCharacterContext=context;
        context.Require(token);
        var choices=new GraphGlyphChoiceData[refs.Count];var utf8=new GraphGlyphUtf8Data[refs.Count];var chars=new GraphCharacterData[refs.Count];
        for(int i=0;i<refs.Count;i++)
        {
            RequireGraphPublication(stage,token);ulong reference=refs[i],key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
            if(progress[i].Key!=key)throw new InvalidDataException("Retained actual reference chronology changed.");
            var source=context.Begin(i,reference,token);var selected=source.Selection;
            byte[] selectedRows=selected.CopySelectionRows4();
            var result=context.Publish(source,token);var rows=result.Payload.Reuse.CopyEnglishRecords(result.Payload,token);
            if((uint)result.Marker>=(uint)rows.Count)throw new NotSupportedException("Actual CHAR selected marker unavailable.");
            choices[i]=new(reference,progress[i].Source.ToArray(),progress[i].Bits,progress[i].Visited,selected.CopyOwnerStorage40(),selectedRows,selected.SelectedMarker);
            utf8[i]=new(reference,rows.Select(x=>x.Flags).ToArray(),rows.Select(x=>x.Text!.ReadBytes()).ToArray(),selectedRows.ToArray(),result.Marker);
            chars[i]=new(reference,result.Payload.Node.Identity,result.Inserted,source.Ordinary,Pairs(source.CopyRanges(token)),result.Payload.SelectionId,
                result.Payload.LineId,result.Payload.Flags,result.Payload.Prior,result.Payload.PriorVisits,result.Marker,
                rows.Select(x=>x.CopyStorage40()).ToArray(),rows.Select(x=>KnownCharacterRecord()).ToArray(),rows.Select(x=>x.Text!.ReadBytes()).ToArray());
            graphWordPublicationContext?.AppendCharacter(source,result.Payload,result.Marker,token);
            context.End(source,token);
        }
        context.Complete(refs.Count,token);RequireGraphPublication(stage,token);
        if(graphWordPublicationContext is not null)context.ReserveWordChoiceSnapshots(graphWordPublicationContext,token);
        graphGlyphChoices=choices;graphGlyphUtf8=utf8;graphCharacters=chars;graphCharacterNodes=context.CopyFinalPayloads(token);
        context.RequireCompleted(token);graphCharacterTreeRoot=context.Interner.RootIdentity;graphCharacterTree=context.Interner.CopyTree();
        graphCharacterCommands=context.Selections.CopyPendingSelections().Select(x=>new GraphCharacterCommandData(x.SelectionId,x.Name,x.CommandIdentity,x.PreviousCommandIdentity,Pairs(x.CopyRanges()))).ToArray();
        context.RequireCompleted(token);
    }
    // Model-neutral scalar024D40 formula only; it cannot issue CHAR authority.
    internal static uint CharacterFlags(bool ordinary,byte prior,uint aggregate,ReadOnlySpan<LegacyNativeInkPositionRange> key)
    {
        uint degenerate=ordinary&&key.Length==1&&SameCoordinate(key[0])?0x800000u:0;
        return(((uint)(prior&1)<<24)^((ordinary?0u:0x200000u)|(aggregate&0xce1fffffu)|degenerate))&0xfdffffffu;
        static bool SameCoordinate(LegacyNativeInkPositionRange r)=>r.First.SourceIndex==r.Last.SourceIndex&&r.First.PointIndex==r.Last.PointIndex&&(r.First.Fraction==r.Last.Fraction||r.First.Fraction==255||r.Last.Fraction==255);
    }
    private static byte[] KnownCharacterRecord()
    {
        var known=new byte[40];known.AsSpan(8,4).Fill(255);known.AsSpan(32,4).Fill(255);return known;
    }
    // Logical range slots/copy traffic, not CLR/RSS sizes. A source operation
    // can produce duplicate intervals: no endpoint uniqueness is assumed.
    internal readonly record struct GraphCharacterRangeReservation(long Current,long References,long Slots,long Moves,long Controls,long KeyBound,int Probes)
    {
        internal static GraphCharacterRangeReservation Start(int current,int references)
        {
            if(current is<0 or>65536||references is<1 or>128)throw new ArgumentOutOfRangeException();
            // Contains(original[1]) can clone one range; singleton joiner can
            // allocate one range even when no later probe executes.
            return new(current,references,16,16,1024,Math.Max(1,current),0);
        }
        internal GraphCharacterRangeReservation AddProbe(int later)
        {
            if(later is<0 or>65536)throw new ArgumentOutOfRangeException(nameof(later));
            checked
            {
                long a=Current,b=later,product=a*b,t=Math.Max(a,b);
                // Existing Bound(65536) checks remain untouched: Intersect may
                // append one filter (<=b rows) before checking; Subtract may
                // insert one row before its per-cut check. Returned arrays
                // always have <=65536 rows. This also keeps long arithmetic safe.
                long intersection=Math.Min(product,65536+b),cuts=Math.Min(product,65536),peak=Math.Min(t+cuts,65537);
                // Union <=b. Intersect <=a*b (even overlapping/repeated filters).
                // ONE cut inserts <=1; one possible Subtract peaks <=t+p.
                // Charge that Subtract for EVERY possible probe, although the
                // actual Process breaks after the first nonempty intersection.
                long slots=7*b+5*intersection+2*t+5*peak+24;
                long moves=7*b+6*intersection+2*t+5*peak+3*b*b+cuts*peak+24;
                long controls=256*(1+b*(b+1)+a*(b+1)+cuts*(peak+1));
                return this with{Slots=Slots+slots,Moves=Moves+moves,Controls=Controls+controls,
                    KeyBound=Math.Max(KeyBound,Math.Min(a+cuts,65536)),Probes=Probes+1};
            }
        }
        internal (long Payload,long Copies,long Visits) Finish()
        {
            checked
            {
                // Thirty-two complete key-equivalent buffers per reference:
                // interner/current/retained key, CHAR command, flags temporary,
                // per-reference/final/pending/returned pair exports and clones.
                // Every call reserves independently, including interner hits.
                long keys=32*KeyBound;
                long payload=16384+Probes*4096L+16*(Slots+keys);
                long copies=32768+Probes*8192L+32*(Moves+keys);
                // <=R tree comparisons/search + insertion/rebalance; conservatively
                // 8R comparisons, all key endpoints, plus source control leaves.
                long visits=checked(copies+Controls+4096+512*References*(KeyBound+1));
                return(payload,copies,visits);
            }
        }
    }
    internal sealed class GraphCharacterRange
    {
        internal readonly ulong Reference;
        internal readonly byte[] Original;
        internal readonly bool Joiner;
        internal readonly LegacyNativeInkPositionRange[] Projected,Clipped;
        internal GraphCharacterRange(ulong reference,byte[] original,bool joiner,LegacyNativeInkPositionRange[] projected,LegacyNativeInkPositionRange[] clipped)
        {Reference=reference;Original=original;Joiner=joiner;Projected=projected;Clipped=clipped;}
    }
    private sealed partial class GraphGlyphGeometryContext
    {
        private readonly List<GraphCharacterRange> characterRanges=[];
        private void RetainCharacterRanges(ulong reference,byte[] original,bool joiner,LegacyNativeInkPositionRange[] projected,LegacyNativeInkPositionRange[] clipped,CancellationToken token)
        {
            Require(token);
            if(!owner.graphCharactersRequested||characterRanges.Count>=128)throw new InvalidOperationException("Same requested live CHAR first pass required.");
            stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,256);
            characterRanges.Add(new(reference,original,joiner,projected,clipped));
        }
        internal GraphCharacterContext CreateCharacterContext(GraphLanguageScope actual,LegacyNativeGlyphRecordPool actualPool,CancellationToken token)
        {
            RequireCharacterFirstPass(actual,actualPool,token);
            if(owner.graphCharacterContext is not null)
            {
                var parent=owner.graphWordPublicationContext??throw new InvalidOperationException("One same-cohort postpass required.");
                parent.RequireChoiceGeometry(this,document,actual,actualPool,token);
                owner.graphCharacterContext.BeginWordChoice(parent,this,characterRanges,token);
                return owner.graphCharacterContext;
            }
            long count=0;foreach(var r in characterRanges)count=checked(count+r.Clipped.Length);
            if(count>4096)throw new NotSupportedException("Bounded total first-pass source ranges required.");
            // Initial private clones only. Later source operations reserve from
            // their CURRENT private lengths before each unchanged Process call.
            // No normalized/disjoint endpoint-growth premise is required.
            long rcount=characterRanges.Count,objects=document.ObjectCount;
            stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(131072+rcount*131072+count*64+objects*64));
            stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(262144+rcount*262144+count*128+objects*128));
            stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(65536+count*128+rcount*1024+objects*128));
            return new GraphCharacterContext(owner,stage,actualPool,this,document,operations,original,characterRanges,token);
        }
        private void RequireCharacterFirstPass(GraphLanguageScope actual,LegacyNativeGlyphRecordPool actualPool,CancellationToken token)
        {
            owner.RequireGraphPublication(stage,token);owner.graphWordSourceSession!.RequireCompletedForTop(publisher,pool,token);
            if(!ReferenceEquals(actual,stage)||!ReferenceEquals(actualPool,pool)||!ReferenceEquals(owner.graphGlyphGeometryContext,this)||
                !ReferenceEquals(owner.graphWordSourceData,source)||!finished||pendingOriginal is not null||characterRanges.Count!=glyphs.Count||characterRanges.Count==0)
                throw new InvalidOperationException("Same finished private range producer and live owner required.");
            publisher.RequireSoleActiveSourceHead(head);
        }
        internal void RequireCharacterContext(GraphCharacterContext actual,CancellationToken token)
        {
            RequireCharacterFirstPass(actual.Stage,actual.Pool,token);
            if(!ReferenceEquals(owner.graphCharacterContext,actual)||!ReferenceEquals(actual.Document,document))throw new InvalidOperationException("Exact retained private postpass context required.");
        }
        internal void RequireCharacterRange(GraphCharacterContext context,GraphCharacterRange actual,int index,CancellationToken token)
        {
            RequireCharacterContext(context,token);
            if((uint)index>=(uint)characterRanges.Count||!ReferenceEquals(characterRanges[index],actual))throw new InvalidOperationException("Actual first-pass range identity required.");
        }
    }
    internal sealed class GraphCharacterSource
    {
        internal readonly GraphCharacterContext Context;
        internal readonly int Index;
        internal readonly ulong Reference;
        internal readonly bool Ordinary;
        internal readonly GraphCharacterRange Range;
        internal readonly LegacyNativeGlyphRecordOwner Glyphs;
        internal readonly LegacyPreparedGlyphAlternativeSelection Selection;
        private readonly LegacyNativeInkPositionRange[] key;
        internal GraphCharacterSource(GraphCharacterContext context,int index,GraphCharacterRange range,bool ordinary,LegacyNativeInkPositionRange[] key,
            LegacyNativeGlyphRecordOwner glyphs,LegacyPreparedGlyphAlternativeSelection selection)
        {Context=context;Index=index;Reference=range.Reference;Range=range;Ordinary=ordinary;this.key=key;Glyphs=glyphs;Selection=selection;}
        internal int RangeCount=>key.Length;
        internal LegacyNativeInkPositionRange[] CopyRanges(CancellationToken token){Context.RequireCurrent(this,token);return key.ToArray();}
    }
    internal sealed partial class GraphCharacterContext:IDisposable
    {
        private readonly LegacyEnglishCaseNode0Completion owner;
        internal GraphLanguageScope Stage{get;}
        internal LegacyNativeGlyphRecordPool Pool{get;}
        internal LegacyOwnedRawInkDocument Document{get;}
        internal LegacyOwnedMainTextSourceInterner Interner{get;}
        internal LegacyOwnedFreshTextSourceContext Selections{get;}
        internal LegacyNativeUtf8StringPool Strings{get;}=new();
        internal LegacyNativeTextPublicationData Properties{get;}
        internal LegacyNativeUnicodeNormalizationData Normalization{get;}
        internal LegacyInkAlphabet Alphabet{get;}
        private readonly byte[] header;
        private object geometryIdentity;
        private readonly LegacyOwnedRawInkRangeOperations operations;
        private readonly LegacyNativeInkPositionRange[] original;
        private IReadOnlyList<GraphCharacterRange> produced;
        private LegacyNativeInkPositionRange[][] ranges;
        private bool[] skipped,positive,joiner;
        private readonly LegacyNativeSelectedInkRange[] inputs;
        private readonly Dictionary<int,GraphCharacterPayload> payloads=[];
        private GraphCharacterSource? current;
        private GraphCharacterPayload? initializing;
        private int next;
        private bool completed,disposed;
        internal GraphCharacterContext(LegacyEnglishCaseNode0Completion owner,GraphLanguageScope stage,LegacyNativeGlyphRecordPool pool,object geometryIdentity,
            LegacyOwnedRawInkDocument document,LegacyOwnedRawInkRangeOperations operations,LegacyNativeInkPositionRange[] original,IReadOnlyList<GraphCharacterRange> produced,CancellationToken token)
        {
            this.owner=owner;Stage=stage;Pool=pool;this.geometryIdentity=geometryIdentity;Document=document;this.operations=operations;this.original=original;this.produced=produced;
            Interner=LegacyOwnedMainTextSourceInterner.Prepare(document,token);Selections=owner.graphWordPublicationContext?.Selections??LegacyOwnedFreshTextSourceContext.Prepare(document,token);
            Properties=stage.Lease.Pass.Initializer.PlanForAudit.ReadGraphGlyphPublication(stage,token);Normalization=owner.graphGlyphNormalization!;Alphabet=stage.Lease.Root.Source.Alphabet;
            header=Alphabet.CopySerializedHeader();ranges=produced.Select(x=>x.Clipped.ToArray()).ToArray();skipped=new bool[produced.Count];positive=Enumerable.Repeat(true,produced.Count).ToArray();joiner=produced.Select(x=>x.Joiner).ToArray();inputs=document.CopySelectedInputs();
        }
        internal void Require(CancellationToken token)
        {
            owner.RequireGraphPublication(Stage,token);
            if(disposed||!owner.graphCharactersRequested||!ReferenceEquals(owner.graphCharacterContext,this)||!ReferenceEquals(owner.graphGlyphGeometryContext,geometryIdentity))
                throw new InvalidOperationException("Same live private CHAR cohort required.");
            owner.graphGlyphGeometryContext!.RequireCharacterContext(this,token);
        }
        internal GraphCharacterSource Begin(int index,ulong reference,CancellationToken token)
        {
            Require(token);if(completed||current is not null||index!=next||(uint)index>=(uint)produced.Count||produced[index].Reference!=reference)
                throw new InvalidOperationException("Next exact reference after complete all-reference first pass required.");
            owner.graphGlyphGeometryContext!.RequireCharacterRange(this,produced[index],index,token);
            ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
            if(owner.graphGlyphDispatch is null||!owner.graphGlyphDispatch.TryRead(Pool,key,token,out _))throw new InvalidOperationException("Same owner frontend readiness must survive into postpass.");
            // Selection and glyph snapshot are produced HERE from the identical
            // private pool. Prepared/caller snapshots are never issuer inputs.
            ReserveCurrentRanges(index,token);
            var node=Pool.CopyOwnerNode(key,token);
            var selection=LegacyPreparedGlyphAlternativeSelection.Select(node,unchecked((uint)(int)(short)(reference>>40)),
                new byte[checked((node.Records.Length/40*3/2+2)*4)],0,token);
            if(!selection.Succeeded)throw new NotSupportedException("Actual CHAR selection allocation refused.");
            var glyphs=new LegacyNativeGlyphRecordOwner(new[]{node});
            var result=LegacyPreparedMainTextSourceProcessing.Process(operations,original,ranges,skipped,positive,joiner,index,token);ranges[index]=result.Key;
            current=new(this,index,produced[index],result.Ordinary,result.Key,glyphs,selection);RequireCurrent(current,token);return current;
        }
        private void ReserveCurrentRanges(int index,CancellationToken token)
        {
            Require(token);
            var charge=GraphCharacterRangeReservation.Start(ranges[index].Length,produced.Count);
            // Joiner and negative/skipped paths do not probe later keys. The
            // outside-original early return is conservatively charged as probes.
            if(positive[index]&&!joiner[index])
                for(int later=index+1;later<ranges.Length;later++)
                {token.ThrowIfCancellationRequested();if(!skipped[later]&&positive[later])charge=charge.AddProbe(ranges[later].Length);}
            var totals=charge.Finish();
            Stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,totals.Payload);
            Stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,totals.Copies);
            Stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,totals.Visits);
            Require(token);
        }
        internal void RequireCurrent(GraphCharacterSource actual,CancellationToken token)
        {Require(token);if(!ReferenceEquals(current,actual)||!ReferenceEquals(actual.Context,this)||actual.Index!=next)throw new InvalidOperationException("Actual current processed source receipt required.");}
        internal (GraphCharacterPayload Payload,bool Inserted,int Marker) Publish(GraphCharacterSource source,CancellationToken token)
        {
            RequireCurrent(source,token);var intern=Interner.InternEnglish(source,token);GraphCharacterPayload payload;int marker;
            if(intern.Inserted)
            {
                var fresh=EmitFresh(source,token);payload=new(this,source,intern.Node,fresh,token);payloads.Add(intern.Node.Identity,payload);initializing=payload;
                payload.Initialize(token);initializing=null;marker=fresh.AdjustedMarkerIndex;
            }
            else
            {
                if(!payloads.TryGetValue(intern.Node.Identity,out payload!)||!payload.Complete||!ReferenceEquals(payload.Context,this))
                    throw new InvalidOperationException("Exact initialized same-context CHAR payload required.");
                var result=payload.Reuse.ApplyEnglish(payload,source,token);
                if(!result.Succeeded)throw new NotSupportedException("Native21760 failed prefix retained; owner is not retryable.");
                payload.Flags=payload.Reuse.AggregateFlags;marker=result.AdjustedMarkerIndex;
            }
            RequireCurrent(source,token);return(payload,intern.Inserted,marker);
        }
        private LegacyNativeTextGlyphCallerResult EmitFresh(GraphCharacterSource source,CancellationToken token)
        {
            RequireCurrent(source,token);byte[] selections=source.Selection.CopySelectionRows4();ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(source.Reference);
            for(int j=0;j<selections.Length;j+=4)
            {
                var glyph=source.Glyphs.Resolve(key|((ulong)BinaryPrimitives.ReadUInt16LittleEndian(selections.AsSpan(j))<<40));
                RequireOrdinary(glyph);
            }
            return LegacyNativeTextGlyphCaller.Produce(source.Glyphs,Alphabet,header,selections,key,false,unchecked((int)source.Selection.SelectedMarker!.Value),Strings,Properties,Normalization,cancellationToken:token);
        }
        internal byte[] EmitReuse(GraphCharacterSource source,LegacyNativeOwnedGlyphRecord glyph,CancellationToken token)
        {RequireCurrent(source,token);RequireOrdinary(glyph);return LegacyNativeOrdinaryGlyphUtf8.Emit(glyph.Symbol,Alphabet,cancellationToken:token);}
        private void RequireOrdinary(LegacyNativeOwnedGlyphRecord glyph)
        {
            var d=glyph.Symbol;ushort e=BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(6));
            if((d.KindFlags&7)!=0||d.SymbolOrRunLength>=Alphabet.LabelCount||d.SymbolOrRunLength==e||!Alphabet.TryGetText(d.SymbolOrRunLength,out string text)||System.Text.Encoding.UTF8.GetByteCount(text)>64)
                throw new NotSupportedException("Actual ordinary bounded nonHeaderE label required.");
        }
        internal void RequireInitializing(GraphCharacterPayload payload,CancellationToken token)
        {
            RequireCurrent(payload.Source,token);
            if(!ReferenceEquals(initializing,payload)||!ReferenceEquals(payload.Context,this)||!payloads.TryGetValue(payload.Node.Identity,out var actual)||!ReferenceEquals(actual,payload)||!ReferenceEquals(payload.Node.Owner,Interner))
                throw new InvalidOperationException("Actual newly interned payload initialization required.");
        }
        internal void RequireReusable(GraphCharacterPayload payload,GraphCharacterSource source,CancellationToken token)
        {
            RequireCurrent(source,token);
            if(!payload.Complete||!ReferenceEquals(payload.Context,this)||!payloads.TryGetValue(payload.Node.Identity,out var actual)||!ReferenceEquals(actual,payload))
                throw new InvalidOperationException("Actual complete same-context reused payload required.");
        }
        internal (byte Prior,int Visits) ReadPrior(GraphCharacterSource source,CancellationToken token)
        {
            RequireCurrent(source,token);ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(source.Reference);
            if(!owner.graphGlyphDispatch!.TryRead(Pool,key,token,out _))throw new InvalidOperationException("Actual successful frontend readiness required.");
            // Tiny preflight/candidate readonly wrappers and metadata, before
            // any allocation. Source DFS uses same-owner nonalloc node access.
            Stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,1024);
            Stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,2048);
            Stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,4096);
            var address=LegacyNativeResidualSource.Decode(key);var graph=Stage.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph;
            var arc=graph.GetNode(address.Node)[address.Arc];var actualRanges=owner.graphCharacterSourceRanges??throw new InvalidOperationException("Same retained actual V140 range issuer required.");
            var bound=actualRanges.Preflight(checked((int)arc.StartContextId),checked((int)arc.EndContextId),token);
            Stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(bound.Payload+512L*bound.Depth));
            Stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(bound.Copies+1024L*bound.Depth+64L*bound.Depth*bound.Depth));
            Stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(32768L+bound.Starts*(long)bound.Depth*bound.SourcePoints*16));
            //021D10's own actual source count/fill is distinct from217670's
            //readiness; never substitute a returned diagnostic Source26 array.
            var count=actualRanges.ReadOrdinary140(address.Node,address.Arc,token);var fill=actualRanges.ReadOrdinary140(address.Node,address.Arc,token);
            if(count.Length==0||!count.AsSpan().SequenceEqual(fill))throw new InvalidDataException("Same immutable prior count/fill changed.");
            RequireCurrent(source,token);int visits=0;byte prior=1;
            foreach(var r in fill)
            {
                if(r.First.First!=0||r.Last.First!=0||(uint)r.First.Second>=(uint)inputs.Length||(uint)r.Last.Second>=(uint)inputs.Length)throw new NotSupportedException("Actual group0 C4A bounds required.");
                for(int i=r.First.Second;i<=r.Last.Second;i++){token.ThrowIfCancellationRequested();if(++visits>1_048_576||inputs[i].IsKnownText)throw new NotSupportedException("Fresh original raw source only.");prior=0;}
            }
            return(prior,visits);
        }
        internal int ReadLine(GraphCharacterSource source,CancellationToken token)
        {
            RequireCurrent(source,token);var a=LegacyNativeResidualSource.Decode(source.Reference);
            return owner.graphLoopGeometry!.ReadGraphCharacterLine(Stage.Lease.Root.Knowledge.Owner,Stage.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph.RequireSameSource(),a.Node,a.Arc,token);
        }
        internal void End(GraphCharacterSource source,CancellationToken token){RequireCurrent(source,token);current=null;next++;}
        internal void Complete(int count,CancellationToken token){Require(token);if(current is not null||next!=count||next!=produced.Count)throw new InvalidOperationException("All actual CHAR references required.");completed=true;}
        internal void RequireCompleted(CancellationToken token){Require(token);if(!completed)throw new InvalidOperationException("Completed private CHAR postpass required.");}
        internal GraphCharacterPayloadData[] CopyFinalPayloads(CancellationToken token)
        {
            RequireCompleted(token);var output=new List<GraphCharacterPayloadData>(payloads.Count);
            foreach(var payload in payloads.OrderBy(x=>x.Key).Select(x=>x.Value))
            {
                token.ThrowIfCancellationRequested();var rows=payload.Reuse.CopyEnglishRecords(payload,token);
                output.Add(new(payload.Node.Identity,Pairs(payload.CopyRetainedRanges()),payload.SelectionId,payload.LineId,payload.Flags,payload.Prior,payload.PriorVisits,payload.AssignedDefaults!,payload.Written,
                    rows.Select(x=>x.CopyStorage40()).ToArray(),rows.Select(x=>KnownCharacterRecord()).ToArray(),rows.Select(x=>x.Text!.ReadBytes()).ToArray()));
            }
            RequireCompleted(token);return output.ToArray();
        }
        public void Dispose(){if(disposed)return;disposed=true;Strings.Dispose();}
    }
    internal sealed class GraphCharacterPayload
    {
        internal readonly GraphCharacterContext Context;
        internal readonly GraphCharacterSource Source;
        internal readonly LegacyOwnedMainTextSourceNode Node;
        internal readonly LegacyPreparedTextGlyphReuseRecord[] Initial;
        private readonly LegacyNativeInkPositionRange[] retainedRanges;
        internal int RetainedRangeCount=>retainedRanges.Length;
        internal LegacyNativeInkPositionRange[] CopyRetainedRanges()=>retainedRanges.ToArray();
        internal LegacyPreparedMainTextGlyphReuse Reuse{get;private set;}=null!;
        internal long SelectionId{get;private set;}=-1;
        internal int LineId{get;private set;}=-1;
        internal uint Flags{get;set;}
        internal byte Prior{get;private set;}
        internal int PriorVisits{get;private set;}
        internal bool Complete{get;private set;}
        internal GraphCharacterAssignedScalars? AssignedDefaults{get;private set;}
        internal GraphCharacterWritten Written{get;private set;}
        internal GraphCharacterPayload(GraphCharacterContext context,GraphCharacterSource source,LegacyOwnedMainTextSourceNode node,LegacyNativeTextGlyphCallerResult fresh,CancellationToken token)
        {
            context.RequireCurrent(source,token);Context=context;Source=source;Node=node;retainedRanges=source.CopyRanges(token);
            Initial=fresh.Records.Select(r=>{token.ThrowIfCancellationRequested();context.Strings.Validate(r.Text);var b=new byte[40];BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(8),r.Flags);BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(32),r.ByteCount);return new LegacyPreparedTextGlyphReuseRecord(b,r.Text);}).ToArray();
        }
        internal void Initialize(CancellationToken token)
        {
            Context.RequireInitializing(this,token);
            bool succeeded=true;
            if(Source.Ordinary)
            {
                var selection=Context.Selections.CreateEnglishCharacter(this,token);succeeded=selection is not null;
                if(succeeded){SelectionId=selection!.SelectionId;Written|=GraphCharacterWritten.SelectionId;}
            }
            else{SelectionId=-1;Written|=GraphCharacterWritten.SelectionId;}
            //024D40 still assigns defaults and prior-vector state after a normal
            //registration refusal; the library then poisons rather than retries.
            AssignedDefaults=new(-1,-1L,0,0,0,-1);Written|=GraphCharacterWritten.BasicScalars|GraphCharacterWritten.EmptyPriorVector;
            if(succeeded&&Source.Ordinary){var prior=Context.ReadPrior(Source,token);Prior=prior.Prior;PriorVisits=prior.Visits;Written|=GraphCharacterWritten.PriorRead;}
            LineId=-1;Written|=GraphCharacterWritten.DefaultLine;
            if(!succeeded)throw new NotSupportedException("Actual CHAR registration refused; prefix retained.");
            LineId=Context.ReadLine(Source,token);Written|=GraphCharacterWritten.AssignedLine;uint aggregate=0;foreach(var row in Initial)aggregate|=row.Flags;
            var key=Source.CopyRanges(token);Flags=CharacterFlags(Source.Ordinary,Prior,aggregate,key);
            Written|=GraphCharacterWritten.ComputedFlags;Reuse=LegacyPreparedMainTextGlyphReuse.CreateEnglish(this,token);Complete=true;
        }

    }
}
