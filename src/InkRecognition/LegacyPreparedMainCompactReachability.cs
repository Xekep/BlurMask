using System.Buffers.Binary;

namespace InkRecognition;

//Shared numeric207CB0 marking/query walk. Only typed publication owners supply
//the actual snapshots, post-V80 reader, context root and epoch validator.
internal static class LegacyPreparedMainCompactReachability
{
    internal sealed class QueryCounters {public int Checks,Calls;}
    internal sealed record Result(LegacyMainDirectWordNodeResult[] Nodes,byte[] SelectedNodes,int PrefixEnd,
        QueryCounters Counters,Func<int,ushort,CancellationToken,bool> LanguageProbe);
    private const int Impossible=0x4000000;
    internal static Result Run(LegacyNativeWordControllerConfiguration configuration,int maximumAtoms,
        int startingNode,int endingNode,Func<int,LegacyMainDirectWordNodeResult> actualSnapshot,
        LegacyNativeDelimiterQuerySource actualReader,ulong contextRoot,Action<CancellationToken> validate,
        bool configuredSecondary,CancellationToken cancellationToken)
    {
        int primaryLimit = configuration.PrimaryArcLimit230a;
        int lastNode = endingNode;
        //222.VA0/151FF0 ->21B.V30/1A75D0=min(node+1, graph8).
        //The current publication source retains actual2490 and fresh2601=0.
        //Earlier compact rows and predecessor indices remain untouched.
        int prefixEnd = lastNode - Math.Min(lastNode + 1, maximumAtoms);
        var snapshots = new LegacyMainDirectWordNodeResult[lastNode + 1];
        var activeNodes = new byte[lastNode + 1];
        for (int node = 0; node < snapshots.Length; node++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snapshot = actualSnapshot(node);
            if (snapshot.Records.Length % 88 != 0 || snapshot.SourceEnd23f0 > snapshot.Records.Length / 88 ||
                snapshot.PrimaryAlternatives.Length != checked(primaryLimit * 2 * 40))
                throw new InvalidDataException("Result rows must retain their full native source/pool extents.");
            byte[] records = snapshot.Records.ToArray(), alternatives = snapshot.PrimaryAlternatives.ToArray();
            byte[] secondary = snapshot.SecondaryAlternatives.ToArray();
            if (configuredSecondary && secondary.Length != checked(records.Length / 88 * 80))
                throw new InvalidDataException("Configured2410 must retain the complete actual88 allocation extent.");
            if (node >= startingNode)
            {
                for (int ordinal = 0; ordinal < snapshot.SourceEnd23f0; ordinal++)
                {
                    records[ordinal * 88 + 6] &= 0xfd;
                    if (configuredSecondary)
                        for (int k = 0; k < 2; k++) secondary[(ordinal * 2 + k) * 40 + 0x1b] &= 0x7f;
                }
                for (int ordinal = 0; ordinal < primaryLimit * 2; ordinal++)
                    alternatives[ordinal * 40 + 0x1b] &= 0x7f;
            }
            snapshots[node] = snapshot with { Records = records, PrimaryAlternatives = alternatives, SecondaryAlternatives = secondary };
            activeNodes[node] = node >= startingNode && node > prefixEnd ? (byte)1 : (byte)0;
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
            for (int ordinal = 0; ordinal < snapshots[node].SourceEnd23f0; ordinal++)
            {
                var row = snapshots[node].Records.Span.Slice(ordinal * 88, 88);
                if (I32(row, 0x10) >= Impossible) continue;
                if (ordinal < primaryLimit && node <= prefixEnd && !AcceptsLanguage(node, row, cancellationToken)) continue;
                if (ordinal < primaryLimit)
                {
                    for (int alternative = 0; alternative < 2; alternative++)
                    {
                        var entry = snapshots[node].PrimaryAlternatives.Span.Slice((ordinal * 2 + alternative) * 40, 40);
                        if (I32(entry, 0) >= Impossible) break;
                        int previousNode = node, previousOrdinal = ordinal, previousAlternative = alternative;
                        while (previousNode >= startingNode && previousOrdinal < primaryLimit)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            byte[] pool = GetAlternativeStorage(previousNode, previousOrdinal, previousAlternative);
                            int at = (previousOrdinal * 2 + previousAlternative) * 40;
                            if ((sbyte)pool[at + 0x1b] < 0) break;
                            pool[at + 0x1b] |= 0x80;
                            if ((pool[at + 0x1f] & 0x20) != 0) break;
                            previousOrdinal = U16(pool.AsSpan(at), 0x18);
                            previousNode = unchecked(previousNode - 1 - (sbyte)pool[at + 0x1a]);
                            previousAlternative = pool[at + 0x1b] & 0x7f;
                        }
                    }
                }
                if (configuredSecondary)
                    LegacyNativeSecondaryChainReachability.Mark(snapshots, activeNodes,
                        startingNode, primaryLimit, node, ordinal, cancellationToken);
                //Packed22E8.low3>1 takes the ordinary secondary branch;
                //Both count1 and count2 fall through to the beam-chain loop2084BB.
                int parentNode = node, parentOrdinal = ordinal;
                while (parentNode >= startingNode)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    byte[] records = GetRecordStorage(parentNode, parentOrdinal);
                    int at = parentOrdinal * 88;
                    if (parentOrdinal < primaryLimit)
                    {
                        if ((records[at + 6] & 2) != 0) break;
                    }
                    else activeNodes[parentNode] = 1;
                    records[at + 6] |= 2;
                    parentOrdinal = U16(records.AsSpan(at), 0x1e);
                    parentNode = unchecked(parentNode - 1 - (sbyte)records[at + 0x20]);
                }
            }
        }
        validate(cancellationToken);
        return new(snapshots, activeNodes, prefixEnd, counters,
            (node, ordinal, token) => AcceptsLanguage(node,
                snapshots[node].Records.Span.Slice(ordinal * 88, 88), token));

        byte[] GetRecordStorage(int node, int ordinal)
        {
            if ((uint)node >= snapshots.Length || (uint)ordinal >= snapshots[node].SourceEnd23f0)
                throw new InvalidDataException("Native predecessor must name an owned source row.");
            //Only private snapshot arrays are exposed to this local algorithm.
            return System.Runtime.InteropServices.MemoryMarshal.TryGetArray(snapshots[node].Records,
                out ArraySegment<byte> storage) ? storage.Array! : throw new InvalidDataException("Owned record storage absent.");
        }
        byte[] GetAlternativeStorage(int node, int ordinal, int alternative)
        {
            if ((uint)node >= snapshots.Length || (uint)ordinal >= primaryLimit || (uint)alternative >= 2)
                throw new InvalidDataException("Native primary predecessor must name its owned alternative slot.");
            return System.Runtime.InteropServices.MemoryMarshal.TryGetArray(snapshots[node].PrimaryAlternatives,
                out ArraySegment<byte> storage) ? storage.Array! : throw new InvalidDataException("Owned alternative storage absent.");
        }
        bool AcceptsLanguage(int node, ReadOnlySpan<byte> row, CancellationToken token)
        {
            validate(token);
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
            while (true)
            {
                var page = reader.Read(request, token);
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

    private static int I32(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadInt32LittleEndian(bytes[at..]);
    private static ushort U16(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
}
