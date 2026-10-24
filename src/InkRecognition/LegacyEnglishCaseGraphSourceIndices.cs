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
