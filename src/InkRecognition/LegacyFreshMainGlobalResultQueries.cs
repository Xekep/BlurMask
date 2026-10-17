using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainGlobalResult.Probe")]
[assembly:InternalsVisibleTo("LegacyMainGlobalTerminalMetadata.Probe")]
namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalCaseSession
{
    internal sealed class ResultQueryLease(LegacyFreshMainGlobalCaseSession owner,long sourceEpoch,
        LegacyPreparedCaseDictionarySession.ResultQueryLease dictionaryLease)
    {
        internal readonly LegacyFreshMainGlobalCaseSession Owner=owner;
        internal readonly long SourceEpoch=sourceEpoch;
        internal readonly LegacyPreparedCaseDictionarySession.ResultQueryLease DictionaryLease=dictionaryLease;
    }
    private ResultQueryLease? activeResultQueries;

    //Final2144F0 V80 resets slots, retaining the preceding actual50 map.
    //No D0 refresh, per-query reset or replacement backend is introduced here.
    internal ResultQueryLease CaptureResultQueryLease(CancellationToken token)
    {
        Enter(token);
        try
        {
            if(lastCompleted!=graph.NodeCount-1||nextSpan!=0)
                throw new InvalidOperationException("Global result requires all actual nodes and final source V80.");
            var lease=dictionary.CaptureResultQueryLease(token);
            return activeResultQueries=new(this,epoch,lease);
        }
        finally{Exit();}
    }
    internal void ValidateResultQueryLease(ResultQueryLease lease,CancellationToken token)
    {Enter(token);try{CheckResultQueryLease(lease,token);}finally{Exit();}}
    internal ReadOnlyMemory<byte> ReadResultTerminal(ResultQueryLease lease,ulong tag,
        ushort endingSymbol,byte boundaryArgument,CancellationToken token)
    {
        Enter(token);
        try
        {
            CheckResultQueryLease(lease,token);
            return dictionary.ReadResultTerminal(lease.DictionaryLease,tag,endingSymbol,boundaryArgument,token);
        }
        finally{Exit();}
    }
    internal LegacyAggregateDictionaryQueryPage ReadResultQuery(ResultQueryLease lease,ulong tag,
        LegacyPreparedDictionaryQuery query,bool auxiliary,CancellationToken token)
    {
        Enter(token);
        try
        {
            CheckResultQueryLease(lease,token);ArgumentNullException.ThrowIfNull(query);
            if(query.ContextRoot!=Contexts.ContainerIdentity)
                throw new ArgumentException("Actual global result context container required.");
            if(query.ParentContext!=0)_=Contexts.ReadNode(query.ParentContext);
            return dictionary.ReadResultQuery(lease.DictionaryLease,tag,query,auxiliary,token);
        }
        finally{Exit();}
    }
    private void CheckResultQueryLease(ResultQueryLease lease,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(lease);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(lease.Owner,this)||!ReferenceEquals(activeResultQueries,lease)||lease.SourceEpoch!=epoch)
            throw new InvalidOperationException("Actual completed global result source epoch required.");
        dictionary.ValidateResultQueryLease(lease.DictionaryLease,token);
    }
}
