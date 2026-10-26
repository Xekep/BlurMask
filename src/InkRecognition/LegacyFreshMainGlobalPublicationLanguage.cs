using System.Buffers.Binary;
namespace InkRecognition;

//One actual207CB0 node's LANGUAGE V58 scratch. FIRST/LATER204290
//evaluations remain in their existing producer; this owner adds no beam query.
//Closed LIVE/result union: raw bytes and callbacks cannot issue source leases.
internal sealed class LegacyFreshMainGlobalPublicationLanguage
{
    internal const int NativeTemporarySlots=11,NativeTemporaryPayloadBytes=11*40;
    private readonly LegacyFreshMainGlobalWordControllerPolicy policy;
    private readonly LegacyFreshMainGlobalCaseSession.LiveQueryLease? live;
    private readonly LegacyFreshMainGlobalWordPassResult? result;
    private readonly byte[] temporaryHeap;
    private readonly int maximumEvaluations,publicationNode;
    private int evaluations,active;
    private bool valid=true;
    internal int EvaluationCount=>evaluations;
    internal int RetainedPayloadBytes=>temporaryHeap.Length;
    private LegacyFreshMainGlobalPublicationLanguage(LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyFreshMainGlobalCaseSession.LiveQueryLease? live,LegacyFreshMainGlobalWordPassResult? result,
        int publicationNode,ReadOnlySpan<byte> temporaryHeapResidue440,int maximumEvaluations,int maximumRetainedPayloadBytes)
    {
        if(temporaryHeapResidue440.Length!=NativeTemporaryPayloadBytes||maximumRetainedPayloadBytes<NativeTemporaryPayloadBytes||
            maximumEvaluations is <1 or >1_000_000||(live is null)==(result is null)||
            publicationNode<0||publicationNode>=policy.Owner.Graph.State.NodeCount||
            (live is not null&&publicationNode>live.Span.EndingNode))
            throw new ArgumentException("Explicit native11-slot temporary40 allocator residue and bounded invocation required.");
        this.policy=policy;this.live=live;this.result=result;this.maximumEvaluations=maximumEvaluations;this.publicationNode=publicationNode;
        temporaryHeap=temporaryHeapResidue440.ToArray();
        //207CB0 initializes only heap sentinel cost. Slot0's other36 bytes
        //and every untouched field in the actual slot1 remain caller residue.
        BinaryPrimitives.WriteInt32LittleEndian(temporaryHeap,0x4000000);
    }
    internal static LegacyFreshMainGlobalPublicationLanguage FromLive(
        LegacyFreshMainGlobalWordControllerPolicy policy,LegacyFreshMainGlobalCaseSession.LiveQueryLease lease,int publicationNode,
        ReadOnlySpan<byte> temporaryHeapResidue440,int maximumEvaluations,int maximumRetainedPayloadBytes,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(policy);ArgumentNullException.ThrowIfNull(lease);
        _=LegacyFreshMainGlobalTerminalInvocation.Prepare(policy,token);
        policy.Owner.ValidateLiveQueryLease(lease,token);
        return new(policy,lease,null,publicationNode,temporaryHeapResidue440,maximumEvaluations,maximumRetainedPayloadBytes);
    }
    internal static LegacyFreshMainGlobalPublicationLanguage FromResult(LegacyFreshMainGlobalWordPassResult pass,int publicationNode,
        ReadOnlySpan<byte> temporaryHeapResidue440,int maximumEvaluations,int maximumRetainedPayloadBytes,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(pass);_=LegacyFreshMainGlobalTerminalInvocation.Prepare(pass.Policy,token);
        pass.ValidateResultQueryLease(token);
        return new(pass.Policy,null,pass,publicationNode,temporaryHeapResidue440,maximumEvaluations,maximumRetainedPayloadBytes);
    }
    internal bool IsFor(LegacyFreshMainGlobalWordControllerPolicy actualPolicy)=>ReferenceEquals(policy,actualPolicy);
    private void Validate(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(!valid||!policy.Owner.Symbols.LanguageRecords.IsForModel(policy.Owner.Model))
            throw new InvalidOperationException("Failed/foreign publication language owner.");
        if(live is not null)policy.Owner.ValidateLiveQueryLease(live,token);
        else result!.ValidateResultQueryLease(token);
    }
    internal LegacyNativeKnownWordLanguageRecordResult EvaluateTerminal(int actualPublicationNode,ulong tag,ushort endingSymbol,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Publication language scratch must be serialized.");
        try
        {
            Validate(token);
            if(actualPublicationNode!=publicationNode)throw new ArgumentException("This temporary heap belongs to its actual207CB0 node.");
            if(evaluations>=maximumEvaluations)throw new NotSupportedException("Explicit publication language evaluation budget exhausted.");
            if((byte)(tag>>32)!=1||(tag&(1UL<<41))==0||(tag&(1UL<<56))!=0)
                throw new NotSupportedException("Actual ordinary terminal source1 with tag56 clear required.");
            var sourceToken=live is not null?policy.Owner.ReadLiveTerminal(live,tag,endingSymbol,0,token):
                result!.ReadResultTerminal(tag,endingSymbol,0,token);
            //207CB0 clears request0..17 and installs full source70 at18.
            //Its request20 category is therefore token+8, not beam geometry.
            int category=BinaryPrimitives.ReadInt32LittleEndian(sourceToken.Span[8..]);
            evaluations++;
            var value=policy.Owner.Symbols.LanguageRecords.Evaluate(sourceToken.Span,[],category,false,
                0x4000000,temporaryHeap.AsSpan(40,40),token);
            Validate(token);
            //Checked flags3/no-map supplies one source ordinal per V58. Each
            //request resets count0, so its sole accepted row uses native slot1.
            //Preserve the complete returned40 as residue for that slot's reuse.
            if(value.Accepted)value.Record40.Span.CopyTo(temporaryHeap.AsSpan(40,40));
            return value;
        }
        catch{valid=false;throw;}
        finally{Volatile.Write(ref active,0);}
    }
}
