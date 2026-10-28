using System.Buffers.Binary;
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishWordPublication.Probe")]
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal sealed record GraphWordCharacterLinkData(int SourceNode,int Marker,byte[] Scalar24);
    internal sealed record GraphWordChoiceData(int Ordinal,uint Flags,ulong[] OrdinaryPairs,byte[] Scalar48,
        GraphWordCharacterLinkData[] Links,GraphCharacterData[] Characters);
    internal sealed record GraphWordAssignedData(int Field08,ulong Field00,long Field40,int Field48);
    internal record GraphWordPublicationData(int SourceNode,int SelectedOrdinal,long SelectionId,uint Flags,
        GraphWordAssignedData Defaults,ulong[] OriginalPairs,GraphWordChoiceData[] Choices,
        GraphCharacterPayloadData[] FinalCharacters,GraphCharacterCommandData[] PendingCommands,string[] Trace);
    internal sealed record GraphWordPublicationResult(GraphCharactersResult LastChoiceCharacters,GraphWordPublicationData Word);
    private bool graphWordPublicationRequested;
    private GraphWordPublicationContext? graphWordPublicationContext;
    private GraphWordPublicationData? graphWordPublicationData;

    //All275 WORD publication only. TEXT/source lines remain a later typed join
    //INSIDE CompleteGraphWordPublication before RunGraphFirstCharacters finally.
    internal GraphWordPublicationResult RunGraphFirstWordPublication(LegacyNativeUnicodeNormalizationData normalization,
        CancellationToken token=default,Action<GraphMainConfidenceObservation>? observer=null,bool captureSourceInputs=false)
    {
        ArgumentNullException.ThrowIfNull(normalization);primary.RequireGraphLoop(this,token);
        if(graphWordPublicationRequested)throw new InvalidOperationException("One same-owner all275 WORD pass required.");
        graphWordPublicationRequested=true;
        var characters=RunGraphFirstCharacters(normalization,token,observer,captureSourceInputs);
        return new(characters,graphWordPublicationData??throw new InvalidOperationException("No completed owning WORD publication."));
    }
    private void CompleteGraphWordPublication(GraphLanguageScope stage,LegacyPreparedMainCompactPublication.Result compact,
        LegacyNativePlainWordPublisher publisher,LegacyNativeGlyphRecordPool pool,LegacyMainOrdinaryGlyphProductionOperations ordinary,CancellationToken token)
    {
        RequireGraphPublication(stage,token);
        if(graphWordPublicationContext is not null||!graphCharactersRequested||!graphGlyphGeometryRequested)
            throw new InvalidOperationException("Fresh requested same-owner CHAR/geometry path required.");
        var parent=GraphWordPublicationContext.Issue(this,stage,publisher,pool,token);graphWordPublicationContext=parent;
        try
        {
            parent.InternWord(token); //028E25/defaults precede every024F30 refresh.
            GraphCompactRefreshIndex? refreshIndex=null;
            if(graphPeriodicActive&&graphOriginalSelectedRequested&&graphTextPublicationRequested)
                refreshIndex=GraphCompactRefreshIndex.Issue(stage,compact,pool,parent,token);
            for(int ordinal=0;ordinal<parent.Count;ordinal++)
            {
                parent.Open(ordinal,token);
                graphGlyphGeometryContext=null; //private per-choice geometry, SAME original document.
                var output=graphPeriodicActive&&graphOriginalSelectedRequested&&graphTextPublicationRequested&&ordinal<parent.Count-1
                    ?GraphCharacterOutput.IntermediatePrefixesOnly:GraphCharacterOutput.FullCanonical;
                CompleteGraphGlyphChoiceRows(stage,compact,publisher,pool,ordinary,parent.CurrentReferences(token),token,output,refreshIndex);
                parent.Close(token);
            }
            graphWordPublicationData=parent.CompleteWord(token);
            if(graphTextPublicationRequested)graphTextPublicationData=parent.CompleteText(token);
            if(graphPostTextCoverageRequested)graphPostTextCoverageData=parent.CompletePostTextCoverage(token);
            if(graphSelectedSourcesRequested)graphSelectedSourcesData=parent.CompleteSelectedSources(token);
            if(graphSourceIndicesRequested)graphSourceIndicesData=parent.CompleteSourceIndices(token);
            if(graphOriginalSelectedRequested)graphEarlySelectedInputData=parent.CopyEarlySelectedInputData(token);
            if(graphPeriodicActive&&graphOriginalSelectedRequested&&graphTextPublicationRequested&&graphSelectedSourcesRequested)
                graphWordPublicationData=parent.CopyFinalSelectedData(graphWordPublicationData!,token);
            //Future021AB0/NULL projection/TEXT must be called HERE while the
            //same CHAR context/StringPool/Selections are live, never on result DATA.
        }
        catch{parent.Poison();throw;}
    }

    //Model-neutral029080 all48 flag fold; no source/owner capability.
    internal static uint FoldGraphWordFlags(uint prior,ReadOnlySpan<GraphWordChoiceData> choices)
    {foreach(var choice in choices)prior|=choice.Flags;return prior;}

    internal sealed partial class GraphWordPublicationContext
    {
        private readonly LegacyEnglishCaseNode0Completion owner;
        internal readonly GraphLanguageScope Stage;
        internal readonly LegacyNativeGlyphRecordPool Pool;
        internal readonly LegacyOwnedRawInkDocument Document;
        internal readonly LegacyOwnedFreshTextSourceContext Selections;
        private readonly LegacyNativePlainWordPublisher publisher;
        private readonly LegacyNativePublishedWordNode head;
        private readonly LegacyNativePlainWordSpanSnapshot span;
        private readonly LegacyOwnedMainTextSourceInterner words;
        private readonly LegacyOwnedRawInkRangeOperations operations;
        private readonly LegacyNativeInkPositionRange[] originalKey;
        private readonly List<string> trace=[];
        private readonly GraphWordChoiceData[] output;
        private readonly int thread=Environment.CurrentManagedThreadId;
        private LegacyOwnedMainTextSourceNode? wordNode;
        private List<GraphWordCharacterLinkData>? links;
        private LegacyNativeInkPositionRange[] ordinary=[];
        private uint flags=0x200000,choiceFlags;
        private int next,current=-1,firstPassNext;
        private long selectionId=long.MaxValue;
        private bool poisoned,finished;
        internal int Count=>span.Words.Count;
        internal int TotalReferences{get;}
        internal int SelectedOrdinal{get;}
        private GraphWordPublicationContext(LegacyEnglishCaseNode0Completion owner,GraphLanguageScope stage,
            LegacyNativePlainWordPublisher publisher,LegacyNativeGlyphRecordPool pool,CancellationToken token)
        {
            this.owner=owner;Stage=stage;this.publisher=publisher;Pool=pool;
            owner.RequireGraphPublication(stage,token);owner.graphWordSourceSession!.RequireCompletedForTop(publisher,pool,token);
            head=publisher.Head??throw new InvalidDataException("Actual selected WORD required.");publisher.RequireSoleActiveSourceHead(head);
            if(head.Next is not null)throw new NotSupportedException("One actual279 WORD span required.");
            //Fixed4096 row-pointer reservation BEFORE CaptureSpan allocates its
            //words/selected/ordinal arrays, lists and validation hashsets.
            if(owner.graphPeriodicActive&&owner.graphOriginalSelectedRequested&&owner.graphTextPublicationRequested)
            {
                Reserve(0,0,512); // BEFORE the allocation-free actual same-pool span scalar validation.
                var scalar=publisher.ReadFreshSingletonTopScalars(head,pool,token);
                var budget=LegacyEnglishPeriodicPublicationBudget.SpanCapture(scalar.Count);
                Reserve(budget.Payload,budget.Copies,budget.Visits);
                span=publisher.CaptureSpan(head,[head],token);
                if(span.Words.Count!=scalar.Count||span.SelectedOrdinals.Count!=1||span.SelectedOrdinals[0]!=scalar.Selected)
                    throw new InvalidOperationException("Same immutable span counts/selection changed during capture.");
            }
            else
            {
                Reserve(65536,131072,32768);
                span=publisher.CaptureSpan(head,[head],token);
            }
            if(span.Words.Count is<1 or>4095||span.SelectedNodes.Count!=1||!ReferenceEquals(span.SelectedNodes[0],head))
                throw new NotSupportedException("One live TOP and complete bounded275 span required.");
            SelectedOrdinal=span.SelectedOrdinals[0];Reserve(checked(4096+Count*256L),checked(8192+Count*512L),checked(4096+Count*256L));int references=0;
            foreach(var row in span.Words)
            {
                token.ThrowIfCancellationRequested();
                if(!ReferenceEquals(row.Pool,pool)||row.FlatReferences.Count is<1 or>128||row.FlatReferences.Count>4096-references)
                    throw new NotSupportedException("Bounded whole275 ordinary reference domain required before mutation.");
                foreach(ulong reference in row.FlatReferences)
                    if(unchecked((short)(reference>>24))<0||(reference&(1UL<<56))!=0)
                        throw new NotSupportedException("Negative/higher source needs its genuine geometry issuer.");
                references+=row.FlatReferences.Count;
                var b=row.CopyStorage();
                if(b.Length!=120||BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(88))!=0||BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(96))!=0||
                    BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(104))!=0||BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(112))!=0)
                    throw new NotSupportedException("All275 actual plain/no-auxiliary WORD layout required.");
            }
            TotalReferences=references;
            Reserve(checked(65536+Count*1024L+references*2048L),checked(131072+Count*2048L+references*4096L),checked(32768+references*512L));
            Document=stage.Lease.Root.Knowledge.Owner.Owner.Owner.Parent.CreateGraphRawDocument(stage,token);
            Selections=owner.graphOriginalSelectedRequested?PrepareEarlyOriginalSelected(token):LegacyOwnedFreshTextSourceContext.Prepare(Document,token);words=LegacyOwnedMainTextSourceInterner.Prepare(Document,token);
            operations=LegacyOwnedRawInkRangeOperations.Prepare(Document);
            if(owner.graphPostTextCoverageRequested)PreparePostTextState(token);
            if(owner.graphTextPublicationRequested)PrepareTextInputs(token); //native028771, before WORD source/intern.
            var source=owner.graphWordSourceData??throw new InvalidOperationException("Actual private source completion required.");
            var root=new[]{new LegacyNativeInkPositionRange(LegacyNativeInkPosition.Pack(0,0,3,Document.Strokes[0].Points.Count),
                LegacyNativeInkPosition.Pack((uint)Document.ObjectCount-1,Document.Strokes[^1].Points.Count-1,5,Document.Strokes[^1].Points.Count))};
            //Same producer source26 -> original WORD key. Neither a CHAR union
            //nor a result snapshot is imported as authority.
            LegacyEnglishCaseNode0Completion.Reserve(stage,source.References.Length,Document.ObjectCount,0,token);
            originalKey=ProjectCore(Document,new LegacyNativeTextSourceInputMap(Document.CopySelectedInputs(),[],0),root,source.References,token);
            output=new GraphWordChoiceData[Count];trace.Add("document-choice32");
        }
        internal static GraphWordPublicationContext Issue(LegacyEnglishCaseNode0Completion owner,GraphLanguageScope stage,
            LegacyNativePlainWordPublisher publisher,LegacyNativeGlyphRecordPool pool,CancellationToken token)
        {
            stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,27);
            stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,27);
            return new(owner,stage,publisher,pool,token);
        }
        private void Reserve(long payload,long copies,long visits)
        {Stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,payload);Stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,copies);Stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,visits);}
        internal void RequireCompactRefreshIndex(GraphLanguageScope actualStage,LegacyNativeGlyphRecordPool actualPool,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(poisoned||finished||thread!=Environment.CurrentManagedThreadId||!ReferenceEquals(Stage,actualStage)||
                !ReferenceEquals(Pool,actualPool)||!ReferenceEquals(owner.graphWordPublicationContext,this)||
                !owner.graphPeriodicActive||!owner.graphOriginalSelectedRequested||!owner.graphTextPublicationRequested)
                throw new InvalidOperationException("Same private rich WORD/compact refresh index required.");
        }
        internal LegacyNativeInkPositionRange[] CopyGeometryWordClip(GraphLanguageScope actualStage,
            LegacyNativeGlyphRecordPool actualPool,LegacyOwnedRawInkDocument actualDocument,GraphWordSourceData actualSource,CancellationToken token)
        {
            // BEFORE new metadata reads; no complete-span rewalk or source query.
            actualStage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,1024);
            RequireCompactRefreshIndex(actualStage,actualPool,token);
            if(!ReferenceEquals(Document,actualDocument)||!ReferenceEquals(owner.graphWordSourceData,actualSource)||
                current!=next||current<0||current>=Count||firstPassNext!=0)
                throw new InvalidOperationException("Same private original WORD projection before first geometry reference required.");
            // Retain original Issue projection-product admission, even though its
            // identical projection already completed in this private WORD owner.
            if((long)actualSource.References.Length*Document.ObjectCount>4096)
                throw new NotSupportedException("Geometry projection/intersection admission cap reached before allocation.");
            int references=span.Words[current].FlatReferences.Count;
            var budget=GraphGeometryReuseCharge(originalKey.Length,Document.ObjectCount,references);
            Stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,budget.Payload);
            Stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,budget.Copies);
            Stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,budget.Visits);
            return originalKey.ToArray(); // Never expose the private writable backing array.
        }
        internal void RequireIntermediateCharacterOutput(GraphLanguageScope actualStage,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(poisoned||thread!=Environment.CurrentManagedThreadId||!ReferenceEquals(Stage,actualStage)||
                !ReferenceEquals(owner.graphWordPublicationContext,this)||!owner.graphPeriodicActive||
                !owner.graphOriginalSelectedRequested||!owner.graphTextPublicationRequested||
                current!=next||current<0||current>=Count-1)
                throw new InvalidOperationException("Actual nonfinal rich WORD choice required for prefix-only CHAR DATA.");
        }
        internal void Require(CancellationToken token)
        {
            owner.RequireGraphPublication(Stage,token);owner.graphWordSourceSession!.RequireCompletedForTop(publisher,Pool,token);
            if(poisoned||thread!=Environment.CurrentManagedThreadId||!ReferenceEquals(owner.graphWordPublicationContext,this)||head.WordIndex!=SelectedOrdinal)
                throw new InvalidOperationException("Same live all275 owner/thread/selected ordinal required.");
            publisher.RequireSoleActiveSourceHead(head);publisher.RequireLiveSpanRows(span,token);
        }
        internal void InternWord(CancellationToken token)
        {
            Require(token);if(wordNode is not null||current!=-1||next!=0||words.Count!=0)throw new InvalidOperationException("Fresh WORD source required.");
            var actual=words.InternEnglishWord(this,token);if(!actual.Inserted)throw new InvalidOperationException("Fresh WORD tree must insert.");
            wordNode=actual.Node;
            if(owner.graphSourceIndicesRequested)InitializeWordHeader30(token);
            trace.Add("word-intern");trace.Add("word-defaults");
        }
        internal LegacyOwnedMainTextSourceInternResult InternInto(LegacyOwnedMainTextSourceInterner actual,CancellationToken token)
        {Require(token);if(!ReferenceEquals(actual,words)||wordNode is not null||current!=-1||next!=0)throw new InvalidOperationException("Actual new WORD intern point required.");return actual.InternPrepared(originalKey,token);}
        internal void Open(int ordinal,CancellationToken token)
        {
            Require(token);if(wordNode is null||finished||current!=-1||ordinal!=next||(uint)ordinal>=(uint)Count)throw new InvalidOperationException("Next exact275 choice required.");
            current=ordinal;firstPassNext=0;links=new(span.Words[current].FlatReferences.Count);ordinary=[];choiceFlags=0;trace.Add("word-choice48:"+current);
            if(owner.graphPostTextCoverageRequested)BeginPostTextChoice(token);
        }
        internal IReadOnlyList<ulong> CurrentReferences(CancellationToken token)
        {Require(token);if(current!=next||current<0)throw new InvalidOperationException("Current275 cursor required.");return span.Words[current].FlatReferences;}
        internal void RequireReferences(IReadOnlyList<ulong> actual,CancellationToken token)
        {if(!ReferenceEquals(CurrentReferences(token),actual))throw new InvalidOperationException("Exact private current275 reference backing required.");}
        internal void BeforeReference(ulong actual,CancellationToken token)
        {
            Require(token);var refs=CurrentReferences(token);if(current<0||firstPassNext>=refs.Count||refs[firstPassNext]!=actual)throw new InvalidOperationException("Next current ordinary reference required.");firstPassNext++;
            //All references admitted above are positive/eligible; native first
            //pass clears parent200000 BEFORE that reference's own refresh.
            flags&=~0x200000u;
        }
        internal void RequireChoiceGeometry(object actual,LegacyOwnedRawInkDocument document,GraphLanguageScope stage,LegacyNativeGlyphRecordPool pool,CancellationToken token)
        {Require(token);if(current<0||firstPassNext!=CurrentReferences(token).Count||!ReferenceEquals(owner.graphGlyphGeometryContext,actual)||!ReferenceEquals(Document,document)||!ReferenceEquals(Stage,stage)||!ReferenceEquals(Pool,pool))throw new InvalidOperationException("Same current WORD private geometry required.");}
        internal void AppendCharacter(GraphCharacterSource source,GraphCharacterPayload payload,int marker,byte[] currentSelectionRows,CancellationToken token)
        {
            Require(token);source.Context.RequireCurrent(source,token);
            if(current<0||links is null||links.Count>=CurrentReferences(token).Count||CurrentReferences(token)[links.Count]!=source.Reference||
                !ReferenceEquals(source.Context,owner.graphCharacterContext)||!ReferenceEquals(source.Context.Document,Document)||!ReferenceEquals(source.Context.Selections,Selections)||!payload.Complete)
                throw new InvalidOperationException("Next actual completed same-choice CHAR required.");
            if(payload.AssignedDefaults?.Field40!=-1)throw new NotSupportedException("Actual CHAR override40 needs native handling.");
            if(source.Ordinary)
            {
                Reserve(checked(1024+source.RangeCount*32L),checked(2048+source.RangeCount*64L),1024);
                var key=source.CopyRanges(token);
                if(owner.graphSourceIndicesRequested)AppendWordHeader30(payload,key,token);
                long n=checked((long)ordinary.Length+key.Length);
                Reserve(checked(4096+128*n),checked(8192+256*n+48*n*n),checked(4096+128*n*n));
                ordinary=operations.Union(ordinary,key,token);
            }
            if(owner.graphPeriodicActive&&owner.graphOriginalSelectedRequested&&owner.graphTextPublicationRequested)
            {
                // Same live marked flags only: no full canonical row/UTF8 snapshot is consumed here.
                // Retain the original fixed envelope for guards, lock/scalar reads and the following link allocation.
                Reserve(4096,8192,4096);
                uint actualFlags=payload.Reuse.ReadEnglishCurrentMarkerFlags(source,payload,marker,token);
                choiceFlags=LegacyFreshMainGlobalWordSourceChoice.FoldFlags(choiceFlags,actualFlags,actualFlags);
            }
            else
            {
                Reserve(checked(4096+payload.Reuse.RecordCount*512L),checked(8192+payload.Reuse.RecordCount*1024L),checked(4096+payload.Reuse.RecordCount*256L));
                var rows=payload.Reuse.CopyEnglishRecords(payload,token);
                if((uint)marker>=(uint)rows.Count)throw new NotSupportedException("Actual selected CHAR marker unavailable.");
                choiceFlags=LegacyFreshMainGlobalWordSourceChoice.FoldFlags(choiceFlags,rows[marker].Flags,rows[marker].Flags);
            }
            var b=new byte[24];BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(16),marker);links.Add(new(payload.Node.Identity,marker,b));
            if(owner.graphPostTextCoverageRequested)RetainPostTextLink(source,payload,marker,currentSelectionRows,token);
        }
        internal void Close(CancellationToken token)
        {
            Require(token);var context=owner.graphCharacterContext??throw new InvalidOperationException("Actual CHAR context required.");context.RequireCompleted(token);
            if(current!=next||links is null||links.Count!=CurrentReferences(token).Count)throw new InvalidOperationException("Every current275 CHAR link required.");
            if(owner.graphPostTextCoverageRequested)SealPostTextChoice(token);
            output[current]=new(current,choiceFlags,Pairs(ordinary),LegacyFreshMainGlobalWordSourceChoice.EncodeScalar48(choiceFlags,links.Count),links.ToArray(),owner.graphCharacters!);
            trace.Add("word-choice-complete:"+current);current=-1;next++;links=null;
        }
        internal GraphWordPublicationData CompleteWord(CancellationToken token)
        {
            Require(token);if(finished||wordNode is null||current!=-1||next!=Count)throw new InvalidOperationException("All275 choice completion required.");
            var context=owner.graphCharacterContext??throw new InvalidOperationException("Actual live CHAR context required.");context.RequireCompleted(token);
            if(owner.graphPeriodicActive&&owner.graphOriginalSelectedRequested&&owner.graphTextPublicationRequested&&
                (owner.graphCharacterNodes is null||owner.graphCharacterTree is null||owner.graphCharacterCommands is null))
                throw new InvalidOperationException("Final rich CHAR canonical export must precede WORD completion.");
            flags=FoldGraphWordFlags(flags,output); //native029080, after ALL choices.
            if((flags&0x200000)!=0)selectionId=-1;
            else
            {
                var selection=Selections.CreateEnglishWord(this,token);if(selection is null)throw new NotSupportedException("WORD registration refusal retains native prefix.");selectionId=selection.SelectionId;
                bool same=originalKey.Length==1&&SameCoordinate(originalKey[0]);flags=(flags&~0x800000u)|(same?0x800000u:0);
            }
            trace.Add("word-registered");finished=true;
            var commands=Selections.CopyPendingSelections().Select(x=>new GraphCharacterCommandData(x.SelectionId,x.Name,x.CommandIdentity,x.PreviousCommandIdentity,Pairs(x.CopyRanges()))).ToArray();
            return new(wordNode.Identity,SelectedOrdinal,selectionId,flags,new(-1,0,-1,-1),Pairs(originalKey),output,owner.graphCharacterNodes!,commands,trace.ToArray());
        }
        internal LegacyOwnedFreshTextSelection? RegisterInto(LegacyOwnedFreshTextSourceContext actual,CancellationToken token)
        {
            Require(token);if(finished||next!=Count||current!=-1||wordNode is null||!ReferenceEquals(actual,Selections)||!ReferenceEquals(actual.Document,Document)||!MatchesGraphSelectionBinding(actual,token)||(flags&0x200000)!=0)
                throw new InvalidOperationException("Actual all275 WORD registration prefix required.");
            return actual.CreateEnglishWordCore(this,token);
        }
        internal LegacyNativeInkPositionRange[] CopyRegistrationKey(LegacyOwnedFreshTextSourceContext actual,CancellationToken token)
        {
            Require(token);if(finished||next!=Count||current!=-1||wordNode is null||!ReferenceEquals(actual,Selections)||(flags&0x200000)!=0)throw new InvalidOperationException("Exact completed WORD key required.");
            Reserve(checked(4096+originalKey.Length*64L),checked(8192+originalKey.Length*128L),checked(4096+originalKey.Length*64L));return originalKey.ToArray();
        }
        internal void Poison(){poisoned=true;}
        private static bool SameCoordinate(LegacyNativeInkPositionRange r)=>r.First.SourceIndex==r.Last.SourceIndex&&r.First.PointIndex==r.Last.PointIndex&&(r.First.Fraction==r.Last.Fraction||r.First.Fraction==255||r.Last.Fraction==255);
    }

    internal sealed partial class GraphCharacterContext
    {
        internal void BeginWordChoice(GraphWordPublicationContext parent,object geometry,IReadOnlyList<GraphCharacterRange> nextProduced,CancellationToken token)
        {
            parent.RequireChoiceGeometry(geometry,Document,Stage,Pool,token);
            if(disposed||!completed||current is not null||next!=produced.Count||!ReferenceEquals(owner.graphCharacterContext,this)||!ReferenceEquals(Selections,parent.Selections))
                throw new InvalidOperationException("Previous complete same-owner CHAR choice required before rebinding.");
            long k=0;foreach(var r in nextProduced)k=checked(k+r.Clipped.Length);
            if(nextProduced.Count is<1 or>128||k>4096)throw new NotSupportedException("Bounded complete private next-choice ranges required.");
            Stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(4096+nextProduced.Count*256L+k*64));
            Stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(8192+nextProduced.Count*512L+k*128));
            Stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(4096+k*128));
            geometryIdentity=geometry;produced=nextProduced;ranges=produced.Select(x=>x.Clipped.ToArray()).ToArray();skipped=new bool[produced.Count];positive=Enumerable.Repeat(true,produced.Count).ToArray();joiner=produced.Select(x=>x.Joiner).ToArray();next=0;completed=false;Require(token);
        }
        internal void ReserveWordChoiceSnapshots(GraphWordPublicationContext parent,CancellationToken token)
        {
            RequireCompleted(token);parent.Require(token);long records=0,keys=0;
            foreach(var p in payloads.Values){records=checked(records+p.Reuse.RecordCount);keys=checked(keys+p.RetainedRangeCount);}
            Stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(65536+records*1024+keys*256+parent.TotalReferences*1024L));
            Stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(131072+records*2048+keys*512+parent.TotalReferences*2048L));
            Stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(16384+records*512+keys*128));
        }
    }
}
public sealed partial class LegacyOwnedMainTextSourceInterner
{
    internal LegacyOwnedMainTextSourceInternResult InternEnglishWord(LegacyEnglishCaseNode0Completion.GraphWordPublicationContext actual,CancellationToken token)=>actual.InternInto(this,token);
}
public sealed partial class LegacyOwnedFreshTextSourceContext
{
    internal LegacyOwnedFreshTextSelection? CreateEnglishWord(LegacyEnglishCaseNode0Completion.GraphWordPublicationContext actual,CancellationToken token)=>actual.RegisterInto(this,token);
    internal LegacyOwnedFreshTextSelection? CreateEnglishWordCore(LegacyEnglishCaseNode0Completion.GraphWordPublicationContext actual,CancellationToken token)
    {
        actual.Require(token);if(!ReferenceEquals(actual.Selections,this)||!ReferenceEquals(actual.Document,Document))throw new InvalidOperationException("Actual same-word selection context required.");
        long output=long.MaxValue;return CreateCore(actual.CopyRegistrationKey(this,token),1,ref output,0,token);
    }
}
