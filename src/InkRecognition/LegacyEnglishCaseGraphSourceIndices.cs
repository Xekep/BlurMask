[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishSourceIndices.Probe")]
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishSourceIndices.ActualEntry")]
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal sealed record GraphIndexedSourceData(int SourceNode,uint Flags,int HeaderOffset,ulong[] Pairs);
    internal sealed record GraphIndexNodeData(int Identity,int Page,int Slot,int Left,int Right,sbyte Balance,
        int MembershipHead,int CurrentSource,int? CachedCount,ulong[] Pairs);
    internal sealed record GraphIndexMembershipData(int Identity,int Page,int Slot,int SourceNode,int Next);
    internal sealed record GraphIndexData(int IntervalRoot,int VectorRoot,bool Dirty,int SourcesAdded,
        GraphIndexNodeData[] Intervals,GraphIndexNodeData[] Vectors,GraphIndexMembershipData[] Memberships);
    internal sealed record GraphSourceIndicesData(GraphIndexedSourceData[] Characters,GraphIndexedSourceData[] Words,
        GraphIndexData CharacterIndex,GraphIndexData WordIndex,long PendingNext,int PendingCount,bool Ready,
        bool DocumentApplied,string Scope);
    internal sealed record GraphSourceIndicesResult(GraphSelectedSourcesResult Publication,GraphSourceIndicesData Indices);
    private bool graphSourceIndicesRequested;
    private GraphSourceIndicesData? graphSourceIndicesData;

    internal GraphSourceIndicesResult RunGraphFirstSourceIndices(LegacyNativeUnicodeNormalizationData normalization,
        CancellationToken token=default,Action<GraphMainConfidenceObservation>? observer=null,bool captureSourceInputs=false)
    {
        primary.RequireGraphLoop(this,token);ArgumentNullException.ThrowIfNull(normalization);
        if(graphSourceIndicesRequested||graphSelectedSourcesRequested||graphOriginalSelectedRequested||graphWordPublicationRequested||graphCharactersRequested)
            throw new InvalidOperationException("Fresh owning original-selected source-index path required.");
        graphSourceIndicesRequested=true;
        var publication=RunGraphFirstSelectedSources(normalization,token,observer,captureSourceInputs);
        return new(publication,graphSourceIndicesData??throw new InvalidOperationException("Both live source-index passes required."));
    }

    internal sealed partial class GraphWordPublicationContext
    {
        //Actual fresh V60 zero16 header; accumulates through private VB0 site.
        //Per-choice ordinary remains separate historical DATA for prior stages.
        private LegacyNativeInkPositionRange[] wordOrdinaryAll=[];
        private bool wordHeaderInitialized,sourceIndicesEntered,sourceIndicesReady;
        //Retained private indices, never reconstructed from the detached result.
        //Any future document transaction must continue here before disposal.
        private LegacyMainDocumentSourceIndexKernel<IndexedSource>? characterSourceIndex,wordSourceIndex;
        private void InitializeWordHeader30(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(!owner.graphSourceIndicesRequested||wordHeaderInitialized||wordNode is null||current!=-1||next!=0)
                throw new InvalidOperationException("Literal fresh WORD Header30 initialization point required.");
            wordOrdinaryAll=[];wordHeaderInitialized=true;
        }
        private void AppendWordHeader30(GraphCharacterPayload payload,ReadOnlySpan<LegacyNativeInkPositionRange> actualKey,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(!wordHeaderInitialized||wordNode is null||current!=next||current<0||links is null||
                links.Count>=CurrentReferences(token).Count||!payload.Complete||payload.AssignedDefaults?.Field40!=-1||
                !ReferenceEquals(payload.Context,owner.graphCharacterContext)||!payload.Source.Ordinary)
                throw new InvalidOperationException("Actual successful ordinary same-WORD VB0 append required.");
            var bound=LegacyMainWordHeaderUnionBudget.BeforeUnion(wordOrdinaryAll.Length,actualKey.Length);
            Reserve(bound.Payload,bound.Copies,bound.Visits);
            if(!actualKey.SequenceEqual(payload.Node.Key))throw new InvalidOperationException("Actual append key must equal its live canonical CHAR payload.");
            wordOrdinaryAll=operations.Union(wordOrdinaryAll,actualKey,token);
        }
        //Private sources retain actual payload capabilities until both passes
        //finish. Only detached scalars/ranges leave this owner.
        private sealed class IndexedSource
        {
            internal readonly LegacyOwnedMainTextSourceNode Node;
            internal readonly GraphCharacterPayload? Character;
            internal readonly uint Flags;
            internal readonly int Header;
            internal readonly LegacyNativeInkPositionRange[] Key;
            internal IndexedSource(LegacyOwnedMainTextSourceNode node,GraphCharacterPayload? character,uint flags,int header,
                ReadOnlySpan<LegacyNativeInkPositionRange> key)
            {Node=node;Character=character;Flags=flags;Header=header;Key=key.ToArray();}
            internal GraphIndexedSourceData Copy()=>new(Node.Identity,Flags,Header,Pairs(Key));
        }
        internal GraphSourceIndicesData CompleteSourceIndices(CancellationToken token)
        {
            //Authentication scans full actual275; reserve BEFORE that scan.
            Reserve(256,512,checked(1048576+Count*1024L));Require(token);
            var context=owner.graphCharacterContext??throw new InvalidOperationException("Live CHAR owner required.");
            context.RequireCompleted(token);
            if(sourceIndicesEntered||sourceIndicesReady||characterSourceIndex is not null||wordSourceIndex is not null||!selectedSourcesFinished||selectedSourceStates is null||
                !wordHeaderInitialized||earlySelected is null||wordNode is null||!finished||!postTextFinished||textPhase!=4||current!=-1||next!=Count)
                throw new InvalidOperationException("Exact live post020000 cohort required.");
            earlySelected.RequireRawOwner(this,token);
            int n=context.Interner.Count;
            if(n is<1 or>4096||n>TotalReferences||words.Count!=1||!ReferenceEquals(wordNode.Owner,words))
                throw new NotSupportedException("Bounded complete same-owner insertion lists required.");
            //Insertion arrays/wrappers, source arrays, pending-prefix snapshots,
            //private entries and exported source metadata, before copies/loops.
            Reserve(checked(4096+256L*(n+1)),checked(8192+512L*(n+1)),checked(8192+512L*(n+1)));
            var nodes=context.Interner.CopyInsertionOrder();var wordNodes=words.CopyInsertionOrder();
            if(nodes.Count!=n||wordNodes.Count!=1||!ReferenceEquals(wordNodes[0],wordNode))
                throw new InvalidOperationException("Actual complete WORD/CHAR insertion lists required.");
            var pending=Selections.CopyPendingSelections();long pendingNext=Selections.NextSelectionId;
            //Preserve actual per-tree raw range limits. Unique-node limits are
            //enforced by real Intern (including hit-before-cap behavior), never
            //by conflating duplicate memberships with unique interval nodes.
            long memberships=0;int charCount=0;
            for(int i=0;i<n;i++)
            {
                token.ThrowIfCancellationRequested();var node=nodes[i];
                var p=context.RequireSelectedPayload(this,node,token);
                if(selectedSourceStates[node.Identity] is not{} state||!ReferenceEquals(state.Payload,p)||
                    !ReferenceEquals(node.Previous,i==0?null:nodes[i-1])||!ReferenceEquals(node.Next,i+1==n?null:nodes[i+1])||
                    (p.Written&GraphCharacterWritten.EmptyPriorVector)==0)
                    throw new InvalidOperationException("Actual source insertion order and original-header assignment required.");
                if((p.Flags&0x200000u)!=0)continue;
                memberships=checked(memberships+node.RangeCount);charCount++;
            }
            var wordKey=wordOrdinaryAll.Length!=0?wordOrdinaryAll:originalKey;
            bool addWord=(flags&0x200000u)==0;
            if(memberships>1_048_576||(addWord&&wordKey.Length>65536))
                throw new NotSupportedException("Actual per-tree source-index range bound exceeded before claim.");
            //Fresh private tree owners; no Global/RU receipt, C51 ID fabrication,
            //or Field40 unknown template is consumed by this020920 pass.
            Reserve(1024,2048,1024);sourceIndicesEntered=true;
            var chars=new LegacyMainDocumentSourceIndexKernel<IndexedSource>();
            var wordIndex=new LegacyMainDocumentSourceIndexKernel<IndexedSource>();
            characterSourceIndex=chars;wordSourceIndex=wordIndex;
            var exportedChars=new GraphIndexedSourceData[charCount];
            var exportedWords=new GraphIndexedSourceData[addWord?1:0];int c=0;
            foreach(var node in nodes)
            {
                token.ThrowIfCancellationRequested();var p=context.RequireSelectedPayload(this,node,token);
                if((p.Flags&0x200000u)!=0)continue;
                ReserveIndexAdd(node.RangeCount);
                var actual=new IndexedSource(node,p,p.Flags,0x20,node.Key);
                chars.Add(actual,actual.Key,token);exportedChars[c++]=actual.Copy();
            }
            if(addWord)
            {
                token.ThrowIfCancellationRequested();ReserveIndexAdd(wordKey.Length);
                var actual=new IndexedSource(wordNode,null,flags,wordOrdinaryAll.Length==0?0x20:0x30,wordKey);
                wordIndex.Add(actual,actual.Key,token);exportedWords[0]=actual.Copy();
            }
            Require(token);context.RequireCompleted(token);earlySelected.RequireRawOwner(this,token);
            var after=Selections.CopyPendingSelections();
            if(c!=charCount||Selections.NextSelectionId!=pendingNext||after.Count!=pending.Count||context.Interner.Count!=n||words.Count!=1)
                throw new InvalidOperationException("Source indexing changed registration prefix.");
            for(int i=0;i<pending.Count;i++)
            {token.ThrowIfCancellationRequested();if(!ReferenceEquals(pending[i],after[i]))throw new InvalidOperationException("Pending command identity changed.");}
            var charData=CopyIndex(chars,token);var wordData=CopyIndex(wordIndex,token);
            token.ThrowIfCancellationRequested();sourceIndicesReady=true; //native0209E0 after BOTH passes.
            return new(exportedChars,exportedWords,charData,wordData,pendingNext,after.Count,true,false,
                "SAME_EN_PRIVATE_POST020000_020920_NO_DOCUMENT_APPLY_NO_FRONTEND_C20_OR_GUIDE_CLAIM");
        }
        private void ReserveIndexAdd(int count)
        {
            if(count is<0 or>65536)throw new NotSupportedException("Actual index key bound required.");
            long k=count;
            //k interval Intern calls+one vector Intern. Each creates a path
            //List with<=32 tuples (growth<=64 slots total), one possible node,
            //key clone, and backing-list growth; k membership holders. Includes
            //private source key clone and detached source pair export.
            Reserve(checked(4096+2048*(k+1)+64*k),checked(8192+4096*(k+1)+128*k),checked(8192+8192*(k+1)));
        }
        private GraphIndexData CopyIndex(LegacyMainDocumentSourceIndexKernel<IndexedSource> kernel,CancellationToken token)
        {
            long i=kernel.Intervals.Nodes.Count,v=kernel.Vectors.Nodes.Count,m=kernel.Memberships.Count,keys=0;
            //Count scalar/key lengths before exporting; count traversal has its
            //own reservation before scanning actual nodes.
            Reserve(0,0,checked(256+64*(i+v)));
            foreach(var node in kernel.Intervals.Nodes){token.ThrowIfCancellationRequested();keys=checked(keys+node.Key.Length);}
            foreach(var node in kernel.Vectors.Nodes){token.ThrowIfCancellationRequested();keys=checked(keys+node.Key.Length);}
            Reserve(checked(1024+128*(i+v)+48*m+16*keys),checked(2048+256*(i+v)+96*m+32*keys),checked(1024+128*(i+v+m+keys)));
            GraphIndexNodeData[] CopyNodes(LegacyMainDocumentSourceIndexKernel<IndexedSource>.Tree tree,bool vector)
            {
                var data=new GraphIndexNodeData[tree.Nodes.Count];
                for(int j=0;j<data.Length;j++)
                {
                    token.ThrowIfCancellationRequested();var node=tree.Nodes[j];
                    //No cache-fill operation exists in this private fresh owner.
                    //Every exported vector has completed Add/current-source;
                    //native020860 calls C21.V78=07F820 (explicit [rdx+8]=0).
                    //A failed partial Add is never exported. This is a known
                    //assignment invariant, not managed new-array zero inference.
                    if(vector&&node.CurrentSource is null)throw new InvalidOperationException("Completed vector Add required.");
                    data[j]=new(node.Identity,node.PageIdentity,node.Slot,node.Left?.Identity??0,node.Right?.Identity??0,node.Balance,
                        node.MembershipHead,vector?node.CurrentSource.Node.Identity:0,vector?0:null,Pairs(node.Key));
                }
                return data;
            }
            var intervals=CopyNodes(kernel.Intervals,false);var vectors=CopyNodes(kernel.Vectors,true);
            var members=new GraphIndexMembershipData[kernel.Memberships.Count];
            for(int j=0;j<members.Length;j++)
            {token.ThrowIfCancellationRequested();var x=kernel.Memberships[j];members[j]=new(x.Identity,x.PageIdentity,x.Slot,x.Source.Node.Identity,x.Next);}
            return new(kernel.Intervals.Root?.Identity??0,kernel.Vectors.Root?.Identity??0,kernel.Dirty,kernel.SourcesAdded,intervals,vectors,members);
        }
    }
}

//Private fresh execution-only continuation. The older named diagnostic routes
//continue to export their complete snapshots; no such snapshots are consumed by
//RunGraphFirstPeriodicPendingText. Retain native state, not diagnostic clones.
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal sealed partial class GraphWordPublicationContext
    {
        private bool freshSelectedIndicesFinished;
        private FreshIndexGuard? freshIndexGuard;
        private sealed class FreshIndexGuard : ILegacyFreshSourceIndexGuard
        {
            private readonly GraphWordPublicationContext parent;
            private readonly LegacyEnglishFreshCaseMainSource.Node1QueryLease lease;
            internal FreshIndexGuard(GraphWordPublicationContext parent){this.parent=parent;lease=parent.Stage.Lease;}
            public void Require(CancellationToken token)
            {
                token.ThrowIfCancellationRequested();parent.VisitWithFailureMetadata(GraphFreshContinuationVisitBound.Revocation,nameof(FreshIndexGuard));
                if(parent.poisoned||Volatile.Read(ref parent.owner.phase)!=1||parent.thread!=Environment.CurrentManagedThreadId||
                    !ReferenceEquals(parent.freshIndexGuard,this)||!ReferenceEquals(lease.Full,parent.owner))
                    throw new InvalidOperationException("Released private continuation guard.");
                lease.Root.RequireNode1ContinuationIdentity(lease,token);
            }
        }
        internal bool FreshSelectedIndicesFinished=>freshSelectedIndicesFinished;
        private void CompleteFreshSelectedAndIndices(GraphCharacterContext context,CancellationToken token)
        {
            //Only CompletePostTextCoverage's validated synchronous tail calls
            //this private continuation. No external callback or mutable owner
            //escapes between full validation and this scalar cohort guard.
            token.ThrowIfCancellationRequested();Reserve(256,512,512);
            RequireFreshContinuationCohort(context,token);
            if(!ReferenceEquals(owner.graphCharacterContext,context))throw new InvalidOperationException("Exact validated CHAR context required.");
            if(!owner.graphPeriodicActive||!owner.graphSourceIndicesRequested||!owner.graphSelectedSourcesRequested||
                !finished||!postTextFinished||textPhase!=4||current!=-1||next!=Count||earlySelected is null||
                wordNode is null||!wordHeaderInitialized||!wordScalarsAssigned||wordField48!=-1||
                freshSelectedIndicesFinished||selectedSourcesFinished||sourceIndicesEntered||sourceIndicesReady||selectedSourceStates is not null)
                throw new InvalidOperationException("One fresh private same-owner continuation required.");
            if(freshIndexGuard is not null)throw new InvalidOperationException("Fresh continuation already owns a guard.");
            Reserve(32,32,64);var guard=new FreshIndexGuard(this);freshIndexGuard=guard;
            try
            {
            int n=context.Interner.Count;
            if(n is<1 or>4096||n>TotalReferences||Document.ObjectCount is<1 or>256||TotalReferences>4096)
                throw new NotSupportedException("Bounded same-owner fresh inventory required.");
            //The synchronous private region below has no callback, source query,
            //publisher mutation or externally reachable capability. Coverage's
            //full publisher validation is reused; all native scalar writes and
            //actual index operations below receive their own pre-read bounds.
            Reserve(checked(16384+n*768L+TotalReferences*64L),checked(32768+n*1536L+TotalReferences*128L),
                GraphFreshContinuationVisitBound.Selected(n,TotalReferences,Count));
            var nodes=context.Interner.CopyInsertionOrder();
#if ENGLISH_FRESH_CONTINUATION_CONTROLS
            LegacyFreshContinuationTestHooks.Checkpoint?.Invoke("after-insertion-copy");
#endif
            guard.Require(token);
            if(nodes.Count!=n||words.Count!=1||!ReferenceEquals(wordNode.Owner,words))throw new InvalidDataException("Complete actual insertion lists required.");
            var byId=new GraphCharacterPayload?[n+1];selectedSourceStates=new SelectedCharacterState[n+1];
            foreach(var node in nodes)
            {
                guard.Require(token);
                if((uint)(node.Identity-1)>=(uint)n||byId[node.Identity] is not null)throw new InvalidDataException("Canonical CHAR identities required.");
                byId[node.Identity]=context.RequireSelectedPayload(this,node,token);
            }
            var stack=new LegacyOwnedMainTextSourceNode[64];var visited=new bool[n+1];int depth=0,seen=0;
            int rootIdentity=context.Interner.RootIdentity;
            var cursor=(uint)(rootIdentity-1)<(uint)n?byId[rootIdentity]?.Node:null;
            if(cursor is null)throw new InvalidDataException("Actual CHAR root required.");
            while(cursor is not null||depth!=0)
            {
                while(cursor is not null)
                {
                    guard.Require(token);int id=cursor.Identity;
                    if(depth==64||(uint)(id-1)>=(uint)n||visited[id]||!ReferenceEquals(byId[id]?.Node,cursor)||!ReferenceEquals(cursor.Owner,context.Interner))
                        throw new InvalidDataException("Complete acyclic native64-entry CHAR tree required.");
                    visited[id]=true;stack[depth++]=cursor;cursor=cursor.Left;
                }
                guard.Require(token);cursor=stack[--depth];var payload=byId[cursor.Identity]!;
                var key=payload.Node.Key;uint metadata=payload.Flags;bool lookup=false;int? concrete=null,reported=null;
                if((metadata&0x10600000u)==0)
                {
                    if((metadata&0x20000000u)!=0)
                    {if(payload.SelectionId!=-1)throw new NotSupportedException("Native prior source V128 unavailable.");}
                    else if(key.Length==1&&key[0].First.SourceIndex==key[0].Last.SourceIndex&&key[0].First.PointIndex==0&&
                        key[0].First.Fraction==0&&key[0].Last.PointIndex==3&&key[0].Last.Fraction==255)
                    {
                        Reserve(0,0,GraphFreshContinuationVisitBound.RawLookup);
                        lookup=true;(concrete,reported)=earlySelected.ReadRawKind(this,key[0].First.SourceIndex,token);
                        metadata=(metadata&0xfeffffffu)|(reported==0xc25?0x01000000u:0);
                    }
                }
                var selectedState=new SelectedCharacterState(payload,metadata,lookup,concrete,reported);
                guard.Require(token);payload.Flags=metadata;selectedSourceStates[payload.Node.Identity]=selectedState;
                if(++seen>n)throw new InvalidDataException("Extra CHAR node.");cursor=cursor.Right;
            }
            if(seen!=n)throw new InvalidDataException("Unvisited CHAR node.");
            var chosen=postTextChoices![SelectedOrdinal]??throw new InvalidOperationException("Actual selected WORD links required.");
            if(chosen.Links.Length==0||chosen.Links.Length!=span.Words[SelectedOrdinal].FlatReferences.Count)
                throw new InvalidOperationException("Exact selected WORD link extent required.");
            bool all=true;
            foreach(var link in chosen.Links)
            {
                guard.Require(token);RequirePostTextPayload(link,context,token);
                int id=link.Payload.Node.Identity;
                if((uint)(id-1)>=(uint)n||!ReferenceEquals(byId[id],link.Payload)||selectedSourceStates[id] is not{} state)
                    throw new InvalidOperationException("Selected link references foreign CHAR.");
                if((state.Metadata&0x11004000u)==0)all=false;
            }
            guard.Require(token);flags=(flags&0xfeffffffu)|(all?0x01000000u:0);
            foreach(var link in chosen.Links)
            {
                guard.Require(token);var state=selectedSourceStates[link.Payload.Node.Identity];
                link.Payload.Flags|=0x08000000u;WriteSelectedIndex44(state.After,state.AfterKnown,link.Marker);
            }
            guard.Require(token);flags|=0x08000000u;wordField40=(wordField40&0xffffffffL)|((long)(uint)SelectedOrdinal<<32);
            selectedSourcesFinished=true;
            //020920 traverses insertion order, not the in-order metadata tree.
            Reserve(4096+256L*(n+1),8192+512L*(n+1),GraphFreshContinuationVisitBound.IndexInventory(n));
            var chars=new LegacyMainDocumentSourceIndexKernel<IndexedSource>();var wordIndex=new LegacyMainDocumentSourceIndexKernel<IndexedSource>();
            characterSourceIndex=chars;wordSourceIndex=wordIndex;sourceIndicesEntered=true;int charMax=0;
            foreach(var node in nodes)
            {
                guard.Require(token);var payload=byId[node.Identity]!;
                if(selectedSourceStates[node.Identity] is not{} state||!ReferenceEquals(state.Payload,payload)||
                    (payload.Written&GraphCharacterWritten.EmptyPriorVector)==0)throw new InvalidOperationException("Actual selected CHAR state required.");
                if((payload.Flags&0x200000u)!=0)continue;
                Add(chars,node,payload,payload.Flags,0x20,node.Key,ref charMax);
            }
            var wordKey=wordOrdinaryAll.Length!=0?wordOrdinaryAll:originalKey;int wordMax=0;
            if((flags&0x200000u)==0)Add(wordIndex,wordNode,null,flags,wordOrdinaryAll.Length==0?0x20:0x30,wordKey,ref wordMax);
            sourceIndicesReady=true;
            CompleteFreshCleanupAndInputTail(nodes,byId,guard,token);
            freshSelectedIndicesFinished=true;
            void Add(LegacyMainDocumentSourceIndexKernel<IndexedSource> index,LegacyOwnedMainTextSourceNode node,GraphCharacterPayload? payload,
                uint sourceFlags,int header,ReadOnlySpan<LegacyNativeInkPositionRange> key,ref int maxKey)
            {
                guard.Require(token);Reserve(0,0,64);
                int intervals=index.Intervals.Nodes.Count,vectors=index.Vectors.Nodes.Count;
                long k=key.Length;
                Reserve(checked(4096+2048*(k+1)+64*k),checked(8192+4096*(k+1)+128*k),
                    GraphFreshContinuationVisitBound.IndexAdd(key.Length,intervals,vectors,maxKey));
                var source=new IndexedSource(node,payload,sourceFlags,header,key);
                guard.Require(token);index.Add(source,source.Key,token,freshIndexGuard);maxKey=Math.Max(maxKey,key.Length);
            }
            }
            finally{freshIndexGuard=null;}
        }
        private sealed record FreshDomCharacter(GraphCharacterPayload Payload,uint[] Flags,byte[][] Utf8);
        private void PrepareFreshDomCharacters(IReadOnlyList<LegacyOwnedMainTextSourceNode> nodes,
            GraphCharacterPayload?[] byId,FreshIndexGuard guard,CancellationToken token)
        {
            guard.Require(token);Reserve(checked(128L+8L*nodes.Count),checked(256L+16L*nodes.Count),128);
            if(freshDomCharacters is not null)throw new InvalidOperationException("Fresh DOM character inventory already captured.");
            var result=new FreshDomCharacter[nodes.Count];
            for(int i=0;i<nodes.Count;i++)
            {
                guard.Require(token);Reserve(0,0,64);var payload=byId[nodes[i].Identity]??throw new InvalidOperationException("Actual CHAR source required.");
                int records=payload.Reuse.RecordCount;
                if(records is<1 or>65536)throw new NotSupportedException("Bounded actual CHAR candidate inventory required.");
                Reserve(checked(512L+128L*records),checked(1024L+256L*records),checked(512L+64L*records));
                var copied=payload.Reuse.CopyFreshEnglishDomRows(this,payload,records,guard,token);
                guard.Require(token);result[i]=new(payload,copied.Flags,copied.Utf8);
            }
            guard.Require(token);freshDomCharacters=result;
        }
        internal void RequireFreshDomRowOwner(GraphCharacterPayload payload,LegacyPreparedMainTextGlyphReuse reuse,
            ILegacyFreshSourceIndexGuard actualGuard,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();VisitWithFailureMetadata(160,nameof(RequireFreshDomRowOwner));
            var guard=freshIndexGuard??throw new InvalidOperationException("Private fresh DOM continuation ended.");
            if(!ReferenceEquals(guard,actualGuard))throw new InvalidOperationException("Actual opaque fresh guard required.");
            guard.Require(token);
            int id=payload.Node.Identity;
            if(freshDomTarget is null||!freshCounterFinished||!ReferenceEquals(payload.Reuse,reuse)||
                !ReferenceEquals(payload.Context,owner.graphCharacterContext)||!payload.Complete||
                selectedSourceStates is null||(uint)id>=(uint)selectedSourceStates.Length||
                !ReferenceEquals(selectedSourceStates[id]?.Payload,payload))
                throw new InvalidOperationException("Exact live native DOM character source required.");
        }
        internal void PrechargeFreshDomTextCopy(int bytes,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(bytes<0)throw new InvalidDataException("Invalid actual UTF8 extent.");
            Reserve(bytes,checked(2L*bytes),checked(128L+2L*bytes));
        }
        private sealed class FreshDomTarget(LegacyLibraryTextInputRequest request,LegacyLibraryTextDomArena reader,
            LegacyDomNode field,LegacyLibraryTextDomArena writer,int ordinal,LegacyDomNode? previous,int originalChildren,LegacyLibraryPreparedDomNames names)
        {
            internal readonly LegacyLibraryPreparedDomNames Names=names;
            internal readonly LegacyLibraryTextInputRequest Request=request;
            internal readonly LegacyLibraryTextDomArena Reader=reader,Writer=writer;
            internal readonly LegacyDomNode Field=field;
            internal readonly LegacyDomNode? Previous=previous;
            internal readonly int InsertionOrdinal=ordinal,OriginalChildCount=originalChildren;
        }
        private void PrepareFreshDomTarget(LegacyEnglishOriginalInputContext pre,LegacyEnglish119ParentSubmission sourceParent,
            FreshIndexGuard guard,CancellationToken token)
        {
            guard.Require(token);Reserve(128,256,512);
            if(!freshCounterFinished||freshDomTarget is not null)throw new InvalidOperationException("One actual post-counter fresh2E7 target required.");
            var actual=pre.BorrowFreshDomTarget(sourceParent,token);
            int children=actual.Field.Children.Count;
            if(children>128)throw new NotSupportedException("Bounded actual request-field child inventory required.");
            Reserve(0,0,checked(128L+96L*children));
            int ordinal=children;LegacyDomNode? previous=null;
            for(int i=0;i<children;i++)
            {
                guard.Require(token);var child=actual.Field.Children[i];
                if(child.Released||!ReferenceEquals(child.Parent,actual.Field))throw new InvalidOperationException("Actual live request children required.");
                //061510 chooses the last actual textResult child and ordinal.
                if(string.Equals(child.Name,"textResult",StringComparison.Ordinal)){ordinal=i;previous=child;}
            }
            //Removal must precede source.V30 on a nonempty prior subtree. This
            //fresh bootstrap has actual INPUT child; it is not an empty root.
            if(previous is not null)throw new NotSupportedException("Actual previous textResult requires native BE2 removal before source.V30.");
            guard.Require(token);Reserve(256,512,256);
            var writer=new LegacyLibraryTextDomArena(actual.Request);
            guard.Require(token);
            if(!ReferenceEquals(writer.NamePool,actual.Reader.NamePool)||!ReferenceEquals(writer.Document,Document))
                throw new InvalidOperationException("Actual request-reader name owner required for fresh writer.");
            freshDomTarget=new(actual.Request,actual.Reader,actual.Field,writer,ordinal,previous,children,actual.Names);
        }
        internal (string Name,int Identity) RequireFreshDomName(LegacyLibraryTextDomArena actual,int slot,string expected,CancellationToken token)
        {
            var guard=freshIndexGuard??throw new InvalidOperationException("Actual private DOM continuation ended.");
            guard.Require(token);Reserve(0,0,128);
            var target=freshDomTarget??throw new InvalidOperationException("Actual request DOM target required.");
            if(!ReferenceEquals(target.Writer,actual)||expected.Length>16)throw new InvalidOperationException("Exact fresh writer and native literal name required.");
            return target.Names.Require(target.Request,target.Reader.NamePool,slot,expected);
        }
        internal (string Name,int Identity) PrepareFreshDomNode(LegacyLibraryTextDomArena actual,int slot,string expected,
            LegacyDomNode? attributeOwner,int bytes,CancellationToken token)
        {
            (string Name,int Identity) name;
            if(slot>=0)name=RequireFreshDomName(actual,slot,expected,token);
            else{RequireFreshDomPublication(actual,token);Reserve(0,0,96);name=("",0);}
            Reserve(0,0,64);var layout=actual.ReadEnglishLayout(attributeOwner);
            if(layout.Nodes>=65536||layout.Attributes>16||bytes<0)throw new NotSupportedException("Bounded native fresh DOM node/attribute inventory required.");
            long nodeGrow=layout.Nodes==layout.NodeCapacity?Math.Max(4L,2L*layout.NodeCapacity):0;
            long attrGrow=attributeOwner is not null&&layout.Attributes==layout.AttributeCapacity?Math.Max(4L,2L*layout.AttributeCapacity):0;
            long shifts=(nodeGrow!=0?layout.Nodes:0L)+(attrGrow!=0?layout.Attributes:0L);
            Reserve(checked(512+8*(nodeGrow+attrGrow)+bytes),checked(1024+16*(nodeGrow+attrGrow)+4L*bytes),
                checked(128+16*shifts+32L*layout.Attributes+4L*bytes));
            return name;
        }
        internal void RequireFreshDomPublication(LegacyLibraryTextDomArena actual,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();VisitWithFailureMetadata(32,nameof(RequireFreshDomPublication));
            var guard=freshIndexGuard??throw new InvalidOperationException("Actual lexical DOM continuation ended.");guard.Require(token);
            if(!ReferenceEquals(freshDomTarget?.Writer,actual))throw new InvalidOperationException("Exact fresh writer required.");
        }
        internal void PrepareFreshDomAppend(LegacyLibraryTextDomArena actual,LegacyDomNode parent,CancellationToken token)
        {
            RequireFreshDomPublication(actual,token);Reserve(0,0,384);
            int count=parent.Children.Count,capacity=parent.Children.Capacity;
            long growth=count==capacity?Math.Max(4L,2L*capacity):0;
            Reserve(checked(8*growth),checked(32+16*growth),growth!=0?checked(16L*count):0);
        }
        private sealed record FreshDomRangeEntry(LegacyNativeInkPositionRange[] Key,bool AlterFirstBrace,byte[] Bytes);
        private void BuildFreshTextDom(IReadOnlyList<LegacyOwnedMainTextSourceNode> nodes,FreshIndexGuard guard,CancellationToken token)
        {
            guard.Require(token);Reserve(128,256,256);
            var target=freshDomTarget??throw new InvalidOperationException("Actual request DOM target required.");
            if(freshDomEntered||freshDomBuilt||freshDomRoot is not null||freshDomCharacters is null||freshDomCharacters.Length!=nodes.Count||freshPublicationIdentifier is null||
                freshDocumentAlternativeCount!=1||wordPriorCount38!=0||wordFreeze1C!=0||freshTextFreezeB8!=0||freshTextSelectedBC!=0||
                (int)wordField40!=-1||freshTextSelectionB0<0||textKey is null||textLines is null||textLines.Lines.Length!=1)
                throw new NotSupportedException("Actual fresh ordinary singleton document source fields required.");
            freshDomEntered=true;var arena=target.Writer;
            //Owned only by this synchronous source.V30 invocation. Exact raw
            //key contents and the only effective caller-rewrite bit determine
            //the final UTF8 value. Attribute services still clone each value.
            Reserve(64,128,64);var rangeCache=new List<FreshDomRangeEntry>();
            var root=Element(2,"textResult");freshDomRoot=root;
            String(root,32,"gen",freshPublicationIdentifier.CopyUtf8Storage36());
            var chars=Element(3,"charSegmentList");Append(root,chars);
            foreach(var source in freshDomCharacters)
            {
                guard.Require(token);Reserve(0,0,128);var payload=source.Payload;
                if(payload.AssignedDefaults is not{Field40:-1,Field1C:0}||(payload.Written&GraphCharacterWritten.EmptyPriorVector)==0)
                    throw new NotSupportedException("Native fresh CHAR override/freeze/origin fields required.");
                var segment=Element(4,"charSegment");Range(segment,18,"range",payload.Node.Key,payload.Flags,true);
                if(payload.SelectionId!=-1)Number(segment,19,"tag",4,payload.SelectionId);
                if(payload.LineId!=-1)Number(segment,16,"line",7,payload.LineId);
                for(int r=0;r<source.Utf8.Length;r++)
                {
                    guard.Require(token);Reserve(0,0,32);
                    LegacyFreshMainDetachedTextDom.ValidateOrdinaryFlags(source.Flags[r]);
                    var candidate=Element(5,"charCandidate");var text=arena.EnglishText(this,source.Utf8[r],token)??throw new InvalidOperationException("Native ordinary text-node refusal.");
                    Append(candidate,text);Append(segment,candidate);
                }
                Append(chars,segment);
            }
            var words=Element(8,"wordSegmentList");Append(root,words);
            var word=Element(9,"wordSegment");var wordKey=wordOrdinaryAll.Length==0?originalKey:wordOrdinaryAll;
            Range(word,18,"range",wordKey,flags,true);if(selectionId!=-1)Number(word,19,"tag",4,selectionId);
            for(int c=0;c<Count;c++)
            {
                guard.Require(token);var row=postTextChoices![c]??throw new InvalidOperationException("Actual WORD48 links required.");
                var candidate=Element(10,"wordCandidate");
                foreach(var link in row.Links)
                {
                    guard.Require(token);Reserve(0,0,128);RequirePostTextPayload(link,owner.graphCharacterContext!,token);
                    var reference=Element(11,"char");Range(reference,24,"ref",link.Payload.Node.Key,link.Payload.Flags,true);
                    Number(reference,25,"selected",7,link.Marker);Append(candidate,reference);
                }
                Append(word,candidate);
            }
            Append(words,word);
            var textNode=Element(12,"text");Number(textNode,19,"tag",4,freshTextSelectionB0);
            Range(textNode,18,"range",textKey,0,false);Number(textNode,25,"selected",7,freshTextSelectedBC);
            var textCandidate=Element(13,"textCandidate");var wordReference=Element(14,"word");
            Range(wordReference,24,"ref",wordKey,flags,true);Number(wordReference,25,"selected",7,SelectedOrdinal);
            Append(textCandidate,wordReference);Append(textNode,textCandidate);Append(root,textNode);
            var lines=Element(15,"lines");
            foreach(var line in textLines.Lines)
            {
                guard.Require(token);Reserve(0,0,128);
                if(line.OwnerTag>=0)throw new NotSupportedException("Fresh native line tagId requires its separately interned name slot.");
                var node=Element(16,"line");
                if(Math.Abs(line.OriginX)>=1.1920928955078125e-7&&Math.Abs(line.Angle)>=1.1920928955078125e-7)Double(node,27,"x",line.OriginX);
                Double(node,28,"y",line.OriginY);Double(node,29,"a",line.Angle);Double(node,30,"h",line.XHeight);Append(lines,node);
            }
            Append(root,lines);guard.Require(token);freshDomBuilt=true;
            LegacyDomNode Element(int slot,string name)=>arena.EnglishElement(this,slot,name,token)??throw new InvalidOperationException("Actual native DOM element refusal.");
            void Append(LegacyDomNode parent,LegacyDomNode child)=>arena.EnglishAppend(this,parent,child,token);
            void String(LegacyDomNode node,int slot,string name,byte[] bytes)
            {if(!arena.EnglishStringAttribute(this,node,slot,name,bytes,token))throw new InvalidOperationException("Actual native DOM string-attribute refusal.");}
            void Number(LegacyDomNode node,int slot,string name,byte tag,long raw)
            {if(!arena.EnglishNumericAttribute(this,node,slot,name,tag,raw,token))throw new InvalidOperationException("Actual native DOM numeric-attribute refusal.");}
            void Double(LegacyDomNode node,int slot,string name,double value)=>Number(node,slot,name,5,BitConverter.DoubleToInt64Bits(value));
            void Range(LegacyDomNode node,int slot,string name,ReadOnlySpan<LegacyNativeInkPositionRange> key,uint metadata,bool rewrite)
            {
                guard.Require(token);Reserve(0,0,64);
                bool alter=rewrite&&(metadata&0x400000u)!=0;bool firstProbe=true;
                foreach(var cached in rangeCache)
                {
                    //First probe is immediately adjacent to the gate above:
                    //only stack scalars/List enumerator metadata intervene.
                    //Later probes renew opaque revocation before each key.
                    if(!firstProbe)guard.Require(token);firstProbe=false;
                    Reserve(0,0,64);
                    if(cached.AlterFirstBrace!=alter||cached.Key.Length!=key.Length)continue;
                    Reserve(0,0,checked(32L*key.Length));
                    if(!cached.Key.AsSpan().SequenceEqual(key))continue;
                    //StringAttribute ->Literal always copies; it cannot mutate
                    //this immutable cached value or alias it into a DOM node.
                    String(node,slot,name,cached.Bytes);return;
                }
                Reserve(0,0,checked(64L+128L*key.Length));
                bool canonical=LegacyNativeSourceRangeUtf8.IsCanonical(key);long k=key.Length;
                Reserve(checked(4096+256*k),checked(8192+512*k+32*k*k),checked(512+128*k+(canonical?0:512*k*k)));
                var formatted=LegacyNativeSourceRangeUtf8.Format(key,operations,token:token);
                if(!formatted.Succeeded)throw new InvalidOperationException("Actual source-range formatting refused.");
                guard.Require(token);
                byte[] bytes;
                if(alter){Reserve(formatted.Bytes.Length,checked(2L*formatted.Bytes.Length),checked(64L+2L*formatted.Bytes.Length));bytes=LegacyNativeSourceRangeUtf8.CallerFirstBrace(formatted.Bytes,metadata);}
                else bytes=formatted.Bytes; //removed the old unnecessary no-op clone
                Reserve(0,0,64);int cacheCount=rangeCache.Count,cacheCapacity=rangeCache.Capacity;
                if(cacheCount>=nodes.Count+2)throw new InvalidDataException("One immutable key per CHAR/WORD/TEXT source required.");
                long growth=cacheCount==cacheCapacity?Math.Max(4L,2L*cacheCapacity):0;
                Reserve(checked(64+16*k+8*growth),checked(128+32*k+16*growth),checked(128+32*k+(growth!=0?16L*cacheCount:0)));
                var ownedKey=key.ToArray();
#if ENGLISH_FRESH_CONTINUATION_CONTROLS
                LegacyFreshContinuationTestHooks.Checkpoint?.Invoke("after-dom-range-cache-copy");
#endif
                guard.Require(token);rangeCache.Add(new(ownedKey,alter,bytes));
                String(node,slot,name,bytes);
            }
        }
        private void CompleteFreshCleanupAndInputTail(IReadOnlyList<LegacyOwnedMainTextSourceNode> nodes,
            GraphCharacterPayload?[] byId,FreshIndexGuard guard,CancellationToken token)
        {
            if(freshControlCleanupFinished||freshInputTailFinished||freshCounterFinished||freshReleasedTransforms is not null)
                throw new InvalidOperationException("One fresh cleanup/transform tail required.");
            //023260: actual WORD48 rows in order, then aggregate mask clear,
            //then actual CHAR insertion list. Active branches refuse in place.
            Reserve(0,0,checked(128L+32L*Count+32L*nodes.Count));
            for(int c=0;c<Count;c++)
            {
                guard.Require(token);
                var row=output[c]??throw new InvalidOperationException("Actual WORD48 row required.");
                LegacyFreshMainGlobalControlCleanup.RequireNoWordControl(row.Flags);
            }
            guard.Require(token);flags&=0xffefffff;
            foreach(var node in nodes)
            {
                guard.Require(token);var payload=byId[node.Identity]??throw new InvalidOperationException("Actual CHAR payload required.");
                LegacyFreshMainGlobalControlCleanup.RequireNoCharacterControl(payload.Flags);
            }
            guard.Require(token);freshControlCleanupFinished=true;
            //0283B0/0297EC reads actual imported SourceTag30, NOT newly
            //registered TEXT B0. Reuse only the actual pre-NN immutable export.
            guard.Require(token);Reserve(128,256,512);
            var sourceParent=Stage.Lease.Root.Knowledge.Owner.Owner.Owner.Parent;
            var pre=sourceParent.OriginalInputContext??throw new InvalidOperationException("Actual pre-NN imported owner required.");
            var geometry=pre.BorrowFreshTransformGeometry(sourceParent,Selections.InputBinding!,out long tag,out int oldCount,token);
            if(freshPublicationIdentifier is null||tag==-1||oldCount!=0||freshDocumentAlternativeCount!=1||freshDocumentChoice32.Length!=32||geometry.NativeNonidentityTransformCount!=0)
                throw new NotSupportedException("The actual nonempty fresh original-selected identity export is required.");
            //One real document choice was constructed before all WORD48 rows;
            //Count is WORD choice count and must never substitute for D8.
            Reserve(64,128,128);guard.Require(token);
            var swapped=LegacyDocumentTransformSwap.Apply(ref freshTransforms,LegacyDocumentTransformVector.Fresh(),true);
            freshReleasedTransforms=swapped.Released;
            if(!swapped.ExportSucceeded)throw new InvalidOperationException("Native export failure retains the swapped prefix.");
            //026EF0 returns immediately only for actual empty INPUT children.
            guard.Require(token);
            //Single examines every actual selected DOM node; do not use a
            //fixture-sized fixed charge for a variable node inventory.
            Reserve(0,0,checked(128L+32L*pre.SelectedState.Nodes.Count));
            var daf=Selections.InputBinding!.CopyDafInputRows32();
            if(daf.Length!=0)throw new NotSupportedException("Nonempty actual INPUT spans require native026EF0 matching.");
            guard.Require(token);freshInputTailFinished=true;
            Reserve(64,128,512);
            var counter=Selections.AppendOwnedEnglishFreshCounter(this,token);
            guard.Require(token);
            if(!counter.Succeeded)throw new InvalidOperationException("Actual fresh C4D counter append failed.");
            freshCounterFinished=true;
            PrepareFreshDomTarget(pre,sourceParent,guard,token);
            PrepareFreshDomCharacters(nodes,byId,guard,token);
            BuildFreshTextDom(nodes,guard,token);
        }
        internal (long Old,long Next,ILegacyFreshSourceIndexGuard Guard) RequireFreshCounterOwner(
            LegacyOwnedFreshTextSourceContext actual,CancellationToken token)
        {
            var guard=freshIndexGuard??throw new InvalidOperationException("The actual lexical fresh continuation has ended.");
            guard.Require(token);
            if(!ReferenceEquals(owner.graphWordPublicationContext,this)||!ReferenceEquals(actual,Selections)||
                !sourceIndicesReady||!freshControlCleanupFinished||!freshInputTailFinished||freshCounterFinished||
                textPhase!=4||!finished||!postTextFinished||Selections.InputBinding is null)
                throw new InvalidOperationException("Exact live fresh transform/selection counter owner required.");
            return(Selections.InputBinding.Next,Selections.NextSelectionId,guard);
        }
        private void RequireFreshContinuationCohort(GraphCharacterContext context,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();VisitWithFailureMetadata(GraphFreshContinuationVisitBound.Cohort,nameof(RequireFreshContinuationCohort));
            if(poisoned||Volatile.Read(ref owner.phase)!=1||thread!=Environment.CurrentManagedThreadId||!owner.graphFinalCompactBusy||!ReferenceEquals(owner.graphLanguageScope,Stage)||
                !ReferenceEquals(owner.graphWordPublicationContext,this)||!ReferenceEquals(owner.graphCharacterContext,context)||
                !ReferenceEquals(context.Document,Document)||!ReferenceEquals(context.Selections,Selections)||current!=-1||next!=Count||!finished||!postTextFinished||textPhase!=4)
                throw new InvalidOperationException("Private fresh continuation cohort changed.");
            //No deep publisher/history rewalk: the lexical region above can
            //only mutate its private metadata/index state, on this same thread.
            if(!ReferenceEquals(owner.node1Pass,Stage.Lease.Pass)||!ReferenceEquals(Stage.Lease.Full,owner))throw new InvalidOperationException("Changed private continuation pass.");
            Stage.Lease.Root.RequireNode1ContinuationIdentity(Stage.Lease,token);
        }
    }
}
internal static class GraphFreshContinuationVisitBound
{
    //<=243 semantic scalar bytes/primitive tests plus token, lock and call overhead.
    internal const long Cohort=512,Revocation=256,RawLookup=1024;
    internal static long Selected(int nodes,int references,int choices)
    {
        if(nodes is<1 or>4096||references is<1 or>4096||choices is<1 or>4096)throw new ArgumentOutOfRangeException();
        //Fixed scalar guards/pending-free traversal; per-node insertion copy,
        //dictionary worst-chain authentication, native64-stack walk, metadata
        //and state holder; per-link validation+selected DWORD stores. No tree,
        //pending or field-mask diagnostic snapshots are copied/exported.
        return checked(2048L+512L*nodes+64L*nodes*nodes+256L*references+128L*choices);
    }
    internal static long IndexInventory(int nodes)
    {if(nodes is<1 or>4096)throw new ArgumentOutOfRangeException();return checked(1024L+256L*nodes);}
    internal static long IndexAdd(int ranges,int intervals,int vectors,int maximumExistingVectorKey)
    {
        if(ranges is<0 or>65536||intervals is<0 or>65536||vectors is<0 or>65536||maximumExistingVectorKey is<0 or>65536)throw new ArgumentOutOfRangeException();
        //Closed form: no uncharged O(ranges) arithmetic preflight loop.
        long below=Math.Min((long)ranges,Math.Max(0L,32L-intervals));
        long depthSum=checked(below*intervals+below*(below-1)/2+((long)ranges-below)*32);
        return checked(512+640L*(ranges+1L)+224*depthSum+
            Math.Min(32,vectors)*(192L+32L*Math.Min(ranges,maximumExistingVectorKey)));
    }
}

#if ENGLISH_FRESH_CONTINUATION_CONTROLS
internal static class LegacyFreshContinuationTestHooks
{internal static Action<string>? Checkpoint;internal static Action<LegacyPreparedDictionaryQuery?>? WrapperObserved;}
#endif
