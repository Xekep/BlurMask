using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainGlobalCompactReachability.Probe")]
namespace InkRecognition;

/// <summary>Actual global207CB0 reachability before compact allocation. Uses
/// the producing pass's post-finalV80 query lease and complete logical rows;
/// no old same-source pass, compact IDs or reconstructed text are synthesized.</summary>
public sealed class LegacyFreshMainGlobalCompactReachability
{
    private readonly LegacyPreparedMainCompactReachability.Result prepared;
    private int active;
    public LegacyFreshMainGlobalTerminalResult TerminalResult {get;}
    public int FinalizedPrefixEnd=>prepared.PrefixEnd;
    public int QueryChecks=>prepared.Counters.Checks;
    public int BackendCalls=>prepared.Counters.Calls;
    public int NodeCount=>prepared.Nodes.Length;
    private LegacyFreshMainGlobalCompactReachability(LegacyFreshMainGlobalTerminalResult terminal,
        LegacyPreparedMainCompactReachability.Result prepared)
    {TerminalResult=terminal;this.prepared=prepared;}

    public static LegacyFreshMainGlobalCompactReachability Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalTerminalResult actualTerminal,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualTerminal);token.ThrowIfCancellationRequested();
        var pass=actualTerminal.PassResult;var policy=pass.Policy;var graph=policy.Owner.Graph;var c=policy.Configuration;
        if(!actualTerminal.Winner.HasWinner||!ReferenceEquals(actualTerminal.Invocation.Policy,policy)||
            resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(
                graph.Source.Source.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("A genuine global terminal and its exact selected acoustic resource required.");
        if(c.Mode22f6!=0||c.Trace2368!=0||c.PrimaryAlternativeCount2402!=2||c.SecondaryAlternativeCount2404!=1||
            (c.PackedSource22e8&7)<=1||policy.SecondaryScaleEc!=0||policy.HasAssociatedSource)
            throw new NotSupportedException("Fresh global mode0/2/2/1 ordinary reachability required.");
        if (pass.IntermediatePublicationState is not null)
            throw new NotSupportedException("LIVE global publication requires its owning append path, not one-shot reachability.");
        pass.ValidateResultQueryLease(token);
        var reader=new LegacyNativeDelimiterQuerySource(policy.Owner.Model.AcousticAlphabet,policy.Owner.Contexts,
            (tag,query,cancellation)=>pass.ReadResultQuery(tag,query,false,cancellation),
            LegacyNativeDelimiterQuerySource.ReadV10dCoordinateMode(resource));
        reader.ResetForControllerNode(token);
        var prepared=LegacyPreparedMainCompactReachability.Run(c,graph.State.MaximumAtoms,0,pass.NodeCount-1,
            pass.GetNodeSnapshot,reader,policy.Owner.Contexts.ContainerIdentity,pass.ValidateResultQueryLease,false,token);
        return new(actualTerminal,prepared);
    }
    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int node)
    {
        if((uint)node>=prepared.Nodes.Length)throw new ArgumentOutOfRangeException(nameof(node));
        var value=prepared.Nodes[node];
        return value with{Records=value.Records.ToArray(),PrimaryAlternatives=value.PrimaryAlternatives.ToArray(),
            SecondaryAlternatives=value.SecondaryAlternatives.ToArray()};
    }
    public byte[] CopySelectedNodes()=>prepared.SelectedNodes.ToArray();
    public bool HasLanguageContinuation(int node,ushort primaryOrdinal,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();TerminalResult.PassResult.ValidateResultQueryLease(token);
        int limit=TerminalResult.PassResult.Policy.Configuration.PrimaryArcLimit230a;
        if((uint)node>=prepared.Nodes.Length||primaryOrdinal>=limit||primaryOrdinal>=prepared.Nodes[node].SourceEnd23f0||
            BinaryPrimitives.ReadInt32LittleEndian(prepared.Nodes[node].Records.Span.Slice(primaryOrdinal*88+16))>=0x4000000)
            throw new ArgumentException("A finite primary row of this actual global preparation required.");
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Result queries must be serialized.");
        try{return prepared.LanguageProbe(node,primaryOrdinal,token);}
        finally{Volatile.Write(ref active,0);}
    }
}
