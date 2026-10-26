using System.Buffers.Binary;
namespace InkRecognition;

public sealed partial class LegacyNativePlainWordPublisher
{
    //219580 modifies fields of an ALREADY ADDED duplicate without creating279.
    // Snapshot is the current private275 list binding, never a payload clone issuer.
    internal void UpdateAugmentedDuplicate(LegacyNativePlainWordSpanSnapshot current,int ordinal,int originalCount,
#if BLURMASK_PUBLICATION_DIAGNOSTICS
        float score10,float cost18,float cost1c,ushort wordFlags30,CancellationToken token,LegacyPublicationScoreTrace? candidateTrace=null)
#else
        float score10,float cost18,float cost1c,ushort wordFlags30,CancellationToken token)
#endif
    {
        token.ThrowIfCancellationRequested();current.BeginApply(this,interning);
        if(released||!interning.Spans.TryGetValue(current.Span,out var words)||!current.RetainsNativeList(words)||
            words.Count!=current.Words.Count||words.Where((w,i)=>!ReferenceEquals(w,current.Words[i])).Any()||
            (uint)ordinal>=words.Count||originalCount<1||originalCount>words.Count||ordinal<originalCount)throw new InvalidOperationException("Current actual same-TOP275 duplicate required.");
        byte[] data=words[ordinal].CopyStorage();
#if BLURMASK_PUBLICATION_DIAGNOSTICS
        if(candidateTrace is not null&&candidateTrace.CandidateAdmitCurrent(pool))candidateTrace.CaptureUpdateBefore(data);
#else
#endif
        BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(16),score10);
        BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(24),cost18);
        BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(28),cost1c);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(48),wordFlags30);
        var replacement=new LegacyNativePlainWordRecord(pool,data,words[ordinal].FlatReferences.ToArray());
        //No selected payload is normally eligible for219580's new-only update.
        //Retain the exact pointer/ordinal binding if the actual list ever names it.
        foreach(var node in current.SelectedNodes)
            if(node.WordIndex==ordinal)node.RebindWord(ordinal,replacement);
        words[ordinal]=replacement;
#if BLURMASK_PUBLICATION_DIAGNOSTICS
        if(candidateTrace is not null&&candidateTrace.CandidateAdmitCurrent(pool))candidateTrace.CaptureUpdateApplied(data);
#else
#endif
    }
    internal void RequireSoleActiveSourceHead(LegacyNativePublishedWordNode actual)
    {
        // Public ForkAlternative can retain arbitrarily many empty/released
        // publishers. Bound this NEW owner validator before enumerating history.
        int count=interning.Publishers.Count;
        if(count is <1 or >4096||released||!ReferenceEquals(Head,actual)||actual.Next is not null)
            throw new NotSupportedException("Bounded actual live global TOP279 source list/WORD span required.");
        int active=0;
        for(int i=0;i<count;i++)
        {
            var publisher=interning.Publishers[i];
            if(!publisher.released&&publisher.Head is not null)active++;
        }
        if(interning.Publishers.Count!=count||active!=1)
            throw new NotSupportedException("One unchanged actual live global TOP279 publisher history required.");
    }
}
