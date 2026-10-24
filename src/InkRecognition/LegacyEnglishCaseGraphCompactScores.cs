using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishGraphCompact.Probe")]
[assembly:InternalsVisibleTo("LegacyEnglishGraphCompact.ActualEntry")]
namespace InkRecognition;

internal sealed partial class LegacyMainCompactGroupScores
{
    private LegacyMainCompactGroupScores(ushort[] ownedSymbols,Func<ushort,CancellationToken,float> readWeight)
    {symbols=ownedSymbols;this.readWeight=readWeight;}

    // Only the same live producer may enter. Neither calibrated confidence nor
    // a caller-created shortlist is required by207CB0's symbol/weight channel.
    internal static LegacyMainCompactGroupScores FromEnglishGraph(
        LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,int group,CancellationToken token)
    {
        stage.RequireCompactPhase();token.ThrowIfCancellationRequested();
        var ledger=stage.Ledger;
        // <=150 candidate12 + cost4 + symbol2 + weight4, frames/headers.
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,4096);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,4096);
        var row=stage.Lease.Pass.SpanSession.Scorer.ReadCachedGraphPublication(stage,group,token);
        var actual=row.CopyCandidates();
        if(actual.Length>150)throw new InvalidDataException("Same bounded English shortlist required.");
        var symbols=new ushort[actual.Length];var costs=new int[actual.Length];
        for(int i=0;i<actual.Length;i++)
        {token.ThrowIfCancellationRequested();symbols[i]=actual[i].Symbol;costs[i]=actual[i].Cost;}
        var weights=LegacyOwnedRussianMainGlyphConfidenceGroup.CalculateCandidateWeights(costs,row.PowerScale,token);
        foreach(float weight in weights)if(!float.IsFinite(weight)||weight<0||weight>1)
            throw new NotSupportedException("Native unstabilized normalized weights leave the admitted finite domain.");
        stage.Owner.ObserveGraphCompactScore(stage,group,row.Span,row.PowerScale,symbols,costs,weights);
        stage.RequireCompactPhase();return new(symbols,Read);
        float Read(ushort symbol,CancellationToken ct)
        {
            stage.RequireCompactPhase();ct.ThrowIfCancellationRequested();
            for(int i=0;i<symbols.Length;i++){ct.ThrowIfCancellationRequested();if(symbols[i]==symbol)return weights[i];}
            return 0;
        }
    }
}

public sealed partial class LegacyEnglishFreshCaseMainScorer
{
    internal LegacyEnglishMainArcScores ReadCachedGraphPublication(
        LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,int group,CancellationToken token)
    {
        stage.RequireCompactPhase();token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(stage.Lease.Pass.SpanSession.Scorer,this)||
            !ReferenceEquals(stage.Lease.Root,CaseSource)||(uint)group>=(uint)cache.Length||
            cache[group]is not{} existing||existing.GroupId!=group||existing.Span!=ink.Spans[group])
            throw new InvalidOperationException("Only the already computed same live English group can publish; no NN replay.");
        bool found=false;
        var graph=stage.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph;
        for(int node=0;node<=stage.CurrentNode;node++)foreach(var arc in graph.GetNode(node))
        {token.ThrowIfCancellationRequested();if(arc.GroupId==group)found=true;}
        if(!found)throw new InvalidDataException("Cached group has no actual completed source graph arc.");
        stage.RequireCompactPhase();return existing;
    }
}
