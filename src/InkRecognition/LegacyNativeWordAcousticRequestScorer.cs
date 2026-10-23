using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Owned final native class/cache costs for one actual node/arc.
/// These are not raw neural outputs. Array extents are checked before copying.</summary>
public sealed class LegacyNativeWordAcousticGroupCosts
{
    public ReadOnlyMemory<int> ClassCosts { get; }
    public ReadOnlyMemory<int> GeometryCosts { get; }
    public LegacyNativeWordAcousticGroupCosts(ReadOnlySpan<int> finalClassCosts, ReadOnlySpan<int> geometryCosts)
    {
        if (finalClassCosts.Length is < 1 or > 65535 || geometryCosts.Length > 127)
            throw new ArgumentException("Bounded final class cache and signed-byte geometry table required.");
        ClassCosts = finalClassCosts.ToArray(); GeometryCosts = geometryCosts.ToArray();
    }
}

/// <summary>Literal full-request frontend152790/acoustic1c0fd0 consumer.
/// Threshold selection is SOURCE kind40, geometry index is SIGNED byte12,
/// and request30 accumulates the accepted cost delta. Source/cache ownership
/// and optional frontend suppression remain explicit producers.</summary>
public sealed class LegacyNativeWordAcousticRequestScorer
{
    private const int Impossible = 0x4000000;
    private readonly LegacyAcousticCandidateThresholds thresholds;
    private readonly ushort[] bypassLabels;
    public LegacyNativeWordAcousticRequestScorer(LegacyAcousticCandidateThresholds thresholds,
        ReadOnlySpan<ushort> fiveGeometryBypassLabels)
    {
        thresholds.Validate();
        if (fiveGeometryBypassLabels.Length != 5)
            throw new ArgumentException("Five actual resolved acoustic labels required.");
        this.thresholds = thresholds; bypassLabels = fiveGeometryBypassLabels.ToArray();
    }
    public static LegacyNativeWordAcousticRequestScorer ForAlphabet(LegacyAcousticCandidateThresholds thresholds,
        LegacyInkAlphabet alphabet)
    {
        ArgumentNullException.ThrowIfNull(alphabet);
        ushort[] labels = new[] { "\U000f0001", "\U000f0002", "\U000f0003", "\U000f0004", "\U000f0008" }
            .Select(text => alphabet.TryGetLabel(text, out ushort label) ? label : ushort.MaxValue).ToArray();
        return new(thresholds, labels);
    }

    public bool Score(LegacyNativeWordRecombinationRequest request,
        Func<int, ushort, LegacyNativeWordAcousticGroupCosts> getOwnedGroupCosts,
        bool frontendSuppressionEnabled,
        Func<int, ushort, bool>? isSuppressedE8 = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ScoreCore(new LegacyNativeFullWordAcousticAccessor(request), getOwnedGroupCosts,
            frontendSuppressionEnabled, isSuppressedE8, cancellationToken);
    }

    // Numeric field transport only. A caller of this kernel acquires no source,
    // query, cache, parent, category or mask authority.
    internal bool ScoreCore<TRequest>(TRequest request,
        Func<int, ushort, LegacyNativeWordAcousticGroupCosts> getOwnedGroupCosts,
        bool frontendSuppressionEnabled, Func<int, ushort, bool>? isSuppressedE8,
        CancellationToken cancellationToken)
        where TRequest : struct, ILegacyNativeWordAcousticAccessor
    {
        ArgumentNullException.ThrowIfNull(getOwnedGroupCosts);
        cancellationToken.ThrowIfCancellationRequested();
        int node = request.ReadInt32(0); ushort arc = request.ReadUInt16(12);
        if (node < 0) throw new ArgumentOutOfRangeException(nameof(request));
        if (frontendSuppressionEnabled)
        {
            if (isSuppressedE8 is null) throw new ArgumentException("Actual frontend graphE8 suppression predicate required.");
            bool suppressed = isSuppressedE8(node, arc);
            cancellationToken.ThrowIfCancellationRequested();
            if (suppressed)
            {
                request.Cost.Value = Impossible;
                // Native returns BEFORE reading/updating accumulator30.
                return false;
            }
        }
        int before = request.Cost.Value;
        var group = getOwnedGroupCosts(node, arc);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(group);
        ushort symbol = request.ReadUInt16(36);
        if (symbol >= group.ClassCosts.Length) throw new ArgumentException("Actual class cache does not cover current symbol24.");
        int classCost = group.ClassCosts.Span[symbol];
        uint sourceKind = request.ReadUInt32(64);
        bool rejected = classCost == Impossible || sourceKind switch
        {
            1 => classCost > thresholds.KindOne,
            2 => classCost > thresholds.KindTwo,
            3 => classCost > thresholds.KindThree,
            _ => false
        };
        bool accepted = false;
        if (rejected) request.Cost.Value = Impossible;
        else
        {
            int geometry = 0;
            if (!bypassLabels.AsSpan().Contains(symbol))
            {
                int index = (sbyte)request.ReadByte(18);
                if (index < 0 || index >= group.GeometryCosts.Length)
                    throw new ArgumentException("Actual geometry cache does not cover signed request12 index.");
                geometry = group.GeometryCosts.Span[index];
            }
            // Preserve native signed32 wrap instead of the separate checked
            // scalar scorer's intentionally narrower arithmetic contract.
            int total = unchecked(geometry + before + classCost);
            request.Cost.Value = total;
            accepted = total < request.ReadInt32(52);
        }
        Write32(request, 48, accepted ? unchecked(request.ReadInt32(48) + request.Cost.Value - before) : Impossible);
        cancellationToken.ThrowIfCancellationRequested();
        return accepted;
    }

    private static void Write32<TRequest>(TRequest request, int offset, int value)
        where TRequest : struct, ILegacyNativeWordAcousticAccessor
    {
        Span<byte> bytes = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        for (int i = 0; i < bytes.Length; i++) request.WriteByte(offset + i, bytes[i]);
    }
}
