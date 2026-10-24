using System.Buffers.Binary;
using System.Security.Cryptography;
namespace InkRecognition;

/// <summary>Owning fresh one-span global219580 ordinary/no-auxiliary branch,
/// followed by actual275 ranking and mass publication. Never an app acceptance,
/// imported byte route or replacement model/query service.</summary>
public sealed class LegacyFreshMainGlobalWordAugmentation
{
    private readonly byte[] compact;
    private readonly int[] starts;
    private readonly LegacyFreshMainGlobalPlainRecognitionResult source;
    private readonly LegacyFreshMainGlobalGlyphExecutionSession session;
    private readonly LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot snapshot;
    private readonly LegacyNativePlainWordPublisher publisher;
    private readonly LegacyNativePublishedWordNode span;
    private readonly LegacyFreshMainGlobalSourceSymbolCallbacks callbacks;
    private readonly LegacyFreshMainGlobalWordControllerPolicy policy;
    private readonly ushort marker,joiner,closed,unit,softHyphen;
    private int entered;
    private bool faulted,complete;
    private int controller24a0;
    private byte[] sourceStorage26=[];
    private LegacyOrdinaryGraphSourceReference[] sourceReferences=[];
    public bool IsComplete=>complete;
    public bool IsFaulted=>faulted;
    private LegacyFreshMainGlobalWordAugmentation(ReadOnlySpan<byte> resource,LegacyFreshMainGlobalPlainRecognitionResult source,
        LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot snapshot,byte[] compact,int[] starts,
        LegacyNativePublishedWordNode span)
    {
        this.source=source;this.snapshot=snapshot;this.compact=compact;this.starts=starts;this.span=span;
        session=source.Session!;publisher=session.Ownership.WordPublisher;
        callbacks=session.OrdinaryProducer.Callbacks;policy=source.Selection.Publication.Policy;
        var alphabet=policy.Owner.Model.AcousticAlphabet;
        ushort Label(string value)=>alphabet.TryGetLabel(value,out ushort label)?label:ushort.MaxValue;
        marker=Label("\u0003");joiner=Label("\u200d");closed=Label("\U000f0001");softHyphen=Label("\u00ad");
        unit=BinaryPrimitives.ReadUInt16LittleEndian(resource[105..]);
    }
    public static LegacyFreshMainGlobalWordAugmentation Prepare(ReadOnlySpan<byte> acousticResource,
        LegacyFreshMainGlobalPlainRecognitionResult actualSource,ReadOnlySpan<byte> alternativeAllocation400,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualSource);token.ThrowIfCancellationRequested();actualSource.Selection.Publication.Validate(token);
        if(!actualSource.HasWinner||!actualSource.PublicationCompleted||actualSource.Session is not{} session||
            !session.IsComplete||session.IsStopped||!ReferenceEquals(session.Ownership.PublishedTerminal,actualSource.Selection)||
            alternativeAllocation400.Length!=400||acousticResource.Length!=572763)
            throw new ArgumentException("One actual successful live global published source and allocation400 required.");
        var policy=actualSource.Selection.Publication.Policy;var cuts=policy.Owner.Graph.Source.Source;
        if(policy.Owner.Model.SourceKind!=0x135||policy.HasAssociatedSource||policy.Configuration.Trace2368!=0||
            policy.Configuration.Mode22f6!=0||policy.Refresh.KnowledgeOption22f8!=0||policy.Refresh.SourceFormat22f9!=0||
            !StringComparer.OrdinalIgnoreCase.Equals(Convert.ToHexString(SHA256.HashData(acousticResource)),cuts.SourceFingerprint))
            throw new NotSupportedException("Exact fresh no-property135/nontracing global ordinary source required.");
        //Mode2 source188's trailing-control branch cannot be replaced withmode0.
        //Admit only the same proven one ordinary/no-pending descriptor scope.
        if(cuts.Steps.Count!=1||cuts.Steps[0].IsPendingSeparator||cuts.Steps[0].Kind!=0||
            policy.Owner.Graph.Source.State.Contexts.Count!=0)
            throw new NotSupportedException("Actual single ordinary descriptor without pending mode2 metadata required.");
        var publisher=session.Ownership.WordPublisher;var span=publisher.Head??throw new NotSupportedException("Actual WORD head required.");
        publisher.RequireSoleActiveSourceHead(span);
        if(actualSource.RawWordsInNativeHeadOrder.Count!=1)throw new NotSupportedException("One completed actual WORD span required.");
        foreach(string value in new[]{"\u2610","\u2611","\u2612"})
            if(policy.Owner.Model.AcousticAlphabet.TryGetLabel(value,out _))throw new NotSupportedException("Native checkbox rewrite needs its own source owner.");
        var current=publisher.CaptureSpan(span,[span],token);
        if(current.Words.Count!=1||current.SelectedOrdinals is not [0])throw new NotSupportedException("Unprocessed singleton275 source required beforeaugmentation.");
        var snapshot=LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(actualSource.Selection,session,actualSource.Publications,alternativeAllocation400,token);
        int count=actualSource.Selection.Publication.RecordCount2470;
        var compact=actualSource.Selection.Publication.CopyCompactStorage().AsSpan(0,checked(count*40)).ToArray();
        var starts=actualSource.Selection.Publication.CopyNodeStarts2478();
        token.ThrowIfCancellationRequested();return new(acousticResource,actualSource,snapshot,compact,starts,span);
    }
    private void Validate(CancellationToken token,bool claimed)
    {
        token.ThrowIfCancellationRequested();source.Selection.Publication.Validate(token);
        if(faulted||complete||!source.PublicationCompleted||!ReferenceEquals(source.Session,session)||!session.IsComplete||session.IsStopped||
            !ReferenceEquals(span,publisher.Head)||policy.Owner.AcousticEvaluations!=source.CachedMainGroupEvaluations)
            throw new InvalidOperationException("Same current completed global source/NN cache/WORD list required.");
        publisher.RequireSoleActiveSourceHead(span);if(claimed)session.RequireSourceCompletion(this,token);
    }
    public Result Run(ReadOnlySpan<byte> wordAllocatorResidue120,ReadOnlySpan<byte> glyphAllocatorResidue40,
        ReadOnlySpan<ushort> labelStackResidue256,ReadOnlySpan<byte> requestResidue64,ReadOnlySpan<byte> walkResidue72,
        Span<byte> leadingScratch4,Span<byte> trailingScratch4,Span<byte> compareLeft4,Span<byte> compareRight4,
        CancellationToken token=default)
    {
        Validate(token,false);
        if(wordAllocatorResidue120.Length!=120||glyphAllocatorResidue40.Length!=40||labelStackResidue256.Length!=256||
            requestResidue64.Length!=64||walkResidue72.Length!=72||leadingScratch4.Length!=4||trailingScratch4.Length!=4||
            compareLeft4.Length!=4||compareRight4.Length!=4||leadingScratch4.Overlaps(trailingScratch4)||compareLeft4.Overlaps(compareRight4))
            throw new ArgumentException("Complete separate native allocator/caller residues required.");
        // Bound complete geometry and cumulative selected-kernel expansion BEFORE
        // entered/Claim/quote writes. This is not a whole source-work/RSS proof.
        int sourcePoints=source.Owner.SourceDocument.Strokes.Sum(stroke=>stroke.Points.Count);
        var envelope=LegacyGlobalWordAugmentationEnvelope.Prepare(compact,starts,policy.Owner.Graph.State.NodeCount,sourcePoints,token,span.Word.FlatReferences.Count,span.Span.Last);
        //CreateEmpty validates/clones private120; it does not mutate GlyphPool.
        //Validate allocation/reset before entering live source mutation.
        var workingWord=LegacyNativeWordBuilder.CreateEmpty(session.Ownership.GlyphPool,wordAllocatorResidue120,token);
        byte[] recordStorage=glyphAllocatorResidue40.ToArray(),requestStorage=requestResidue64.ToArray(),walkStorage=walkResidue72.ToArray();
        ushort[] labelsStorage=labelStackResidue256.ToArray();
        if(Interlocked.CompareExchange(ref entered,1,0)!=0)throw new InvalidOperationException("Entered augmentation cannot retry.");
        session.ClaimSourceCompletion(this,true,token);
        try
        {
            LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(snapshot,leadingScratch4,trailingScratch4,token);
            _=LegacyFreshMainGlobalResultGlyphNormalization.Prepare(policy,token).ApplyPublished(snapshot,token);
            //Native frontend228: null v106 sampler preparation +21D last-group
            //invalidation only; immutable cached group snapshots have no such reuse hint.
            var ranges=policy.Owner.Acoustic.Geometry.Context.Source;
            var counted=ranges.ResolveForwardWithResiduals((uint)span.Span.First,(uint)span.Span.Last,token);
            var filled=ranges.ResolveForwardWithResiduals((uint)span.Span.First,(uint)span.Span.Last,token);
            if(!counted.AsSpan().SequenceEqual(filled))throw new InvalidDataException("Actual frontend188 count/fill changed.");
            envelope.Copy(checked(filled.Length*104L));sourceReferences=filled;sourceStorage26=EncodeSourceStorage26(filled,token);
            Validate(token,true);
            var walker=new LegacyGlobalSpanAlternativeWalkKernel(compact,compact.Length/40,policy.Owner.Graph.State.NodeCount,
                marker,joiner,closed,unit,softHyphen,requestStorage,First,Last,(request,cursor,ct)=>
                {
                    Validate(ct,true);var d=callbacks.ReadDelimiter(request,cursor,ct);Validate(ct,true);
                    if(!ReferenceEquals(d.Owner,callbacks))throw new InvalidOperationException("Actual delimiter callback owner required.");
                    return new(d.RemainingCursor,d.CopySymbols().Length,d.NativeFlag0C,d.NativeFlag0D);
                },envelope);
            var scan=LegacyPreparedMainWordAugmentationCandidates.Scan(compact,compact.Length/40,starts,span.Span.First,span.Span.Last,false,
                First,Last,(index,minimum)=>
                {
                    Validate(token,true);var dry=walker.Run(index,minimum,false,ushort.MaxValue,[],token);controller24a0=0;
                    return new(dry.Status,dry.LastSourceKey);
                },token);
            int originalCount=publisher.CaptureSpan(span,[span],token).Words.Count,appended=0,duplicates=0,updated=0;
            foreach(int candidate in scan.CompactIndices)
            {
                Validate(token,true);
                //Native dictionaryV70 must produce this auxiliary payload. Neverzero/substituteit.
                if(BinaryPrimitives.ReadInt32LittleEndian(compact.AsSpan(candidate*40+36))!=-1)
                    throw new NotSupportedException("Eligible candidate auxiliary dictionaryV70 fiveDWORD issuer not yet connected.");
                workingWord.ResetPlain(token);var word=workingWord;
                word.ApplyAugmentationFlag32((compact[candidate*40+32]&16)!=0,token);
                int minimum=span.Span.First<0?-1:LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(span.Span.First);
                var emission=walker.Run(candidate,minimum,true,65534,walkStorage,token);controller24a0=emission.Rows72.Length/72;
                if(emission.Status==1||emission.Status==-1)throw new InvalidOperationException("Included direct-parent candidate failed actual emission.");
                if(emission.ResidualReferences.Length!=0)throw new NotSupportedException("Actual residual217DB0 augmentation association remains separate.");
                for(int at=0;at<emission.Rows72.Length;at+=72)
                    if(emission.Rows72[at+60]!=0)throw new NotSupportedException("Augmentation currently binds actual ordinary0 writer; protocol/higher source requires its own path.");
                var before=publisher.CaptureSpan(span,[span],token);
                foreach(var old in before.Words)foreach(ulong reference in old.FlatReferences)
                    if((BinaryPrimitives.ReadUInt16LittleEndian(session.Ownership.GlyphPool.CopyRecord(reference,token).AsSpan(4))&7)!=0)
                        throw new NotSupportedException("Original275 equality requires separate nonordinary token owner.");
                word.ApplySpanAlternativeFlags30(emission.WordFlags,token);
                var chain=LegacyMainOrdinaryGlyphChainKernel.Apply(word,emission.Rows72,[],policy.Owner.Graph.State.NodeCount,
                    compact.Length/40,starts,compact,node=>policy.Owner.Graph.State.GetNode(node).Count,session.OrdinaryProducer.Produce,
                    static (_,_,_,_,_,_,_)=>throw new NotSupportedException("No protocol writer is substituted."),
                    ushort.MaxValue,uint.MaxValue,uint.MaxValue,false,recordStorage,labelsStorage,requestStorage,1,token);
                controller24a0=0;if(!chain.Succeeded)throw new InvalidOperationException("Actual augmentation218560 returnedfailure.");
                var costs=LegacyPreparedMainAugmentedWordCosts.Calculate(compact,compact.Length/40,candidate,emission.LastDirectParentIndex,
                    LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(span.Span.First),LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(span.Span.Last),0x135,token);
                byte[] candidateData=word.CopyStorage(token);word.SetRawScalars(costs.Score10,BinaryPrimitives.ReadSingleLittleEndian(candidateData.AsSpan(20)),costs.Cost18,costs.Cost1C,token);
                if(word.GetSourceSpan(First,Last,token)!=span.Span)throw new InvalidDataException("Actual emitted candidate source span changed.");
                before=publisher.CaptureSpan(span,[span],token);int match=-1;
                for(int i=0;i<before.Words.Count;i++)
                    if(Compare(session.Ownership.GlyphPool,before.Words[i].CopyStorage(),word.CopyStorage(token),compareLeft4,compareRight4,joiner,static label=>label,null,token)){match=i;break;}
                if(match==-1){Validate(token,true);_=publisher.InternPlainWord(word,span.Span,out _,token);appended++;}
                else
                {
                    duplicates++;byte[] existing=before.Words[match].CopyStorage();
                    if(match>=originalCount&&(ClassWord(emission.WordFlags,existing)||costs.Cost18<BinaryPrimitives.ReadSingleLittleEndian(existing.AsSpan(24))))
                    {
                        Validate(token,true);publisher.UpdateAugmentedDuplicate(before,match,originalCount,costs.Score10-0.00001f,costs.Cost18,costs.Cost1C,emission.WordFlags,token);updated++;
                    }
                    if(chain.FirstNewReference!=ulong.MaxValue)
                        _=session.Ownership.GlyphPool.RemoveCreatedChainSegment(chain.FirstNewReference,chain.LastProcessedReference,token);
                }
                Validate(token,true);
            }
            var current=publisher.CaptureSpan(span,[span],token);
            //Copy explicit scratch because comparison delegates cannot capture ref-like spans.
            byte[] left=compareLeft4.ToArray(),right=compareRight4.ToArray();
            bool Equal(ReadOnlySpan<byte> a,ReadOnlySpan<byte> b){envelope.Equality();return LegacyNativeMainWordEquality.Compare(session.Ownership.GlyphPool,a,b,left,right,joiner,static label=>label,null,token);}
            var ranked=LegacyPreparedMainWordRanking.Rank(current.Words.Select(w=>w.CopyStorage()).ToArray(),current.SelectedOrdinals.ToArray(),
                LegacyPreparedMainWordRanking.InitialQuota(65535,65535,false),false,Equal,token);
            publisher.ApplySpanRanking(current,ranked.Words,ranked.OutputSourceOrdinals,ranked.OldToNew,token);
            current=publisher.CaptureSpan(span,[span],token);var scored=LegacyPreparedMainSpanScorePublication.Normalize(current.Words.Select(w=>w.CopyStorage()).ToArray(),token);
            publisher.ApplySpanScores(current,scored.Words,token);Validate(token,true);
            var result=new Result(scan.CompactIndices.Length,appended,duplicates,updated,ranked.QuotaAfter,sourceReferences,sourceStorage26,publisher.CaptureSpan(span,[span],token).Words.Select(w=>w.CopyStorage()).ToArray());
            if(controller24a0!=0)throw new InvalidOperationException("Native shared row scratch must be cleared afteraugmentation.");
            complete=true;return result;
        }
        catch{faulted=true;session.FaultSourceCompletion(this);throw;}
        bool Compare(LegacyNativeGlyphRecordPool pool,ReadOnlySpan<byte> a,ReadOnlySpan<byte> b,Span<byte> l,Span<byte> r,ushort ignored,Func<ushort,ushort> mapper,Func<string,ushort>? lookup,CancellationToken ct)
        {envelope.Equality();return LegacyNativeMainWordEquality.Compare(pool,a,b,l,r,ignored,mapper,lookup,ct);}
        int First(int node,ushort arc){envelope.Request();Validate(token,true);return unchecked((int)policy.Owner.Graph.State.GetNode(node)[arc].StartContextId);}
        int Last(int node,ushort arc){envelope.Request();Validate(token,true);return unchecked((int)policy.Owner.Graph.State.GetNode(node)[arc].EndContextId);}
        bool ClassWord(ushort flags,byte[] old)
        {
            if(BinaryPrimitives.ReadUInt16LittleEndian(old.AsSpan(48))!=flags||unchecked((short)flags)<0)return false;
            uint[] headers=policy.Owner.Symbols.CopySourceHeaders();if(flags>=headers.Length)throw new NotSupportedException("Actual WORD source header ordinal required.");
            return(headers[flags]&0x1c)!=0;
        }
    }
    internal static byte[] EncodeSourceStorage26(LegacyOrdinaryGraphSourceReference[] references,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();byte[] data=new byte[checked(references.Length*26)];int tail=checked(references.Length*24);
        for(int i=0;i<references.Length;i++)
        {
            token.ThrowIfCancellationRequested();var r=references[i];Write(data.AsSpan(i*24),r.First);Write(data.AsSpan(i*24+12),r.Last);
            data[tail+i*2]=r.FirstResidual;data[tail+i*2+1]=r.LastResidual;
        }
        return data;
        static void Write(Span<byte> destination,LegacySourcePointKey value)
        {BinaryPrimitives.WriteInt32LittleEndian(destination,value.First);BinaryPrimitives.WriteInt32LittleEndian(destination[4..],value.Second);BinaryPrimitives.WriteInt32LittleEndian(destination[8..],value.Third);}
    }
    //Completed live source remains privately bound for a future actual TEXT scope.
    //Detached Result below never becomes that scope/query authority.
    internal void RequireCompleted(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(!complete||faulted)throw new InvalidOperationException("Successful actual owning augmentation required.");
        session.RequireSourceCompletion(this,token);source.Selection.Publication.Validate(token);publisher.RequireSoleActiveSourceHead(span);
        if(policy.Owner.AcousticEvaluations!=source.CachedMainGroupEvaluations)throw new InvalidOperationException("Completed cache lineage changed.");
    }
    public sealed class Result
    {
        private readonly LegacyOrdinaryGraphSourceReference[] references;private readonly byte[] storage26;private readonly byte[][] words;
        public int Candidates{get;}public int Appended{get;}public int Duplicates{get;}public int UpdatedNewDuplicates{get;}public int QuotaAfter{get;}
        public bool AugmentationWasIneligible=>Candidates==0;
        internal Result(int candidates,int appended,int duplicates,int updated,int quota,LegacyOrdinaryGraphSourceReference[] refs,byte[] storage26,byte[][] words)
        {Candidates=candidates;Appended=appended;Duplicates=duplicates;UpdatedNewDuplicates=updated;QuotaAfter=quota;references=refs;this.storage26=storage26;this.words=words;}
        public byte[] CopySourceStorage26()=>storage26.ToArray();
        public byte[][] CopyRankedWords120()=>words.Select(w=>w.ToArray()).ToArray();
        public LegacyOrdinaryGraphSourceReference[] CopySourceReferences()=>references.ToArray();
        //DATA only. No result owner/query/lease or pre-NN document authority retained.
    }
}
