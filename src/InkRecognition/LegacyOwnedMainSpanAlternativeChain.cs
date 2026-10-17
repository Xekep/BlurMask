using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainSpanAlternativeChain.Probe")]

namespace InkRecognition;

/// <summary>The real218560 result over one actual218A30 numeric emission.
/// The working WORD and partial pool prefix are retained. No275 interning,
/// WORD cost update or219580 eligibility is inferred from success.</summary>
public sealed class LegacyOwnedMainSpanAlternativeChainResult
{
    public LegacyOwnedMainSpanAlternativeChain Owner {get;}
    public LegacyOwnedMainSpanAlternativeWalkResult Walk {get;}
    public LegacyNativeWordBuilder Word {get;}
    public LegacyPreparedGlyphChainResult Chain {get;}
    public bool Succeeded=>Chain.Succeeded;
    internal LegacyOwnedMainSpanAlternativeChainResult(LegacyOwnedMainSpanAlternativeChain owner,
        LegacyOwnedMainSpanAlternativeWalkResult walk,LegacyNativeWordBuilder word,LegacyPreparedGlyphChainResult chain)
    {Owner=owner;Walk=walk;Word=word;Chain=chain;}
}

/// <summary>Same-owner218A30 to218560 connection for fresh ordinary subtype0
/// and an empty actual WORD. Native219580 supplies conversion1, forcefalse,
/// previousFFFF and two rawFFFFFFFF TOP scalars. They are not guessed widths.</summary>
public sealed class LegacyOwnedMainSpanAlternativeChain
{
    private sealed class Claim {internal int Entered;}
    private static readonly ConditionalWeakTable<LegacyOwnedMainSpanAlternativeWalkResult,Claim> claims=new();
    public LegacyOwnedMainSpanAlternativeWalk Walker {get;}
    public LegacyFreshMainPlainResultOwnership TopOwner {get;}
    private readonly LegacyOwnedRussianOrdinaryGlyphProducer ordinary;
    private readonly LegacyOwnedRussianProtocolGlyphProducer protocol;
    private LegacyOwnedMainSpanAlternativeChain(ReadOnlySpan<byte> resource,LegacyOwnedMainSpanAlternativeWalk walker,
        LegacyCursiveSeparationTable table,CancellationToken token)
    {
        Walker=walker;var raw=walker.Sources.Candidate.RawResult;var policy=raw.Session.Plan.SourcePolicy;
        TopOwner=raw.Session.Ownership.TopOwner;
        //Create both services from the identical actual confidence/callback
        //owner. No caller-substituted score service or manufactured CASE owner.
        ordinary=LegacyOwnedRussianOrdinaryGlyphProducer.Prepare(resource,policy,walker.Callbacks.Owner,table,token);
        protocol=LegacyOwnedRussianProtocolGlyphProducer.PrepareCase(resource,policy,walker.Callbacks,token);
    }
    public static LegacyOwnedMainSpanAlternativeChain Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedMainSpanAlternativeWalk actualWalker,LegacyCursiveSeparationTable actualTable,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualWalker);ArgumentNullException.ThrowIfNull(actualTable);token.ThrowIfCancellationRequested();
        return new(resource,actualWalker,actualTable,token);
    }
    public LegacyOwnedMainSpanAlternativeChainResult Apply(LegacyOwnedMainSpanAlternativeWalkResult actualEmission,
        LegacyNativeWordBuilder actualEmptyWord,ReadOnlySpan<byte> allocatedRecordResidue40,
        ReadOnlySpan<ushort> labelStackResidue256,ReadOnlySpan<byte> requestStackResidue64,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualEmission);ArgumentNullException.ThrowIfNull(actualEmptyWord);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(actualEmission.Owner,Walker)||!ReferenceEquals(actualEmptyWord.Pool,TopOwner.GlyphPool))
            throw new ArgumentException("Identical actual walk emission and same-TOP working WORD required.");
        if(actualEmptyWord.HeadReference!=ulong.MaxValue||actualEmission.RowCount==0||actualEmission.Status==1)
            throw new NotSupportedException("A nonempty native emission and freshly reset empty WORD are required.");
        if(allocatedRecordResidue40.Length!=40||labelStackResidue256.Length!=256||requestStackResidue64.Length!=64)
            throw new ArgumentException("Complete explicit record, label and request residues required.");
        byte[] word=actualEmptyWord.CopyStorage(token),rows=actualEmission.CopyRows72();
        if(BinaryPrimitives.ReadUInt16LittleEndian(word.AsSpan(48))!=actualEmission.WordFlags)
            throw new ArgumentException("Actual218A30 WORD30 output must already be applied by its owning caller.");
        if(!actualEmission.CopyResidualReferences().AsSpan().IsEmpty)
            throw new NotSupportedException("Nonempty residual217DB0 association needs its actual per-walk owner before this connection.");
        for(int i=0;i<rows.Length;i+=72)
        {
            if(rows[i+60]!=0||unchecked((short)(BinaryPrimitives.ReadUInt64LittleEndian(rows.AsSpan(i+48))>>24))<0)
                throw new NotSupportedException("This connection supports actual base ordinary subtype0; protocol/higher source ownership remains separate.");
        }
        var claim=claims.GetValue(actualEmission,static _=>new());
        if(Interlocked.CompareExchange(ref claim.Entered,1,0)!=0)
            throw new InvalidOperationException("An entered numeric emission cannot be retried after pool writes or failure.");
        //Native217DB0's residual-count-zero guard returns success without
        //touching rows. The existing exact218560 executor then owns all writes.
        var chain=LegacyPreparedOrdinaryGlyphChain.Apply(Walker.Sources.Candidate.RawResult.Session.Ownership.Materialization,
            actualEmptyWord,rows,[],ordinary,protocol,ushort.MaxValue,uint.MaxValue,uint.MaxValue,false,
            allocatedRecordResidue40,labelStackResidue256,requestStackResidue64,conversionArgument:1,token:token);
        return new(this,actualEmission,actualEmptyWord,chain);
    }
}
