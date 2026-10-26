using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("LegacyMainGlobalLivePublication.Probe")]
namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalCaseSession
{
    internal sealed class LiveQueryLease(LegacyFreshMainGlobalCaseSession owner, long sourceEpoch,
        LegacyOwnedGlobalMainWordSpan span, LegacyPreparedCaseDictionarySession.LivePublicationQueryLease dictionaryLease)
    {
        internal readonly LegacyFreshMainGlobalCaseSession Owner = owner;
        internal readonly long SourceEpoch = sourceEpoch;
        internal readonly LegacyOwnedGlobalMainWordSpan Span = span;
        internal readonly LegacyPreparedCaseDictionarySession.LivePublicationQueryLease DictionaryLease = dictionaryLease;
    }
    private LiveQueryLease? activeLiveQueries;
    internal readonly record struct LiveRead(ulong Tag, int Node, int Skip, int Capacity, uint Parameter1c,
        uint Parameter20, ushort Flags24, ushort Flags26, byte Category18, byte Parameter19,
        ulong ParentContext, bool Auxiliary, int ReturnedCount);
    internal List<LiveRead>? LiveReadLog { get; set; }

    internal LiveQueryLease CaptureLiveQueryLease(LegacyOwnedGlobalMainWordSpan span, CancellationToken token)
    {
        Enter(token);
        try
        {
            Check(span, token);
            if (span.EndingNode != lastCompleted + 1 || nextSpan != GetSpanExtent(span.EndingNode) ||
                span.Span != nextSpan - 1)
                throw new InvalidOperationException("Global LIVE publication requires the completed node's actual last D0 span before V80.");
            return activeLiveQueries = new(this, epoch, span,
                dictionary.CaptureLivePublicationQueryLease(span.Map, span.EndingNode, token));
        }
        finally { Exit(); }
    }
    internal void ValidateLiveQueryLease(LiveQueryLease lease, CancellationToken token)
    { Enter(token); try { CheckLiveQueryLease(lease, token); } finally { Exit(); } }
    internal LegacyAggregateDictionaryQueryPage ReadLiveQuery(LiveQueryLease lease, ulong tag,
        LegacyPreparedDictionaryQuery query, bool auxiliary, CancellationToken token)
    {
        Enter(token);
        try
        {
            CheckLiveQueryLease(lease, token);
            ArgumentNullException.ThrowIfNull(query);
            if (query.ContextRoot != Contexts.ContainerIdentity)
                throw new ArgumentException("Actual global LIVE context container required.");
            if (query.ParentContext != 0) _ = Contexts.ReadNode(query.ParentContext);
            var before = new LiveRead(tag, query.Node, query.Skip, query.Capacity, query.Parameter1c, query.Parameter20,
                query.Flags24, query.Flags26, query.Category18, query.Parameter19, query.ParentContext, auxiliary, 0);
            var page = dictionary.ReadLivePublicationQuery(lease.DictionaryLease, tag, query, auxiliary, token);
            LiveReadLog?.Add(before with { ReturnedCount = page.ReturnedCount });
            return page;
        }
        finally { Exit(); }
    }
    internal ReadOnlyMemory<byte> ReadLiveTerminal(LiveQueryLease lease,ulong tag,
        ushort endingSymbol,byte boundaryArgument,CancellationToken token)
    {
        Enter(token);
        try
        {
            CheckLiveQueryLease(lease,token);
            var result=dictionary.ReadLivePublicationTerminal(lease.DictionaryLease,tag,endingSymbol,boundaryArgument,token);
            CheckLiveQueryLease(lease,token);
            return result;
        }
        finally{Exit();}
    }
    internal void EndLiveQueryLease(LiveQueryLease lease, CancellationToken token)
    {
        Enter(token);
        try
        {
            CheckLiveQueryLease(lease, token);
            dictionary.EndLivePublicationQueryLease(lease.DictionaryLease, token);
            activeLiveQueries = null;
        }
        finally { Exit(); }
    }
    private void CheckLiveQueryLease(LiveQueryLease lease, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(lease); token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(lease.Owner, this) || !ReferenceEquals(activeLiveQueries, lease) || lease.SourceEpoch != epoch)
            throw new InvalidOperationException("Actual active global LIVE source lease required.");
        Check(lease.Span, token);
        dictionary.ValidateLivePublicationQueryLease(lease.DictionaryLease, token);
    }
}
