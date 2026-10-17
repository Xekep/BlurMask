using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

public sealed class LegacyMainDescriptorGraphAppendStep
{
    public int DescriptorOrdinal {get;}
    public int LineId {get;}
    public LegacyMainGraphAppendState LocalState {get;}
    public LegacyMainGraphAppendResult Append {get;}
    internal LegacyMainDescriptorGraphAppendStep(int descriptor,int line,LegacyMainGraphAppendState local,LegacyMainGraphAppendResult append)
    {DescriptorOrdinal=descriptor;LineId=line;LocalState=local;Append=append;}
}

/// <summary>Actual cut-source adapter for complete inputs whose every ordinary
/// descriptor has one whole-physical atom. This proves a one-node local graph
/// without deferred/local merging or pruning. Its append-stage state is NOT the
/// finished global graph: cross-source1A3B30 and boundary pruning remain absent.</summary>
public sealed class LegacyFreshMainSingleAtomDescriptorAppend
{
    public LegacyFreshMainDescriptorCutSource Source {get;}
    public LegacyMainGraphAppendState State {get;}
    public IReadOnlyList<LegacyMainDescriptorGraphAppendStep> Steps {get;}
    private LegacyFreshMainSingleAtomDescriptorAppend(LegacyFreshMainDescriptorCutSource source,
        LegacyMainGraphAppendState state,LegacyMainDescriptorGraphAppendStep[] steps)
    {Source=source;State=state;Steps=Array.AsReadOnly(steps);}
    public LegacyMainDescriptorCutStep ResolveGroupSource(int groupIndex,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if((uint)groupIndex>=(uint)State.GroupCount)throw new ArgumentOutOfRangeException(nameof(groupIndex));
        int descriptor=State.Groups[groupIndex].Fields.DescriptorOrdinal;
        return Source.Steps.Single(s=>s.DescriptorOrdinal==descriptor&&!s.IsPendingSeparator);
    }

    public static LegacyFreshMainSingleAtomDescriptorAppend Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainDescriptorCutSource source,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);token.ThrowIfCancellationRequested();
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(source.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            source.CategoryOwner.SelectedSourceOffset!=0||source.CategoryOwner.SamplerNodeOffset!=188157||source.GraphOffset!=189379||
            source.SpanProfile.MaximumAtoms!=8||source.GraphConfigurationByte3)
            throw new ArgumentException("Same actual fresh main0 cut source/resource required.");
        //Check the complete owner before constructing any partial append state.
        //Do not filter unsupported real descriptors out of a larger source.
        foreach(var step in source.Steps)
        {
            token.ThrowIfCancellationRequested();if(step.IsPendingSeparator)continue;
            var atoms=step.CopyAtoms();var counts=step.CopyOutputPointCounts();
            if(step.Kind!=0||step.FirstPhysical!=step.LastPhysical||atoms.Length!=1||atoms[0].Range.StrokeIndex!=step.FirstPhysical||
                atoms[0].Range.StartPoint!=0||atoms[0].Range.EndPointExclusive!=counts[step.FirstPhysical]||atoms[0].Marker.Kind!=1)
                throw new NotSupportedException("Every ordinary descriptor must own one whole-physical atom; allocation-time multi-atom metadata is a later producer.");
        }
        var state=LegacyMainGraphAppendState.Empty(source.SpanProfile.MaximumAtoms);var outputs=new List<LegacyMainDescriptorGraphAppendStep>();
        foreach(var step in source.Steps)
        {
            token.ThrowIfCancellationRequested();if(step.IsPendingSeparator)continue;
            var pending=step.CopyPendingDescriptorOrdinals();var contexts=new List<LegacyMainGraphAppendContext>();byte[] prefix=new byte[16];
            if(pending.Length!=0)prefix[0]=1;
            ushort auxiliary=LegacyMainGraphAppendContext.AppendPending(contexts,prefix,Enumerable.Repeat(unchecked((int)0xfffffff9),pending.Length).ToArray());
            var points=step.CopySelectedPreparedPoints();var range=step.CopyAtoms()[0].Range;
            var bounds=new LegacyMainGraphAppendBounds(points.Min(p=>p.X),points.Min(p=>p.Y),points.Max(p=>p.X),points.Max(p=>p.Y));
            var group=new LegacyMainGraphAppendGroup(new(checked((ushort)step.DescriptorOrdinal),ushort.MaxValue,auxiliary,1,1,0,0,0),bounds,[range],[new[]{range}]);
            int cap=source.SpanProfile.MaximumAtoms;var vectors=Enumerable.Range(0,cap).Select(_=>Array.Empty<int>()).ToArray();
            var arc=new LegacyMainGraphAppendArc(0,0,0,4,0,3,auxiliary,0,0,[-1],vectors);
            var rootVectors=Enumerable.Range(0,cap).Select(n=>n==0?new[]{0}:Array.Empty<int>()).ToArray();
            var root=new LegacyMainGraphAppendArc(0,0,0,4,0,0,ushort.MaxValue,0,0,[],rootVectors);
            //19C280 zeroes the root/header fields and sets4/FFFF.1A1850
            //copies group+4 into arc+18, creates span/placement0/flag4 and
            //links root.1A1EF0 assigns its actual start0/end3 contexts.
            var local=LegacyMainGraphAppendState.Create(cap,[group],contexts,[new[]{arc}],root,token);
            var appended=state.Append(local,token);outputs.Add(new(step.DescriptorOrdinal,step.LineId,local,appended));state=appended.State;
        }
        token.ThrowIfCancellationRequested();return new(source,state,outputs.ToArray());
    }
}
