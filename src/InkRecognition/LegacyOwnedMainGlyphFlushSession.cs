using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("LegacyMainOwnedFlush.Probe")]

namespace InkRecognition;

/// <summary>One real21FFD0 flush after packet field writes and217DB0.
/// Succeeded describes association only, not the later glyph/publication callback.</summary>
public sealed class LegacyOwnedMainAssociatedGlyphFlush
{
    private readonly LegacyNativeResidualAssociationResult association;
    private int publicationStarted;
    public LegacyOwnedMainGlyphFlushSession Owner {get;}
    public LegacyOwnedMainGlyphFlush Flush {get;}
    public LegacyPreparedMainWordPublicationPacket Packet {get;}
    public int Ordinal {get;}
    public bool Succeeded=>association.Succeeded;
    public int RemainingCount=>association.RemainingCount;
    internal LegacyNativeWordBuilder WorkingWord=>Owner.WorkingWord;
    internal LegacyOwnedMainAssociatedGlyphFlush(LegacyOwnedMainGlyphFlushSession owner,
        LegacyOwnedMainGlyphFlush flush,LegacyPreparedMainWordPublicationPacket packet,
        int ordinal,LegacyNativeResidualAssociationResult association)
    {Owner=owner;Flush=flush;Packet=packet;Ordinal=ordinal;this.association=association;}
    public byte[] CopyRows72()=>association.CopyRows();
    public ulong[] CopyResidualStorage()=>association.CopyResidualStorage();
    internal void BeginPublication(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(Interlocked.CompareExchange(ref publicationStarted,1,0)!=0)
            throw new InvalidOperationException("A flush publication is single-use, including failed prefixes.");
    }
}

/// <summary>Actual CASE flush chronology, initial WORD seed and ordinary
/// per-flush residual ownership. A later typed callback must acknowledge the
/// current association before the next segment can run. No success flag,
/// segment, residual label or result-pool policy is supplied by callers.</summary>
public sealed class LegacyOwnedMainGlyphFlushSession
{
    private readonly byte[] initialWord;
    private readonly object gate=new();
    private LegacyOwnedMainAssociatedGlyphFlush? current;
    private ulong[] residual=[];
    private int remaining,next;
    private bool stopped;
    public LegacyFreshMainPlainResultOwnership Ownership {get;}
    public LegacyOwnedMainGlyphFlushPlan Plan {get;}
    public bool IsStopped {get{lock(gate)return stopped;}}
    public int CompletedFlushCount {get{lock(gate)return next;}}
    public bool IsComplete {get{lock(gate)return !stopped&&current is null&&next==Plan.Flushes.Count;}}
    internal LegacyOwnedRussianMainResidualSource SourceOverlay {get;}
    internal LegacyNativeWordBuilder WorkingWord {get;}
    private LegacyOwnedMainGlyphFlushSession(LegacyFreshMainPlainResultOwnership ownership,
        LegacyOwnedMainGlyphFlushPlan plan,LegacyNativeWordBuilder word,
        LegacyOwnedRussianMainResidualSource overlay,byte[] seed)
    {Ownership=ownership;Plan=plan;WorkingWord=word;SourceOverlay=overlay;initialWord=seed;}

    public static LegacyOwnedMainGlyphFlushSession Prepare(LegacyFreshMainPlainResultOwnership ownership,
        LegacyOwnedMainGlyphFlushPlan plan,ReadOnlySpan<byte> emptyWordAllocatorResidue120,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(ownership);
        if(ownership.SecondaryTerminal is not null)
            throw new NotSupportedException("Configured sessions require PrepareConfigured.");
        return PrepareSelected(ownership,plan,emptyWordAllocatorResidue120,token);
    }

    public static LegacyOwnedMainGlyphFlushSession PrepareConfigured(LegacyFreshMainPlainResultOwnership ownership,
        LegacyOwnedMainGlyphFlushPlan plan,ReadOnlySpan<byte> emptyWordAllocatorResidue120,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(ownership);
        if(ownership.SecondaryTerminal is null)
            throw new ArgumentException("Actual configured selected-terminal publisher ownership required.");
        return PrepareSelected(ownership,plan,emptyWordAllocatorResidue120,token);
    }

    private static LegacyOwnedMainGlyphFlushSession PrepareSelected(LegacyFreshMainPlainResultOwnership ownership,
        LegacyOwnedMainGlyphFlushPlan plan,ReadOnlySpan<byte> emptyWordAllocatorResidue120,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(ownership);ArgumentNullException.ThrowIfNull(plan);
        token.ThrowIfCancellationRequested();var materialization=plan.BacktrackRows.SelectedOwner;
        if(!ReferenceEquals(ownership.SelectedOwner.Identity,materialization.Identity)||
            !ReferenceEquals(ownership.Controller,plan.SourcePolicy.Controller)||
            !ReferenceEquals(ownership.Geometry.Owner,materialization.ConfidenceOwner)||
            !ReferenceEquals(ownership.Geometry.Owner.CaseDictionaryOwner,plan.SourcePolicy.DictionaryOwner))
            throw new ArgumentException("The actual flush plan, result pool, source geometry and CASE policy must have identical ownership.");
        var overlay=LegacyOwnedRussianMainResidualSource.Prepare(ownership.Geometry,plan.SourcePolicy,token);
        var word=ownership.CreateWorkingWord(emptyWordAllocatorResidue120,token);
        byte[] initial=word.CopyStorage(token);
        token.ThrowIfCancellationRequested();return new(ownership,plan,word,overlay,initial);
    }

    /// <summary>Preserves constructor residue provenance, including initial
    /// WORD44..53. Returning a copy cannot alter later packet seed resolution.</summary>
    public byte[] CopyInitialWordStorage()=>initialWord.ToArray();

    internal void ValidateCurrent(LegacyOwnedMainAssociatedGlyphFlush association,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(association);token.ThrowIfCancellationRequested();
        lock(gate)
            if(stopped||!ReferenceEquals(current,association))
                throw new InvalidOperationException("Only this session's current live association can be published.");
    }
    internal void AbortPublication(LegacyOwnedMainAssociatedGlyphFlush association)
    {
        ArgumentNullException.ThrowIfNull(association);
        lock(gate)
        {
            if(!ReferenceEquals(current,association))throw new ArgumentException("Cannot abort a foreign association.");
            stopped=true;
        }
    }

    /// <summary>Resolves the real initial WORD dependencies, applies21FE30
    /// packet fields, then invokes217DB0 over this flush alone. The method
    /// cannot advance beyond an unacknowledged callback. A failure after writes
    /// stops the session and preserves its already committed prefix effects.</summary>
    public LegacyOwnedMainAssociatedGlyphFlush PrepareNext(CancellationToken token=default)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(stopped||current is not null||next>=Plan.Flushes.Count)
                throw new InvalidOperationException("A stopped, completed or awaiting-callback flush session cannot prepare another segment.");
            var flush=Plan.Flushes[next];
            byte[] packet=flush.CopyPacketWriteTemplate();int[] origins=flush.CopyInitialWordSeedOrigins();
            if(packet.Length!=64||origins.Length!=64)throw new InvalidDataException("Owned flush packet extent is invalid.");
            for(int i=0;i<64;i++)
            {
                int source=origins[i];
                if(source==-1)continue;
                if((uint)source>=120)throw new InvalidDataException("Owned flush seed origin escapes its initial WORD.");
                packet[i]=initialWord[source];
            }
            var resolved=new LegacyPreparedMainWordPublicationPacket(packet);
            ulong[] additions=flush.CopyResidualAdditions();
            if(remaining<0||remaining>residual.Length||additions.Length>65536-remaining)
                throw new NotSupportedException("Residual prefix exceeds its owned source-vector budget.");
            ulong[] input=new ulong[remaining+additions.Length];
            residual.AsSpan(0,remaining).CopyTo(input);additions.CopyTo(input,remaining);
            //Entering mutation is single-use even if cancellation/unsupported
            //geometry interrupts after a packet store or higher-source merge.
            stopped=true;
            WorkingWord.ApplyMainPublicationPacket(resolved,token);
            var result=Associate(flush.CopyRows72(),input,token);
            residual=result.CopyResidualStorage();remaining=result.RemainingCount;
            current=new(this,flush,resolved,next,result);
            token.ThrowIfCancellationRequested();stopped=false;return current;
        }
    }

    //Explicit PREPARED references exercise the real geometry/overlay bindings
    //in independent controls. They never enter a public actual-trace session.
    internal LegacyNativeResidualAssociationResult AssociatePreparedForControls(ReadOnlySpan<byte> rows72,
        ReadOnlySpan<ulong> references,CancellationToken token=default)
    {
        lock(gate)return Associate(rows72,references,token);
    }
    private LegacyNativeResidualAssociationResult Associate(ReadOnlySpan<byte> rows72,ReadOnlySpan<ulong> references,
        CancellationToken token)
    {
        var geometry=Ownership.Geometry;
        LegacyNativeResidualSource Base(LegacyNativeResidualSource source)=>LegacyNativeResidualSource.Decode(
            SourceOverlay.ResolveBaseReference(Pack(source.Node,source.Arc),token));
        var operations=new LegacyNativeResidualAssociationOperations(
            source=>{var b=Base(source);return geometry.ReadKind(b.Node,b.Arc,token);},
            source=>{var b=Base(source);return geometry.ReadLine(b.Node,b.Arc,token);},
            source=>SourceOverlay.ReadBounds(Pack(source.Node,source.Arc),token),
            line=>geometry.ReadGuide(line,token),symbol=>geometry.ReadAlphabetFlag(symbol,token),
            source=>{var b=Base(source);return geometry.ReadTopMark(b.Node,b.Arc,token);},SourceOverlay.MergeSources);
        return LegacyNativeResidualGlyphAssociation.Apply(rows72,references,false,geometry.CollectionC1,
            checked((ushort)geometry.Owner.Model.AcousticAlphabet.LabelCount),operations,token);
    }

    /// <summary>Only a result created by the real same-association publication
    /// callback can acknowledge public progress. False success cannot be supplied
    /// independently of that typed callback result.</summary>
    public void CompleteCurrent(LegacyOwnedMainWordPublicationResult publication,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(publication);
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(current is null||!ReferenceEquals(publication.Association,current))
                throw new ArgumentException("Publication must acknowledge this exact current association.");
            Finish(publication.Succeeded);
        }
    }

    //Independent headless controls acknowledge an explicitly PREPARED callback
    //outcome. This hook is not a public substitute for the owning executor.
    internal void CompletePrepared(bool callbackSucceeded,CancellationToken token=default)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(stopped||current is null)throw new InvalidOperationException("No live prepared callback is awaiting acknowledgement.");
            if(callbackSucceeded&&!current.Succeeded)
                throw new InvalidOperationException("A failed217DB0 prefix cannot be acknowledged as published.");
            if(callbackSucceeded)WorkingWord.ResetPlain(token);
            Finish(callbackSucceeded);
        }
    }
    private void Finish(bool callbackSucceeded)
    {
        if(stopped||current is null)throw new InvalidOperationException("The current flush is stopped or absent.");
        if(callbackSucceeded&&!current.Succeeded)
            throw new InvalidOperationException("A failed217DB0 prefix cannot be acknowledged as published.");
        if(!callbackSucceeded){stopped=true;return;}
        next++;current=null;
    }
    private static ulong Pack(int node,ushort arc)=>((uint)node&0xffffffUL)|((ulong)arc<<24);
}
