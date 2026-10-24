namespace InkRecognition;

public sealed partial class LegacyOrdinaryGraphSourceRanges
{
    internal readonly record struct GraphWordForwardBounds(int Starts,int Depth,int SourcePoints,long Payload,long Copies);
    // Only private owner arrays are read. No data snapshot/fingerprint or
    // caller-supplied chronology is promoted into source188 authority.
    internal GraphWordForwardBounds PreflightGraphWordForward(uint first,uint last,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(first>int.MaxValue||last>int.MaxValue||first>last||(last>>7)>=(uint)seedGraph.NodeCount)
            throw new NotSupportedException("Actual ordinary forward source interval required.");
        int node=(int)(first>>7),depth=checked((int)((last>>7)-(first>>7)+1));int starts=seedGraph.GetNode(node).Count;
        if(depth is<1 or>49||starts is<1 or>128||keys.Length is<1 or>4096)
            throw new NotSupportedException("Bounded source188 graph layers/points required before DFS allocations.");
        int capacity=4;while(capacity<checked(depth*2+2))capacity=checked(capacity*2);
        // Both count/fill passes: frames list, every possible starting DFS
        // Stack's geometric growth/backing+metadata, path Select array,
        // projection list, sort/merge copies and final references array.
        long pass=checked(4096L+starts*(256L+24L*capacity)+depth*1024L);
        return new(starts,depth,keys.Length,checked(pass*2+depth*26L),checked(pass*4+depth*52L));
    }
}
public sealed partial class LegacyEnglish119MainLocalCandidates
{
    internal GraphWordRangeReceipt RetainGraphWordRanges(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
        =>GraphWordRangeReceipt.Issue(this,stage,token);
    internal sealed class GraphWordRangeReceipt
    {
        private readonly LegacyEnglish119MainLocalCandidates owner;
        private readonly LegacyEnglishCaseNode0Completion.GraphLanguageScope stage;
        private readonly LegacyOrdinaryGraphSourceRanges source;
        private GraphWordRangeReceipt(LegacyEnglish119MainLocalCandidates owner,LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,LegacyOrdinaryGraphSourceRanges source)
        {this.owner=owner;this.stage=stage;this.source=source;}
        internal static GraphWordRangeReceipt Issue(LegacyEnglish119MainLocalCandidates owner,LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
        {
            stage.Owner.RequireGraphPublication(stage,token);
            lock(owner.gate)
            {
                token.ThrowIfCancellationRequested();
                if(!owner.complete||owner.faulted||owner.ranges is null||!ReferenceEquals(stage.Lease.Root.Knowledge.Owner,owner)||
                    !ReferenceEquals(stage.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph.Identity,owner.ranges.Graph))
                    throw new InvalidOperationException("Same private completed local source/seed/chronology graph required.");
                return new(owner,stage,owner.ranges);
            }
        }
        internal void Require(CancellationToken token)
        {
            stage.Owner.RequireGraphPublication(stage,token);lock(owner.gate)
            {token.ThrowIfCancellationRequested();if(!owner.complete||owner.faulted||!ReferenceEquals(owner.ranges,source))
                throw new InvalidOperationException("Retained immutable original source range owner changed.");}
        }
        internal LegacyOrdinaryGraphSourceRanges.GraphWordForwardBounds Preflight(int first,int last,CancellationToken token)
        {Require(token);if(first<0||last<first)throw new NotSupportedException("Forward owned span required.");return source.PreflightGraphWordForward((uint)first,(uint)last,token);}
        internal LegacyOrdinaryGraphSourceRanges.GraphWordDiagnosticInputs CopyDiagnosticInputs(CancellationToken token)
        {Require(token);var data=source.CopyGraphWordDiagnosticInputs(token);Require(token);return data;}
        //Direct1AA470 ordinary branch: preserve the same canonical source arc
        //and call normal1A9AE0 mode0. Higher originalLast/overlay is not guessed.
        internal LegacyOrdinaryGraphSourceReference[] ReadOrdinary140(int node,ushort arc,CancellationToken token)
        {
            Require(token);
            var result=source.ResolveCandidateWithResiduals(node,arc,token);
            Require(token);return result;
        }
        internal LegacyOrdinaryGraphSourceReference[] ReadMode2(int first,int last,CancellationToken token)
        {
            Require(token);
            // NEW fresh ordinary GenerateLeftToRight source has no pending208
            // vector/custom input. Native1A9AE0 mode2's only extra pending-tail
            // branch is therefore absent. Mode0 forward scalar kernel is reused
            // ONLY under this private typed producer, never generalized mode2.
            var result=source.ResolveForwardWithResiduals((uint)first,(uint)last,token);
            Require(token);return result;
        }
    }
}
