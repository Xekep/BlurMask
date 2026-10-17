using System.Buffers.Binary;
using System.Runtime.CompilerServices;
namespace InkRecognition;

/// <summary>Actual successful218A30/218560 to219580 working-WORD cost transfer.
/// Numeric diagnostic inputs are not promoted to eligible augmentation candidates.</summary>
public static class LegacyOwnedMainSpanAlternativeCosts
{
    private sealed class Claim {internal int Entered;}
    private static readonly ConditionalWeakTable<LegacyOwnedMainSpanAlternativeChainResult,Claim> claims=new();

    public static void Apply(LegacyOwnedMainSpanAlternativeChainResult actualChain,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualChain);token.ThrowIfCancellationRequested();
        var walker=actualChain.Owner.Walker;var sources=walker.Sources;
        var raw=sources.Candidate.RawResult;var top=raw.Session.Ownership.TopOwner;
        if(!actualChain.Succeeded||!ReferenceEquals(actualChain.Walk.Owner,walker)||
            !ReferenceEquals(actualChain.Word.Pool,top.GlyphPool)||
            actualChain.Word.HeadReference!=actualChain.Chain.LastProcessedReference)
            throw new ArgumentException("An actual successful same-TOP empty-WORD chain result required.");
        byte[] before=actualChain.Word.CopyStorage(token);
        if(BinaryPrimitives.ReadUInt16LittleEndian(before.AsSpan(48))!=actualChain.Walk.WordFlags)
            throw new InvalidOperationException("The actual emitted WORD flags have changed.");
        var compact=raw.Session.Ownership.Materialization;
        var costs=LegacyPreparedMainAugmentedWordCosts.Calculate(compact.CopyCompactStorage(),compact.RecordCount,
            actualChain.Walk.StartingRecordIndex,actualChain.Walk.LastDirectParentIndex,
            LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(sources.FirstContext),
            LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(sources.LastContext),
            raw.Session.Plan.SourcePolicy.Model.SourceKind,token);
        token.ThrowIfCancellationRequested();
        if(Interlocked.CompareExchange(ref claims.GetValue(actualChain,static _=>new()).Entered,1,0)!=0)
            throw new InvalidOperationException("This working-WORD cost transfer has already entered.");
        actualChain.Word.SetRawScalars(costs.Score10,BinaryPrimitives.ReadSingleLittleEndian(before.AsSpan(20)),
            costs.Cost18,costs.Cost1C,token);
    }
}
