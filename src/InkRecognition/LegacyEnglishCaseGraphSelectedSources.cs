using System.Buffers.Binary;
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishSelectedSources.Probe")]
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishSelectedSources.ActualEntry")]
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal sealed record GraphSelectedCharacterData(int SourceNode,uint MetadataFlags,uint Flags,byte[] FlagsKnown4,
        byte[] Field40Before8,byte[] KnownBefore8,byte[] Field40After8,byte[] KnownAfter8,
        bool RawKindLookedUp,int? ConcreteKind,int? ReportedKind);
    internal sealed record GraphSelectedLinkWrite(int SourceNode,int Marker);
    internal sealed record GraphSelectedSourcesData(int CharacterRoot,LegacyMainTextSourceTreeSnapshot[] Tree,
        GraphSelectedCharacterData[] Characters,GraphSelectedLinkWrite[] Writes,uint WordMetadataFlags,uint WordFlags,byte[] WordFlagsKnown4,
        byte[] WordUnknownField40Template8,byte[] WordKnownBefore8,byte[] WordField40After8,byte[] WordKnownAfter8,
        long PendingNext,int PendingCount,bool DocumentApplied,string Scope);
    internal sealed record GraphSelectedSourcesResult(GraphEarlySelectedInputResult Publication,GraphSelectedSourcesData Sources);
    private bool graphSelectedSourcesRequested;
    private GraphSelectedSourcesData? graphSelectedSourcesData;

    internal GraphSelectedSourcesResult RunGraphFirstSelectedSources(LegacyNativeUnicodeNormalizationData normalization,
        CancellationToken token=default,Action<GraphMainConfidenceObservation>? observer=null,bool captureSourceInputs=false)
    {
        primary.RequireGraphLoop(this,token);ArgumentNullException.ThrowIfNull(normalization);
        if(graphSelectedSourcesRequested||graphOriginalSelectedRequested||graphWordPublicationRequested||graphCharactersRequested)
            throw new InvalidOperationException("Selected source pass must retain the early original-selected owner from before registration.");
        graphSelectedSourcesRequested=true;
        var publication=RunGraphFirstOriginalSelectedPostText(normalization,token,observer,captureSourceInputs);
        return new(publication,graphSelectedSourcesData??throw new InvalidOperationException("No completed live selected sources."));
    }

    internal sealed partial class GraphWordPublicationContext
    {
        //Private live post-020000 state. Earlier CHAR/WORD snapshots remain historical.
        private sealed class SelectedCharacterState
        {
            internal readonly GraphCharacterPayload Payload;
            internal readonly uint Metadata;
            internal readonly byte[] Before=new byte[8],BeforeKnown=new byte[8],After=new byte[8],AfterKnown=new byte[8];
            internal readonly bool Lookup;
            internal readonly int? Concrete,Reported;
            internal SelectedCharacterState(GraphCharacterPayload payload,uint metadata,bool lookup,int? concrete,int? reported)
            {
                Payload=payload;Metadata=metadata;Lookup=lookup;Concrete=concrete;Reported=reported;
                //Field40=-1 is retained as DATA from the actual initializer.
                //Only explicit020000 stores establish new byte knownness here.
                BinaryPrimitives.WriteInt64LittleEndian(Before,payload.AssignedDefaults!.Field40);
                Before.CopyTo(After,0);
            }
            internal GraphSelectedCharacterData Copy()=>new(Payload.Node.Identity,Metadata,Payload.Flags,[255,255,255,255],
                Before.ToArray(),BeforeKnown.ToArray(),After.ToArray(),AfterKnown.ToArray(),Lookup,Concrete,Reported);
        }
        private SelectedCharacterState[]? selectedSourceStates;
        private bool selectedSourcesFinished;
        internal GraphSelectedSourcesData CompleteSelectedSources(CancellationToken token)
        {
            //Small scalars/authentication plus full native275 span scan are paid
            //before Require; all later loops have their own reservation below.
            Reserve(256,512,checked(1048576+Count*1024L));Require(token);
            var context=owner.graphCharacterContext??throw new InvalidOperationException("Live CHAR owner required.");
            context.RequireCompleted(token);
            if(selectedSourcesFinished||selectedSourceStates is not null||earlySelected is null||!postTextFinished||!finished||
                textPhase!=4||postTextChoices is null||current!=-1||next!=Count||wordNode is null||(uint)SelectedOrdinal>=(uint)Count||
                !MatchesGraphSelectionBinding(Selections,token))
                throw new InvalidOperationException("Exact live original-selected post-TEXT owner required.");
            int n=context.Interner.Count,objects=Document.ObjectCount;
            if(n is<1 or>4096||n>TotalReferences||objects is<1 or>256||TotalReferences>4096)
                throw new NotSupportedException("Complete bounded same-owner source inventory required.");
            //Insertion/tree copies, indexed payload/state arrays, fixed64 stack,
            //visited/order arrays, pending prefix copies, per-node field40 masks,
            //exports and selected link writes. No returned string/XY copy here.
            //N*objects conservatively pays repeated typed raw-slot validation;
            //no global callback or generic Global source owner is invoked.
            Reserve(checked(16384+n*768L+TotalReferences*64L),checked(32768+n*1536L+TotalReferences*128L),
                checked(16384+Count*1024L+n*(4096L+objects*512L)+TotalReferences*4096L));
            earlySelected.RequireRawOwner(this,token);
            var nodes=context.Interner.CopyInsertionOrder();var tree=context.Interner.CopyTree();int root=context.Interner.RootIdentity;
            if(nodes.Count!=n||tree.Length!=n||root==0||!ReferenceEquals(context.Document,Document)||!ReferenceEquals(context.Selections,Selections))
                throw new InvalidOperationException("Complete actual CHAR interner and source owner required.");
            var byId=new GraphCharacterPayload?[n+1];LegacyOwnedMainTextSourceNode? rootNode=null;
            foreach(var node in nodes)
            {
                token.ThrowIfCancellationRequested();
                if((uint)(node.Identity-1)>=(uint)n||byId[node.Identity] is not null)throw new InvalidDataException("Noncanonical CHAR node identities.");
                byId[node.Identity]=context.RequireSelectedPayload(this,node,token);if(node.Identity==root)rootNode=node;
            }
            if(rootNode is null)throw new InvalidDataException("Missing actual CHAR root.");
            var pending=Selections.CopyPendingSelections();long pendingNext=Selections.NextSelectionId;
            var stack=new LegacyOwnedMainTextSourceNode[64];var visited=new bool[n+1];var order=new int[n];
            selectedSourceStates=new SelectedCharacterState[n+1];int depth=0,count=0;var cursor=rootNode;
            while(cursor is not null||depth!=0)
            {
                while(cursor is not null)
                {
                    token.ThrowIfCancellationRequested();int id=cursor.Identity;
                    if(depth==64||(uint)(id-1)>=(uint)n||visited[id]||!ReferenceEquals(byId[id]?.Node,cursor)||!ReferenceEquals(cursor.Owner,context.Interner))
                        throw new InvalidDataException("Native64-entry complete acyclic same-owner CHAR tree required.");
                    visited[id]=true;stack[depth++]=cursor;cursor=cursor.Left;
                }
                token.ThrowIfCancellationRequested();cursor=stack[--depth];var p=byId[cursor.Identity]!;
                var key=p.Node.Key;uint metadata=p.Flags;bool lookup=false;int? concrete=null,reported=null;
                //Literal lazy01FD40 ordering. Reached prior-owner calls refuse;
                //not-taken guards must not inspect/reconstruct original objects.
                if((metadata&0x10600000u)==0)
                {
                    if((metadata&0x20000000u)!=0)
                    {if(p.SelectionId!=-1)throw new NotSupportedException("Reached native prior source V128 remains unavailable.");}
                    else if(key.Length==1&&key[0].First.SourceIndex==key[0].Last.SourceIndex&&
                        key[0].First.PointIndex==0&&key[0].First.Fraction==0&&key[0].Last.PointIndex==3&&key[0].Last.Fraction==255)
                    {
                        lookup=true;(concrete,reported)=earlySelected.ReadRawKind(this,key[0].First.SourceIndex,token);
                        metadata=(metadata&0xfeffffffu)|(reported==0xc25?0x01000000u:0);
                    }
                }
                p.Flags=metadata;selectedSourceStates[p.Node.Identity]=new(p,metadata,lookup,concrete,reported);
                if(count==n)throw new InvalidDataException("Too many actual CHAR nodes.");order[count++]=p.Node.Identity;cursor=cursor.Right;
            }
            if(count!=n)throw new InvalidDataException("Unvisited actual CHAR nodes.");
            var selected=postTextChoices[SelectedOrdinal]??throw new InvalidOperationException("Actual selected WORD link vector required.");
            if(selected.Links.Length!=span.Words[SelectedOrdinal].FlatReferences.Count||selected.Links.Length==0)
                throw new InvalidOperationException("Exact selected WORD link extent required.");
            bool all=true;
            foreach(var link in selected.Links)
            {
                token.ThrowIfCancellationRequested();RequirePostTextPayload(link,context,token);
                int id=link.Payload.Node.Identity;
                if((uint)(id-1)>=(uint)n||!ReferenceEquals(byId[id],link.Payload)||selectedSourceStates[id] is not{} state)
                    throw new InvalidOperationException("Selected WORD references foreign/unvisited CHAR.");
                if((state.Metadata&0x11004000u)==0)all=false;
            }
            uint wordMetadata=(flags&0xfeffffffu)|(all?0x01000000u:0);
            flags=wordMetadata; //01FD40 WORD bit24 fold precedes020000.
            //This is NOT a read of a native/live WORD field. The old source
            //initializer exposes GraphWordAssignedData(-1,0,-1,-1), but the
            //private context retains no actual field40 backing. Preserve -1
            //only as an explicitly unknown DATA template; no guard, lookup,
            //selector or downstream issuer may consume these template bytes.
            var wordBefore=new byte[8];BinaryPrimitives.WriteInt64LittleEndian(wordBefore,-1);
            var wordBeforeKnown=new byte[8];var wordAfter=wordBefore.ToArray();var wordAfterKnown=new byte[8];
            var writes=new GraphSelectedLinkWrite[selected.Links.Length];
            //020000 inner CHAR loop precedes its outer WORD stores. Repeated
            //canonical keys retain the last raw marker, no dedup/reordering.
            for(int i=0;i<selected.Links.Length;i++)
            {
                token.ThrowIfCancellationRequested();var link=selected.Links[i];var state=selectedSourceStates[link.Payload.Node.Identity];
                link.Payload.Flags|=0x08000000u;WriteSelectedIndex44(state.After,state.AfterKnown,link.Marker);
                writes[i]=new(link.Payload.Node.Identity,link.Marker);
            }
            //Native200BE/200CD run only after inner CHAR loop2009F/200AE.
            flags|=0x08000000u;WriteSelectedIndex44(wordAfter,wordAfterKnown,SelectedOrdinal);
            Require(token);context.RequireCompleted(token);earlySelected.RequireRawOwner(this,token);
            var after=Selections.CopyPendingSelections();
            if(Selections.NextSelectionId!=pendingNext||after.Count!=pending.Count||context.Interner.Count!=n||context.Interner.RootIdentity!=root||
                !context.Interner.CopyTree().AsSpan().SequenceEqual(tree))throw new InvalidOperationException("Selected writes changed source tree/pending cursor.");
            for(int i=0;i<pending.Count;i++){token.ThrowIfCancellationRequested();if(!ReferenceEquals(pending[i],after[i]))throw new InvalidOperationException("Pending command identity changed.");}
            var exported=new GraphSelectedCharacterData[n];for(int i=0;i<n;i++){token.ThrowIfCancellationRequested();exported[i]=selectedSourceStates[order[i]].Copy();}
            selectedSourcesFinished=true;
            return new(root,tree,exported,writes,wordMetadata,flags,[255,255,255,255],wordBefore,wordBeforeKnown,wordAfter,wordAfterKnown,pendingNext,after.Count,false,
                "SAME_EN_EARLY_SELECTED_LIVE_01FD40_020000_NO_020920_NO_DOCUMENT_APPLY");
        }
    }

    //Pure DWORD44 store: lower40..43 and all unrelated mask lanes preserved.
    internal static void WriteSelectedIndex44(Span<byte> field40,Span<byte> known40,int index)
    {
        if(field40.Length!=8||known40.Length!=8||field40.Overlaps(known40))throw new ArgumentException("Separate eight-byte scalar/mask buffers required.");
        BinaryPrimitives.WriteInt32LittleEndian(field40[4..],index);known40[4..].Fill(255);
    }

    internal sealed partial class GraphCharacterContext
    {
        internal GraphCharacterPayload RequireSelectedPayload(GraphWordPublicationContext parent,LegacyOwnedMainTextSourceNode node,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            //The caller authenticated the complete phase before acquiring the
            //private source traversal. There are no callbacks/owner mutations
            //during that traversal; recheck exact live cohort per node instead
            //of rescanning the publisher history for every dictionary lookup.
            if(disposed||!completed||current is not null||!owner.graphFinalCompactBusy||
                !ReferenceEquals(owner.graphLanguageScope,Stage)||!ReferenceEquals(owner.graphCharacterContext,this)||
                !ReferenceEquals(owner.graphWordPublicationContext,parent)||
                !ReferenceEquals(parent.Selections,Selections)||!ReferenceEquals(parent.Document,Document)||
                !ReferenceEquals(node.Owner,Interner)||!payloads.TryGetValue(node.Identity,out var p)||!ReferenceEquals(p.Node,node)||
                !ReferenceEquals(p.Context,this)||!p.Complete||p.AssignedDefaults is null||
                (p.Written&(GraphCharacterWritten.BasicScalars|GraphCharacterWritten.ComputedFlags))!=(GraphCharacterWritten.BasicScalars|GraphCharacterWritten.ComputedFlags))
                throw new InvalidOperationException("Actual initialized owner-authenticated CHAR payload required.");
            return p;
        }
    }

    private sealed partial class GraphEarlySelectedInputReceipt
    {
        internal void RequireRawOwner(GraphWordPublicationContext actual,CancellationToken token)
        {
            if(!RequireActual(actual,actual.Selections,token)||!ReferenceEquals(selected.CurrentView,view)||!ReferenceEquals(view.Document,actual.Document)||
                view.Curves.Count!=actual.Document.ObjectCount||view.Views.Count!=view.Curves.Count)
                throw new InvalidOperationException("Exact retained original-selected raw slots required.");
            //No original-slot lookup until the literal01FD40 predicate reaches it.
        }
        private void ValidateRawSlot(GraphWordPublicationContext actual,int i)
        {
            var curve=view.Curves[i];var entry=view.Views[i];var d=entry.Descriptor;
            if(!ReferenceEquals(curve.Document,actual.Document)||curve.OriginalSourceIndex!=i||curve.NativeConcreteType!=0xc49||
                !ReferenceEquals(entry.Curve,curve)||entry.CurrentViewIndex!=i||d.SourceIndex!=(uint)i||d.IsKnownText||
                d.PointCount!=curve.PointCount||curve.PointCount!=actual.Document.Strokes[i].Points.Count||
                d.FirstPoint!=0||d.LastPoint!=curve.PointCount-1||d.FirstFraction!=0||d.LastFraction!=0||!d.FirstClosed||!d.LastClosed)
                throw new NotSupportedException("Full ordinary immutable C49 current-view original slot required.");
        }
        internal (int Concrete,int Reported) ReadRawKind(GraphWordPublicationContext actual,uint index,CancellationToken token)
        {
            if(!RequireActual(actual,actual.Selections,token)||!ReferenceEquals(selected.CurrentView,view)||index>=(uint)view.Curves.Count)
                throw new InvalidOperationException("Lazy original-object lookup requires actual retained selected owner.");
            ValidateRawSlot(actual,(int)index);
            //Actual C1E.V40=094940 maps owned concrete C49 to reported C1E.
            //No caller-supplied/prepared reported ID, no Global receipt coercion.
            return(view.Curves[(int)index].NativeConcreteType,0xc1e);
        }
    }
}
