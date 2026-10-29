using System.Buffers.Binary;
namespace InkRecognition;
internal static partial class LegacyPreparedMainCompactReachability
{
    internal static Result RunPeriodic(LegacyEnglishCaseNode0Completion.GraphPeriodicCompactStore store,
        LegacyNativeWordControllerConfiguration configuration,int maximumAtoms,LegacyNativeDelimiterQuerySource actualReader,
        ulong contextRoot,CancellationToken cancellationToken,int maximumPagesPerProbe)
    {
        store.Require(cancellationToken);int startingNode=0,endingNode=store.NodeCount-1;
        if(configuration.PrimaryArcLimit230a!=125||configuration.SecondaryAlternativeCount2404!=1||maximumPagesPerProbe<1)
            throw new NotSupportedException("Same ordinary125/2/1 publication only.");
        int primaryLimit = configuration.PrimaryArcLimit230a;
        int lastNode = endingNode;
        //222.VA0/151FF0 ->21B.V30/1A75D0=min(node+1, graph8).
        //The current publication source retains actual2490 and fresh2601=0.
        //Earlier compact rows and predecessor indices remain untouched.
        int prefixEnd = lastNode - Math.Min(lastNode + 1, maximumAtoms);
        store.Require(cancellationToken);store.Allocate(lastNode+1+128);store.Allocate(8);store.Copy(8);
        var activeNodes=new byte[lastNode+1];
        for(int node=0;node<=lastNode;node++)
        {
            cancellationToken.ThrowIfCancellationRequested();int end=store.SourceEnd(node);
            if(end is<125 or>156)throw new InvalidDataException("Actual retained source23F0 required.");
            for(int ordinal=0;ordinal<end;ordinal++)store.ClearRowMark(node,ordinal);
            for(int index=0;index<250;index++)store.ClearPrimaryMark(node,index);
            store.Copy(1);activeNodes[node]=node>prefixEnd?(byte)1:(byte)0;
        }

        var reader = actualReader;
        var request = new LegacyNativeDelimiterQuery
        {
            Request = new() { Capacity = 128, ContextRoot = contextRoot }
        };
        var counters = new QueryCounters();
        for (int node = lastNode; node >= startingNode; node--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (activeNodes[node] == 0) continue;
            for (int ordinal = 0; ordinal < store.SourceEnd(node); ordinal++)
            {
                store.Visit(88);var row = store.Rows(node).Slice(ordinal * 88, 88);
                if (I32(row, 0x10) >= Impossible) continue;
                if (ordinal < primaryLimit && node <= prefixEnd && !AcceptsLanguage(node, row, cancellationToken)) continue;
                if (ordinal < primaryLimit)
                {
                    for (int alternative = 0; alternative < 2; alternative++)
                    {
                        var entry = store.Primary(node).Slice((ordinal * 2 + alternative) * 40, 40);
                        if (I32(entry, 0) >= Impossible) break;
                        int previousNode = node, previousOrdinal = ordinal, previousAlternative = alternative;
                        while (previousNode >= startingNode && previousOrdinal < primaryLimit)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            Span<byte> pool = GetAlternativeStorage(previousNode, previousOrdinal, previousAlternative);
                            int at = (previousOrdinal * 2 + previousAlternative) * 40;
                            if ((sbyte)pool[at + 0x1b] < 0) break;
                            store.MarkPrimary(previousNode,previousOrdinal,previousAlternative);
                            if ((pool[at + 0x1f] & 0x20) != 0) break;
                            previousOrdinal = U16(pool[at..], 0x18);
                            previousNode = unchecked(previousNode - 1 - (sbyte)pool[at + 0x1a]);
                            previousAlternative = pool[at + 0x1b] & 0x7f;
                        }
                    }
                }
                //Packed22E8.low3>1 takes the ordinary secondary branch;
                //Both count1 and count2 fall through to the beam-chain loop2084BB.
                int parentNode = node, parentOrdinal = ordinal;
                while (parentNode >= startingNode)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Span<byte> records = GetRecordStorage(parentNode, parentOrdinal);
                    int at = parentOrdinal * 88;
                    if (parentOrdinal < primaryLimit)
                    {
                        if ((records[at + 6] & 2) != 0) break;
                    }
                    else activeNodes[parentNode] = 1;
                    store.MarkRow(parentNode,parentOrdinal);
                    parentOrdinal = U16(records[at..], 0x1e);
                    parentNode = unchecked(parentNode - 1 - (sbyte)records[at + 0x20]);
                }
            }
        }
        store.Require(cancellationToken);
        return new([], activeNodes, prefixEnd, counters,
            (node, ordinal, token) => AcceptsLanguage(node,
                store.Rows(node).Slice(ordinal * 88, 88), token));

        Span<byte> GetRecordStorage(int node,int ordinal)
        {
            store.Visit(88);if((uint)node>=(uint)store.NodeCount||(uint)ordinal>=(uint)store.SourceEnd(node))
                throw new InvalidDataException("Actual retained row predecessor required.");
            return store.Rows(node);
        }
        Span<byte> GetAlternativeStorage(int node,int ordinal,int alternative)
        {
            store.Visit(40);if((uint)node>=(uint)store.NodeCount||(uint)ordinal>=125||(uint)alternative>=2)
                throw new InvalidDataException("Actual retained primary predecessor required.");
            return store.Primary(node);
        }
        bool AcceptsLanguage(int node, ReadOnlySpan<byte> row, CancellationToken token)
        {
            store.Require(token);store.Visit(128);
            counters.Checks++;
            if ((row[6] & 1) != 0 || I32(row, 0x30) != -1) return false;
            if (((row[5] >> 1) & 1) != 0) return true;
            request.SourceTag = BinaryPrimitives.ReadUInt64LittleEndian(row);
            request.Request.Node = unchecked((int)(uint)request.SourceTag);
            request.Request.ParentContext = 0;
            request.Request.Category18 = 0;
            request.Request.Parameter1c = (uint)node;
            request.Request.Parameter20 = unchecked((uint)(int)(sbyte)row[0x20]);
            request.Request.Flags24 = 0x100;
            request.Request.Flags26 = 0; //Fresh ordinary graph kind0: no special input16 flags.
            request.Request.Skip = 0;
            int probePages=0;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if(probePages++==maximumPagesPerProbe)throw new NotSupportedException("Explicit publication query-page bound exceeded.");
                store.Allocate(8192);store.Copy(8192);var page = reader.Read(request, token);
                counters.Calls += page.BackendCalls;
                if (page.ReturnedCount <= 0) return false;
                for (int i = 0; i < page.ReturnedCount; i++)
                {
                    //207780 starts at controller5F: query output48+17,
                    //not the distinct packed tag/extra field at row1F.
                    int kind = page.Records.Span[i * 32 + 0x17] & 7;
                    if (kind is 1 or 2) return true;
                }
                request.Request.Skip = checked(request.Request.Skip + page.ReturnedCount);
            }
        }
    }

}
