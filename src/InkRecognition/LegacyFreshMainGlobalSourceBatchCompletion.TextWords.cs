using System.Buffers.Binary;
namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalSourceBatchCompletion
{
    private CompletedTextWords? preparedTextWords;
    private long earlyTextPreparationCopyBytes;
    internal void ChargeTextPreparationCopy(long bytes,CancellationToken token)
    {
        RequireCompleted(token);
        if(preparedTextWords is not null){preparedTextWords.ReservePreparationCopy(bytes,token);return;}
        if(bytes<0||bytes>RemainingEnumeratedCopyBudget(token))throw new NotSupportedException("Owned TEXT resource preparation exceeds cumulative remaining copy envelope.");
        earlyTextPreparationCopyBytes+=bytes;
    }

    //This internal issuer is deliberately not an overload of singular TEXT
    //publication. One later graph transaction must consume the whole cursor;
    //calling the old one-WORD parent repeatedly would claim/register repeatedly.
    internal CompletedTextWords PrepareTextWords(CancellationToken token)
    {
        RequireCompleted(token);
        if(preparedTextWords is null)preparedTextWords=new(this,token);
        preparedTextWords.Require(token);return preparedTextWords;
    }
    internal LegacyLibraryGlobalTextPublicationScope TextScope(CancellationToken token){RequireCompleted(token);return scope;}
    internal LegacyFreshMainGlobalPlainRecognitionResult TextSource(CancellationToken token){RequireCompleted(token);return source;}
    internal LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot TextSnapshot(CancellationToken token){RequireCompleted(token);return snapshot;}

    internal sealed class CompletedTextWords
    {
        private readonly LegacyFreshMainGlobalSourceBatchCompletion owner;
        private readonly LegacyNativePlainWordSpanSnapshot[] owners;
        private readonly int[] ownerIndices,selected;
        private readonly LegacyNativeInkPositionRange[][] keys;
        private readonly int thread=Environment.CurrentManagedThreadId;
        private readonly long copyLimit;
        private int entered,next;
        private WordCursor? current;
        private bool poisoned,finished,reserved;
        private long publicationReserve;
        private readonly long validationLoopBodies;
        internal int Count=>owner.binding.Nodes.Length;
        internal int TotalChoices{get;}
        internal int TotalReferences{get;}
        internal long EnumeratedCopyBytes{get;private set;}
        internal long LookaheadPairs{get;}
        internal long EqualityEndpointReadBound{get;}
        internal LegacyFreshMainGlobalSourceBatchCompletion Completion=>owner;
        internal LegacyNativePublishedWordNode Head(CancellationToken token){Require(token);return owner.binding.Nodes[0];}
        internal LegacyNativePublishedWordNode NodeAt(int ordinal,CancellationToken token)
        {Require(token);if((uint)ordinal>=Count)throw new ArgumentOutOfRangeException(nameof(ordinal));return owner.binding.Nodes[ordinal];}
        internal LegacyNativePlainWordSpanSnapshot SpanAt(int ordinal,CancellationToken token)
        {Require(token);if((uint)ordinal>=Count)throw new ArgumentOutOfRangeException(nameof(ordinal));return owners[ownerIndices[ordinal]];}
        internal LegacyNativePublishedWordNode CurrentNode(CancellationToken token)
        {Require(token);return owner.binding.Nodes[current?.Ordinal??0];}
        internal bool IsCurrentNode(LegacyNativePublishedWordNode actual,CancellationToken token)
        {Require(token);return current is not null&&ReferenceEquals(owner.binding.Nodes[current.Ordinal],actual);}
        internal LegacyNativeInkPositionRange[] CopyCurrentKey(CancellationToken token)
        {Require(token);int index=current?.Ordinal??0;ChargeExport(keys[index].Length);return keys[index].ToArray();}
        internal void ReservePublication(long bytes,CancellationToken token)
        {
            Require(token);
            //A cancellation before the wrapper actually claims must leave a
            //same-size pre-entry reservation reusable, without charging twice.
            if(reserved&&entered==0&&bytes==publicationReserve)return;
            if(reserved||entered!=0||bytes<0||bytes>copyLimit-EnumeratedCopyBytes)
                throw new NotSupportedException("One bounded whole-batch publication reserve required before entry.");
            EnumeratedCopyBytes+=bytes;publicationReserve=bytes;reserved=true;
        }
        internal void ReservePreparationCopy(long bytes,CancellationToken token)
        {
            Require(token);
            if(reserved||entered!=0||bytes<0||bytes>copyLimit-EnumeratedCopyBytes)
                throw new NotSupportedException("Batch TEXT preparation copy exceeds cumulative remaining envelope.");
            EnumeratedCopyBytes+=bytes;
        }
        internal void ReserveHigherMetadata(long bytes,long loops,CancellationToken token)
        {
            Require(token);
            //Both debits precede metadata projection allocations. Exhaustion
            //keeps the consumed preflight reserve; there is no retry refund.
            owner.DebitValidationLoops(loops,token);ReservePreparationCopy(bytes,token);
        }

        //Internal for the containing issuer's C# access only. Construction
        //alone grants nothing: Require checks owner.preparedTextWords==this.
        internal CompletedTextWords(LegacyFreshMainGlobalSourceBatchCompletion owner,CancellationToken token)
        {
            this.owner=owner;owner.RequireCompleted(token);
            int w=Count,u=owner.spans.Length,inputs=owner.scope.Document.ObjectCount;
            if(w is <1 or >4096||u is <1 or >64||inputs is <1 or >256)
                throw new NotSupportedException("Bounded completed original batch required for TEXT queue.");
            long remaining=owner.RemainingEnumeratedCopyBudget(token),charged=0;copyLimit=remaining;
            //Tuple12U +snapshot/ref matrices16U +owner/selected indices8W
            //+input/output queued range-array references16W. CLR metadata/padding and BCL
            //collection backing growth remain separately declared exclusions.
            Charge(checked(28L*u+24L*w));
            var bounds=new (int Rows,int References,int Selections)[u];
            long rows=0,references=0;
            for(int i=0;i<u;i++)
            {
                bounds[i]=owner.publisher.InspectBatchTextSpan(owner.binding,i,token,owner);
                rows=checked(rows+(long)bounds[i].Rows*bounds[i].Selections);
                references=checked(references+(long)bounds[i].References*bounds[i].Selections);
                if(rows>65536||references>65536)
                    throw new NotSupportedException("Whole queued WORD choices/reference uses exceed64Ki before TEXT claim.");
                //CaptureSpan copies rows8A/selections8K/ordinals4K; layout
                //inspection below copies120A. One snapshot per private owner.
                Charge(checked(128L*bounds[i].Rows+12L*bounds[i].Selections));
            }
            TotalChoices=(int)rows;TotalReferences=(int)references;
            long actualOwnerRows=0,actualSelections=0;
            owner.DebitValidationLoops(u,token);
            foreach(var bound in bounds){actualOwnerRows=checked(actualOwnerRows+bound.Rows);actualSelections=checked(actualSelections+bound.Selections);}
            validationLoopBodies=checked(u+actualOwnerRows+actualSelections+w);
            owners=new LegacyNativePlainWordSpanSnapshot[u];
            var originalKeys=new LegacyNativeInkPositionRange[u][];
            //Two selected-input arrays (document copy + map-owned copy),27
            //declared scalar bytes per input; one16B original source interval.
            Charge(checked(54L*inputs+16));
            var document=owner.scope.Document;
            var map=new LegacyNativeTextSourceInputMap(document.CopySelectedInputs(),[],0);
            LegacyNativeInkPositionRange[] original=[new(
                LegacyNativeInkPosition.Pack(0,0,3,document.Strokes[0].Points.Count),
                LegacyNativeInkPosition.Pack((uint)inputs-1,document.Strokes[^1].Points.Count-1,5,document.Strokes[^1].Points.Count))];
            for(int i=0;i<u;i++)
            {
                token.ThrowIfCancellationRequested();
                owner.DebitValidationLoops(checked(1L+owner.binding.PublisherCount+w+bounds[i].Rows+bounds[i].Selections),token);
                var snapshot=owner.publisher.CaptureSpan(owner.binding.Owners[i],owner.binding.Heads,token);
                if(snapshot.Words.Count!=bounds[i].Rows||snapshot.SelectedNodes.Count!=bounds[i].Selections)
                    throw new InvalidOperationException("Same completed275 owner extent changed during TEXT preflight.");
                owners[i]=snapshot;
                foreach(var row in snapshot.Words)
                {
                    byte[] b=row.CopyStorage();
                    if(b.Length!=120||BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(88))!=0||BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(96))!=0||
                        BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(104))!=0||BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(112))!=0)
                        throw new NotSupportedException("Every queued275 row must have ordinary no-auxiliary WORD layout.");
                }
                int sourceCount=owner.spans[i].Worker.BatchTextReferenceCount(owner.spans[i],token);
                long rangeBound=checked((long)sourceCount*inputs);
                if(sourceCount is <1 or >4096||rangeBound>65536)
                    throw new NotSupportedException("Whole actual group0 projection extent exceeds TEXT subset.");
                //Source26 array +ordinary Project result16B/range. Its List
                //growth remains an explicit allocation estimate frontier.
                Charge(checked(26L*sourceCount+16*rangeBound));
                originalKeys[i]=owner.spans[i].Worker.ProjectBatchTextRanges(owner.spans[i],map,original,token);
                if(originalKeys[i].Length>rangeBound)throw new InvalidOperationException("Actual group0 range projection exceeded proven extent.");
            }
            ownerIndices=new int[w];selected=new int[w];
            var queue=new LegacyNativeInkPositionRange[w][];
            for(int i=0;i<w;i++)
            {
                token.ThrowIfCancellationRequested();var node=owner.binding.Nodes[i];int found=-1;
                for(int j=0;j<u;j++)if(owners[j].Span==node.Span){found=j;break;}
                if(found<0||(uint)node.WordIndex>=owners[found].Words.Count||!ReferenceEquals(node.Word,owners[found].Words[node.WordIndex]))
                    throw new InvalidOperationException("Every actual279 must retain its ranked275 owner and selected index.");
                ownerIndices[i]=found;selected[i]=node.WordIndex;queue[i]=originalKeys[found];
            }
            //No source/WORD mutation has occurred. All native WORD lookahead
            //runs over private keys before the future ONE publication reserve.
            long pairBound=checked((long)w*(w-1)/2);
            if(pairBound>owner.maximumWork-owner.PreflightStructuralWork)
                throw new NotSupportedException("WORD queue pair extent exceeds remaining structural-work envelope before lookahead.");
            var lookahead=LegacyPreparedMainWordRangeLookahead.Run(LegacyOwnedRawInkRangeOperations.Prepare(document),queue,
                pairBound,remaining-charged,token);
            keys=lookahead.Keys;Charge(lookahead.CopiedRangePayload);LookaheadPairs=lookahead.Pairs;
            //Equal compares two endpoints per side. Summing (a+b) across all
            //pairs gives (W-1)*sum lengths, so2*(W-1)*sum bounds endpoint reads
            //even if every equal-length pair differs only in its last row.
            //This is an explicit loop/extent bound, not the still-open full
            //transitive TEXT helper/BCL source-operation certificate.
            long totalKeyRows=0;foreach(var key in keys)totalKeyRows=checked(totalKeyRows+key.Length);
            EqualityEndpointReadBound=checked(pairBound+2L*(w-1)*totalKeyRows);
            if(EqualityEndpointReadBound>owner.maximumWork-owner.PreflightStructuralWork)
                throw new NotSupportedException("WORD key equality extent exceeds remaining structural-work envelope before pair scans.");
            for(int i=0;i<keys.Length;i++)
            {
                if(keys[i].Length==0)throw new NotSupportedException("Batch TEXT subset requires nonempty WORD keys after native lookahead.");
                for(int j=0;j<i;j++)if(LegacyOwnedRawInkRangeOperations.Equal(keys[i],keys[j]))
                    throw new NotSupportedException("Shared processed-WORD payload reconciliation requires a separate owner path.");
            }
            EnumeratedCopyBytes=charged;
            void Charge(long bytes)
            {
                if(bytes<0||bytes>remaining-charged)throw new NotSupportedException("Completed-batch TEXT preflight exceeds remaining enumerated-copy budget.");
                charged+=bytes;
            }
        }

        internal void Require(CancellationToken token)
        {
            owner.RequireCompleted(token);
            if(!ReferenceEquals(owner.preparedTextWords,this)||poisoned||thread!=Environment.CurrentManagedThreadId)
                throw new InvalidOperationException("Same healthy completed-batch TEXT queue/thread required.");
            owner.DebitValidationLoops(validationLoopBodies,token);
            for(int i=0;i<owners.Length;i++)owner.publisher.RequireLiveSpanRows(owners[i],token);
            for(int i=0;i<Count;i++)
            {
                var node=owner.binding.Nodes[i];var snapshot=owners[ownerIndices[i]];
                if(node.WordIndex!=selected[i]||!ReferenceEquals(node.Word,snapshot.Words[selected[i]]))
                    throw new InvalidOperationException("Queued actual279 selected index changed after TEXT preflight.");
            }
        }
        internal void DebitSnapshotValidation(LegacyNativePlainWordSpanSnapshot snapshot,CancellationToken token)
            =>owner.DebitValidationLoops(checked((long)snapshot.Words.Count+snapshot.SelectedNodes.Count),token);
        internal void Begin(CancellationToken token)
        {
            Require(token);
            if(!reserved)throw new InvalidOperationException("Whole-batch publication must reserve before queue entry.");
            if(Interlocked.CompareExchange(ref entered,1,0)!=0)throw new InvalidOperationException("Completed-batch TEXT queue cannot restart.");
        }
        internal WordCursor Open(CancellationToken token)
        {
            Require(token);
            if(entered!=1||finished||current is not null||next>=Count)throw new InvalidOperationException("Next actual batch WORD cursor required.");
            return current=new(this,next);
        }
        internal void Close(WordCursor actual,CancellationToken token)
        {RequireCursor(actual,token);current=null;next++;}
        internal void Finish(CancellationToken token)
        {
            Require(token);
            if(entered!=1||finished||current is not null||next!=Count)throw new InvalidOperationException("All actual batch WORDs must finish before one TEXT registration.");
            finished=true;
        }
        internal void Fault()
        {
            //Publication failure poisons this queue only. The genuinely
            //completed augmentation/source remains completed and is not reset.
            if(thread!=Environment.CurrentManagedThreadId||!reserved)throw new InvalidOperationException("Reserved same-thread TEXT queue required to fault.");
            poisoned=true;
        }
        private void RequireCursor(WordCursor actual,CancellationToken token)
        {
            Require(token);
            if(entered!=1||finished||!ReferenceEquals(current,actual)||!ReferenceEquals(actual.Owner,this)||actual.Ordinal!=next)
                throw new InvalidOperationException("Current private actual batch WORD cursor required.");
        }
        private void ChargeExport(int rows)
        {
            long bytes=checked(16L*rows);
            if(bytes>copyLimit-EnumeratedCopyBytes)throw new NotSupportedException("Queued WORD range copy exceeds remaining cumulative TEXT envelope.");
            EnumeratedCopyBytes+=bytes;
        }
        internal sealed class WordCursor
        {
            internal CompletedTextWords Owner{get;}
            internal int Ordinal{get;}
            internal WordCursor(CompletedTextWords owner,int ordinal){Owner=owner;Ordinal=ordinal;}
            internal LegacyNativePublishedWordNode ActualNode(CancellationToken token)
            {Owner.RequireCursor(this,token);return Owner.owner.binding.Nodes[Ordinal];}
            internal int SelectedIndex(CancellationToken token)
            {Owner.RequireCursor(this,token);return Owner.selected[Ordinal];}
            internal int ChoiceCount(CancellationToken token)
            {Owner.RequireCursor(this,token);return Owner.owners[Owner.ownerIndices[Ordinal]].Words.Count;}
            internal LegacyNativePlainWordRecord ActualChoice(int index,CancellationToken token)
            {
                Owner.RequireCursor(this,token);var rows=Owner.owners[Owner.ownerIndices[Ordinal]].Words;
                if((uint)index>=rows.Count)throw new ArgumentOutOfRangeException(nameof(index));return rows[index];
            }
            internal LegacyNativeInkPositionRange[] CopyOriginalKey(CancellationToken token)
            {Owner.RequireCursor(this,token);Owner.ChargeExport(Owner.keys[Ordinal].Length);return Owner.keys[Ordinal].ToArray();}
        }
    }
}
