using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LegacyMainSecondaryReachability.Probe")]

namespace InkRecognition;

/// <summary>Actual configured CASE2410 ownership through the marking phase
/// of207CB0. No compact40 index, WORD or273 alternative is manufactured.</summary>
public sealed class LegacyOwnedMainSecondaryCompactReachability
{
    private readonly LegacyOwnedMainCompactWordReachability marked;
    public LegacyOwnedMainCaseWordPassResult PassResult { get; }
    public int FinalizedPrefixEnd => marked.FinalizedPrefixEnd;
    public int QueryChecks => marked.QueryChecks;
    public int BackendCalls => marked.BackendCalls;

    private LegacyOwnedMainSecondaryCompactReachability(LegacyOwnedMainCaseWordPassResult pass,
        LegacyOwnedMainCompactWordReachability marked)
    { PassResult = pass; this.marked = marked; }

    public static LegacyOwnedMainSecondaryCompactReachability Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedMainCaseWordPassResult actualPass, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actualPass);
        cancellationToken.ThrowIfCancellationRequested();
        var policy = actualPass.SourcePolicy;
        if (resource.Length != 572763 || !StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(resource)), policy.Controller.Owner.SelectedSourceOwner.SourceFingerprint))
            throw new ArgumentException("The exact producing selected main resource is required.", nameof(resource));
        if (!ReferenceEquals(policy.Controller, actualPass.Storage.ControllerOwner) ||
            !ReferenceEquals(policy.Graph, policy.Controller.Owner.SourceGraph) ||
            !ReferenceEquals(policy.DictionaryOwner, policy.Controller.Owner.CaseDictionaryOwner) ||
            actualPass.NodeCount != policy.Graph.NodeCount || policy.HasAssociatedSource ||
            policy.Controller.SecondaryCountProperty12d0003 != 2)
            throw new ArgumentException("Actual configured fresh CASE source/pass ownership required.", nameof(actualPass));
        if (actualPass.Storage.NextUnpublishedNode2490 != 0 || actualPass.NodeCount > 120)
            throw new NotSupportedException("Only fresh one-shot configured2410 marking through node119 is restored.");
        actualPass.Storage.ValidateResultQueryEpoch(cancellationToken);
        var source = LegacyMainCompactPublicationSource.Final(resource, actualPass.Storage, policy, 0, cancellationToken);
        return new(actualPass, LegacyOwnedMainCompactWordReachability.PrepareSource(resource, source,
            cancellationToken, configuredSecondary: true));
    }

    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int endingNode) => marked.GetNodeSnapshot(endingNode);
    public byte[] CopySelectedNodes() => marked.CopySelectedNodes();
}

//Literal208340..2084B7. Arrays are private snapshots of the same source.
//Primary-ordinal links deliberately do not stop at an existing mark; the
//native early-stop test is exclusively for ordinals >=230A.
internal static class LegacyNativeSecondaryChainReachability
{
    internal static void Mark(LegacyMainDirectWordNodeResult[] nodes, byte[] selected,
        int startingNode, int primaryLimit, int node, int ordinal, CancellationToken token)
    {
        for (int alternative = 0; alternative < 2; alternative++)
        {
            token.ThrowIfCancellationRequested();
            byte[] initial = Storage(node, ordinal, alternative);
            int at = (ordinal * 2 + alternative) * 40;
            if (BinaryPrimitives.ReadInt32LittleEndian(initial.AsSpan(at)) >= 0x4000000) break;
            int currentNode = node, currentOrdinal = ordinal, currentAlternative = alternative;
            ushort replacement = ordinal < primaryLimit ? ushort.MaxValue : U16(initial, at + 0x20);
            byte replacementAlternative = ordinal < primaryLimit ? byte.MaxValue : initial[at + 0x22];
            //The native source spans own a backwards DAG. Refuse malformed
            //prepared cycles instead of allowing unbounded portable execution.
            int remaining = checked(nodes.Length * 65536);
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (--remaining < 0) throw new InvalidDataException("Secondary predecessor chain is cyclic.");
                byte[] storage = Storage(currentNode, currentOrdinal, currentAlternative);
                int offset = (currentOrdinal * 2 + currentAlternative) * 40;
                if (currentOrdinal >= primaryLimit)
                {
                    if ((sbyte)storage[offset + 0x1b] < 0) break;
                    selected[currentNode] = 1;
                }
                storage[offset + 0x1b] |= 0x80;
                currentNode = unchecked(currentNode - (sbyte)storage[offset + 0x1a] - 1);
                if (currentNode < startingNode) break;
                currentOrdinal = U16(storage, offset + 0x18);
                currentAlternative = storage[offset + 0x1b] & 0x7f;
                if (currentOrdinal >= primaryLimit)
                {
                    if (replacement != ushort.MaxValue)
                    { currentOrdinal = replacement; currentAlternative = replacementAlternative; }
                    byte[] previous = Storage(currentNode, currentOrdinal, currentAlternative);
                    int previousAt = (currentOrdinal * 2 + currentAlternative) * 40;
                    replacement = U16(previous, previousAt + 0x20);
                    replacementAlternative = previous[previousAt + 0x22];
                }
            }
        }

        byte[] Storage(int n, int row, int k)
        {
            if ((uint)n >= nodes.Length || n < startingNode || (uint)row >= nodes[n].SourceEnd23f0 || (uint)k >= 2)
                throw new InvalidDataException("Secondary predecessor must name its actual node/ordinal/alternative.");
            if (!MemoryMarshal.TryGetArray(nodes[n].SecondaryAlternatives, out ArraySegment<byte> array) ||
                array.Offset != 0 || array.Array is null || array.Count != nodes[n].Records.Length / 88 * 80)
                throw new InvalidDataException("Owned full2410 snapshot storage required.");
            return array.Array;
        }
    }

    private static ushort U16(byte[] storage, int offset)
        => BinaryPrimitives.ReadUInt16LittleEndian(storage.AsSpan(offset));
}
