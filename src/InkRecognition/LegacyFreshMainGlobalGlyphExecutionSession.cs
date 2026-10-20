namespace InkRecognition;

/// <summary>Actual global21FE30: derive packet from real initial WORD120,
/// write it before217DB0, execute218560, publish positive symbol spans, reset
/// WORD and acknowledge only the actual typed callback result.</summary>
public sealed class LegacyFreshMainGlobalGlyphExecutionSession
{
    private readonly LegacyNativeWordBuilder word;
    private readonly byte[] initialWord;
    private readonly LegacyFreshMainGlobalOrdinaryGlyphProducer ordinary;
    private readonly LegacyFreshMainGlobalProtocolGlyphProducer protocol;
    private readonly object gate=new();
    private int next,active;
    private bool stopped;
    internal LegacyFreshMainGlobalOrdinaryGlyphProducer OrdinaryProducer=>ordinary;
    public LegacyFreshMainGlobalPlainResultOwnership Ownership{get;}
    public LegacyFreshMainGlobalGlyphAssociationSession Associations{get;}
    public LegacyFreshMainGlobalSourceWidthStatistics Width{get;}
    public bool IsStopped{get{lock(gate)return stopped||Associations.IsStopped;}}
    public bool IsComplete{get{lock(gate)return !IsStopped&&Associations.IsComplete;}}
    public int CompletedFlushCount{get{lock(gate)return Associations.CompletedFlushCount;}}
    private LegacyFreshMainGlobalGlyphExecutionSession(LegacyFreshMainGlobalPlainResultOwnership ownership,
        LegacyFreshMainGlobalGlyphAssociationSession associations,LegacyFreshMainGlobalSourceWidthStatistics width,
        LegacyNativeWordBuilder word,LegacyFreshMainGlobalOrdinaryGlyphProducer ordinary,LegacyFreshMainGlobalProtocolGlyphProducer protocol,byte[] initial)
    {Ownership=ownership;Associations=associations;Width=width;this.word=word;this.ordinary=ordinary;this.protocol=protocol;initialWord=initial;}
    public static LegacyFreshMainGlobalGlyphExecutionSession Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalPlainResultOwnership ownership,LegacyFreshMainGlobalGlyphFlushPlan plan,
        ReadOnlySpan<byte> emptyWordAllocatorResidue120,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(ownership);ArgumentNullException.ThrowIfNull(plan);token.ThrowIfCancellationRequested();
        if(!plan.BacktrackRows.CompactOwner.IsSameSelection(ownership.CompactOwner)||
            !ReferenceEquals(plan.BacktrackRows.SourceCallbacks.Geometry,ownership.Geometry))
            throw new ArgumentException("One actual global compact/flush/source/pool owner required.");
        var associations=LegacyFreshMainGlobalGlyphAssociationSession.Prepare(plan,ownership.Geometry,token);
        var word=ownership.CreateWorkingWord(emptyWordAllocatorResidue120,token);
        var ordinary=LegacyFreshMainGlobalOrdinaryGlyphProducer.Prepare(resource,plan.BacktrackRows.SourceCallbacks,token);
        var protocol=LegacyFreshMainGlobalProtocolGlyphProducer.Prepare(resource,plan.BacktrackRows.SourceCallbacks,token);
        var width=LegacyFreshMainGlobalSourceWidthStatistics.Prepare(plan,ownership.Geometry,token);
        token.ThrowIfCancellationRequested();return new(ownership,associations,width,word,ordinary,protocol,word.CopyStorage(token));
    }
    public byte[] CopyInitialWordStorage()=>initialWord.ToArray();
    public byte[] CopyWorkingWordStorage(CancellationToken token=default){lock(gate)return word.CopyStorage(token);}
    public LegacyFreshMainGlobalGlyphExecutionResult ExecuteNext(ReadOnlySpan<byte> allocatedRecordResidue40,
        ReadOnlySpan<ushort> labelStackResidue256,ReadOnlySpan<byte> requestStackResidue64,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(allocatedRecordResidue40.Length!=40||labelStackResidue256.Length!=256||requestStackResidue64.Length!=64)
            throw new ArgumentException("Explicit complete glyph/label/request allocator storage required.");
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Global glyph execution must be serialized.");
        try
        {
            lock(gate)
            {
                if(IsStopped||next!=Associations.CompletedFlushCount||next>=Associations.Plan.Flushes.Count)
                    throw new InvalidOperationException("Stopped/completed/interfered global glyph execution cannot advance.");
                var pass=Ownership.CompactOwner.Pass;pass.ValidateResultQueryLease(token);
                var flush=Associations.Plan.Flushes[next];byte[] packetBytes=flush.CopyPacketWriteTemplate();var origins=flush.CopyInitialWordSeedOrigins();
                if(packetBytes.Length!=64||origins.Length!=64)throw new InvalidDataException("Actual complete global callback packet required.");
                for(int i=0;i<64;i++)
                {
                    if(origins[i]==-1)continue;
                    if((uint)origins[i]>=120)throw new InvalidDataException("Actual packet origin escapes the initial WORD120.");
                    packetBytes[i]=initialWord[origins[i]];
                }
                var packet=new LegacyPreparedMainWordPublicationPacket(packetBytes);
                byte[] recordResidue=allocatedRecordResidue40.ToArray(),requestResidue=requestStackResidue64.ToArray();
                ushort[] labelResidue=labelStackResidue256.ToArray();
                LegacyFreshMainGlobalAssociatedGlyphFlush? association=null;
                //After the first write, failure is a retained single-use prefix.
                stopped=true;
                try
                {
                    word.ApplyMainPublicationPacket(packet,token);
                    association=Associations.PrepareNext(token);Associations.ValidateCurrent(association,token);
                    if(!ReferenceEquals(association.Flush,flush)||association.Ordinal!=next)throw new InvalidDataException("Actual flush chronology changed during execution.");
                    LegacyPreparedGlyphChainResult? chain=null;
                    bool ExecuteChain()
                    {
                        if(!association.Succeeded)return false;
                        chain=LegacyFreshMainGlobalGlyphChain.Apply(Ownership,word,association,ordinary,protocol,
                            packet.PreviousSymbol,Width.TopAcBits,Width.TopB0Bits,recordResidue,labelResidue,requestResidue,token);
                        return chain.Value.Succeeded;
                    }
                    var outcome=LegacyMainPlainWordCallbackKernel.Execute(word,ExecuteChain,
                        ()=>Ownership.WordPublisher.Publish(word,First,Last,packet.FirstBoundaryOverride,packet.LastBoundaryOverride,token),token);
                    var result=new LegacyFreshMainGlobalGlyphExecutionResult(this,association,packet,chain,outcome.Span,outcome.Succeeded,outcome.Published,outcome.CompletedWord);
                    pass.ValidateResultQueryLease(token);Associations.CompleteExecuted(result,token);
                    if(outcome.Succeeded){next++;stopped=false;}return result;
                }
                catch{Associations.AbortExecution(association);throw;}
            }
        }
        finally{Volatile.Write(ref active,0);}
    }
    private LegacyNativeResidualSource Base(int node,ushort arc)
        =>LegacyNativeResidualSource.Decode(Associations.SourceOverlay.ResolveBaseReference(((uint)node&0xffffffUL)|((ulong)arc<<24)));
    private int First(int node,ushort arc)
    {var source=Base(node,arc);return unchecked((int)Ownership.Geometry.Graph.State.GetNode(source.Node)[source.Arc].StartContextId);}
    private int Last(int node,ushort arc)
    {var source=Base(node,arc);return unchecked((int)Ownership.Geometry.Graph.State.GetNode(source.Node)[source.Arc].EndContextId);}
}

public sealed class LegacyFreshMainGlobalGlyphExecutionResult
{
    private readonly byte[] completedWord;
    public LegacyFreshMainGlobalGlyphExecutionSession ExecutionSession{get;}
    public LegacyFreshMainGlobalAssociatedGlyphFlush Association{get;}
    public LegacyPreparedMainWordPublicationPacket Packet{get;}
    public LegacyPreparedGlyphChainResult? Chain{get;}
    public int SymbolSpan{get;}
    public bool Succeeded{get;}
    public LegacyNativePublishedWordNode? PublishedWord{get;}
    internal LegacyFreshMainGlobalGlyphExecutionResult(LegacyFreshMainGlobalGlyphExecutionSession session,
        LegacyFreshMainGlobalAssociatedGlyphFlush association,LegacyPreparedMainWordPublicationPacket packet,
        LegacyPreparedGlyphChainResult? chain,int span,bool succeeded,LegacyNativePublishedWordNode? published,byte[] completedWord)
    {ExecutionSession=session;Association=association;Packet=packet;Chain=chain;SymbolSpan=span;Succeeded=succeeded;PublishedWord=published;this.completedWord=completedWord;}
    public byte[] CopyCompletedWordStorage120()=>completedWord.ToArray();
}
