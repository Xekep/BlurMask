using System.Security.Cryptography;

namespace InkRecognition;

public sealed record LegacyMainGlobalGroupSource(LegacyMainDescriptorCutStep Primary,LegacyMainDescriptorCutStep? Secondary);

/// <summary>Owned ordinary19E8B0 append/cross/prune result on the shared main
/// cut domain. Retains both actual descriptor/line owners for cross groups.
/// Native compact38/arc-table materialization and global NN/context sampling
/// remain separate; this is not a multiline recognizer or decoded result.</summary>
public sealed class LegacyFreshMainGlobalOrdinaryState
{
    public LegacyFreshMainDescriptorGraphAppend LocalGraphs {get;}
    public LegacyFreshMainDescriptorCutSource Source=>LocalGraphs.Source;
    public LegacyMainGraphAppendState State {get;}
    internal IReadOnlyList<LegacyMainCrossGraphEvent> Events {get;}
    private LegacyFreshMainGlobalOrdinaryState(LegacyFreshMainDescriptorGraphAppend local,LegacyMainGraphAppendState state,LegacyMainCrossGraphEvent[] events)
    {LocalGraphs=local;State=state;Events=Array.AsReadOnly(events);}
    public LegacyMainGlobalGroupSource ResolveGroupSource(int groupIndex,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if((uint)groupIndex>=(uint)State.GroupCount)throw new ArgumentOutOfRangeException(nameof(groupIndex));
        var f=State.Groups[groupIndex].Fields;
        var primary=Source.Steps.Single(s=>s.DescriptorOrdinal==f.DescriptorOrdinal&&!s.IsPendingSeparator);
        var secondary=f.SecondaryDescriptorOrdinal==ushort.MaxValue?null:Source.Steps.Single(s=>s.DescriptorOrdinal==f.SecondaryDescriptorOrdinal&&!s.IsPendingSeparator);
        return new(primary,secondary);
    }
    public static LegacyFreshMainGlobalOrdinaryState Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainDescriptorGraphAppend local,LegacyCursiveSeparationTable table,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(local);ArgumentNullException.ThrowIfNull(table);token.ThrowIfCancellationRequested();var source=local.Source;
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(source.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            source.GraphOffset!=189379||source.CategoryOwner.SamplerNodeOffset!=188157||source.SpanProfile.MaximumAtoms!=8||resource[189728]!=1)
            throw new ArgumentException("Same actual fresh main graph/resource owner required.");
        //The cut owner excludes external/reused property state. NEW19C970
        //clears1DB within DWORD1D9 and2D3 within DWORD2D0;1A6980 replaces
        //only1D9/1D8/1DC/2D0 bytes. Thus cross construction is enabled and
        //the optional19E320 postpass is absent for this exact fresh scope.
        var guides=source.CopyGuides();var descriptors=source.CopyDescriptors().Select(d=>new LegacyMainCrossDescriptor(d,guides)).ToArray();
        var ink=LegacyCursivePreparedInk.Create(source.CopyStrokes(),token);
        var profile=LegacyOrdinarySamplerGroupPruningProfile.ReadV106Node(resource,188157);
        var state=LegacyMainGraphAppendState.Empty(source.SpanProfile.MaximumAtoms);var events=new List<LegacyMainCrossGraphEvent>();int previous=-1;
        foreach(var step in local.Steps)
        {
            token.ThrowIfCancellationRequested();int oldEnd=state.NodeCount-1;
            state=state.Append(step.LocalState,token).State;
            if(previous!=-1&&previous+step.LocalState.NodeCount-1<state.MaximumAtoms)
            {
                var kernel=new LegacyMainCrossSourceGraph(state,descriptors,ink,profile,table,token);
                kernel.MergeBoundary(oldEnd,previous-1,state.NodeCount-1);state=kernel.Snapshot();events.AddRange(kernel.Events);
            }
            previous=step.LocalState.NodeCount;
        }
        var pruning=new LegacyMainCrossSourceGraph(state,descriptors,ink,profile,table,token);
        pruning.PruneAll(local.Steps.Select(s=>s.LocalState.NodeCount).ToArray());state=pruning.Snapshot();events.AddRange(pruning.Events);
        token.ThrowIfCancellationRequested();return new(local,state,events.ToArray());
    }
}
