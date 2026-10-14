using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Explicit ordinary prepared source facts. These are compact-group
///and input-descriptor fields, not physical stroke ordinals. No main source
///postprocessor/frame factory is inferred from the first-parser frame.</summary>
public readonly record struct LegacyMainWordArcSourceFacts(int DescriptorOrdinal, int DescriptorLineId,
    ushort DescriptorFlags, short BoundaryIndex, byte EndpointNibbles, byte GroupKind, byte GroupFlags6);

/// <summary>Actual main297 ordinary1941b0 caches and full80 frontend118/140
///consumer. Source guide height/gap and prepared source facts are explicit.
///Supports same-descriptor plain-symbol kinds0/2, with no boundary network,
///Arabic context, additional opaque sampler or special-label bypass.</summary>
public sealed class LegacyMainWordTransitionGeometry
{
    private const int Impossible = 0x4000000;
    private readonly LegacyMainWordTransitionProfile profile;
    private readonly LegacyInkAlphabet alphabet;
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly LegacyMainWordArcSourceFacts[][] facts;
    private readonly LegacyFirstTransitionBounds[][] bounds;
    private readonly float[][] futureMinimum, prefixMaximum;
    private readonly int[][] join, tripleJoin;
    private readonly int[][][] matrix;
    private readonly bool[][] wholeStarts;
    private readonly float height;

    public LegacyMainWordTransitionGeometry(LegacyMainWordTransitionProfile profile, LegacyInkAlphabet alphabet,
        LegacyCursivePreparedInk ink, LegacyOrdinaryCursiveCandidateGraph graph,
        IReadOnlyList<ReadOnlyMemory<LegacyMainWordArcSourceFacts>> preparedSourceFacts,
        float sourceGuideHeight, float sourceGap, LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile); ArgumentNullException.ThrowIfNull(alphabet);
        ArgumentNullException.ThrowIfNull(ink); ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(preparedSourceFacts); ArgumentNullException.ThrowIfNull(table);
        cancellationToken.ThrowIfCancellationRequested();
        if (!float.IsFinite(sourceGuideHeight) || sourceGuideHeight <= 0 || !float.IsFinite(sourceGap) ||
            profile.Categories.BaseEntryCount != alphabet.LabelCount || preparedSourceFacts.Count != graph.NodeCount ||
            graph.NodeCount is < 1 or > 4096 || graph.CandidateCount > 131072)
            throw new ArgumentException("Matching main profile, owned graph and explicit finite source guide required.");
        this.profile = profile; this.alphabet = alphabet; this.graph = graph; height = sourceGuideHeight;
        // Validate ordinal extents before copying caller memory. In particular,
        // a large foreign row must not cause a large allocation before refusal.
        facts = new LegacyMainWordArcSourceFacts[graph.NodeCount][];
        for (int node = 0; node < graph.NodeCount; node++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var row = preparedSourceFacts[node];
            if (row.Length != graph.GetNode(node).Count)
                throw new ArgumentException("Source facts must cover actual graph ordinals.");
            facts[node] = row.ToArray();
        }
        bounds = new LegacyFirstTransitionBounds[graph.NodeCount][];
        futureMinimum = new float[graph.NodeCount][]; prefixMaximum = new float[graph.NodeCount][];
        join = new int[graph.NodeCount][]; tripleJoin = new int[graph.NodeCount][];
        matrix = new int[graph.NodeCount][][]; wholeStarts = new bool[graph.NodeCount][];
        var boxes = graph.Groups.Select(group => Box(ink, group.Ranges.Span, cancellationToken)).ToArray();
        int descriptor = facts[0].Length == 0 ? -1 : facts[0][0].DescriptorOrdinal;
        int line = facts[0].Length == 0 ? -2 : facts[0][0].DescriptorLineId;
        for (int node = 0; node < graph.NodeCount; node++)
        {
            var arcs = graph.GetNode(node);
            if (facts[node].Length != arcs.Count) throw new ArgumentException("Source facts must cover actual graph ordinals.");
            bounds[node] = new LegacyFirstTransitionBounds[arcs.Count]; futureMinimum[node] = new float[arcs.Count];
            prefixMaximum[node] = new float[arcs.Count]; join[node] = new int[arcs.Count]; tripleJoin[node] = new int[arcs.Count];
            matrix[node] = new int[arcs.Count][]; wholeStarts[node] = new bool[arcs.Count];
            for (int ordinal = 0; ordinal < arcs.Count; ordinal++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var f = facts[node][ordinal];
                if (descriptor < 0 || line < -1 || f.DescriptorOrdinal != descriptor || f.DescriptorLineId != line ||
                    (f.DescriptorFlags & 15) != 0 || f.BoundaryIndex >= 0 || f.EndpointNibbles != 0 ||
                    f.GroupKind != 0 || f.GroupFlags6 != 0 || (arcs[ordinal].Flags & 8) != 0)
                    throw new NotSupportedException("Only explicit ordinary same-descriptor source facts without boundary overrides are supported.");
                bounds[node][ordinal] = boxes[arcs[ordinal].GroupId]; matrix[node][ordinal] = new int[36];
                wholeStarts[node][ordinal] = graph.Groups[arcs[ordinal].GroupId].Ranges.Span.ToArray().All(range => range.StartPoint == 0);
            }
        }
        var connections = new LegacyOrdinaryCandidateConnections(graph, cancellationToken);
        for (int node = graph.NodeCount - 1; node >= 0; node--)
            for (ushort ordinal = 0; ordinal < bounds[node].Length; ordinal++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                float low = bounds[node][ordinal].MinimumX;
                if (node != graph.GetNode(node)[ordinal].Span && line != -1 && node + 1 < graph.NodeCount)
                    foreach (int next in connections.GetSuccessorOrdinals(node, ordinal, 0, cancellationToken).Span)
                        low = Math.Min(low, futureMinimum[node + 1][next]);
                futureMinimum[node][ordinal] = low;
            }
        float defaultGap = sourceGap >= 0 ? sourceGap * profile.SourceGapScale : profile.DefaultGap;
        float matrixScale = sourceGap < 0 || profile.Matrix.Span[0] == 0 ? 1 : sourceGap * profile.SourceMatrixScale / profile.Matrix.Span[0];
        for (int node = 0; node < graph.NodeCount; node++)
            for (int ordinal = 0; ordinal < bounds[node].Length; ordinal++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var arc = graph.GetNode(node)[ordinal]; var box = bounds[node][ordinal];
                if (node == arc.Span || line == -1)
                {
                    prefixMaximum[node][ordinal] = box.MaximumX;
                    join[node][ordinal] = tripleJoin[node][ordinal] = Impossible;
                    Array.Fill(matrix[node][ordinal], Impossible);
                    continue;
                }
                int previous = node - arc.Span - 1; float high = -float.MaxValue;
                foreach (int preceding in arc.PredecessorArcIndices.Span)
                {
                    if (preceding < 0 || preceding >= bounds[previous].Length)
                        throw new InvalidDataException("Prepared graph contains invalid previous source arc.");
                    high = Math.Max(high, prefixMaximum[previous][preceding]);
                }
                if (high == -float.MaxValue) high = box.MaximumX;
                prefixMaximum[node][ordinal] = Math.Max(box.MaximumX, high);
                float gap = (futureMinimum[node][ordinal] - high) / height;
                join[node][ordinal] = Math.Min(LogCost((gap - defaultGap) * -profile.GapSlope, table.Values.Span, true), profile.JoinMaximumCost);
                tripleJoin[node][ordinal] = Math.Min(LogCost((gap - defaultGap * 3f) * -profile.GapSlope, table.Values.Span, true), profile.JoinMaximumCost);
                for (int i = 0; i < 36; i++)
                {
                    int cost = LogCost((gap - matrixScale * profile.Matrix.Span[i]) * profile.GapSlope, table.Values.Span, false);
                    matrix[node][ordinal][i] = i / 6 == 5 ? checked(cost - 1) : i % 6 == 5 ? cost : Math.Min(cost, profile.MatrixMaximumCost);
                }
            }
    }

    public LegacyNativeHigherArcTransitionOperations CreateOperations()
        => LegacyNativeHigherArcTransitionOperations.ForOrdinaryConnections(new(graph), Calculate118, Validate140);

    public bool Calculate118(LegacyNativeWordRecombinationRequest request, CancellationToken cancellationToken = default)
    {
        Check(request, cancellationToken);
        int current = request.ReadInt32(0), currentArc = request.ReadUInt16(12);
        int previous = request.ReadInt32(4), previousArc = request.ReadUInt16(14);
        int before = request.Cost.Value;
        if (request.ReadByte(28) == 2 && !wholeStarts[current][currentArc])
        {
            request.Cost.Value = Impossible; Write32(request, 48, Impossible); return false;
        }
        int correction;
        if (request.ReadByte(28) == 0)
            correction = request.ReadUInt32(64) is 7 or 8 ? tripleJoin[current][currentArc] : join[current][currentArc];
        else
        {
            int first = request.ReadInt32(20), prior = request.ReadUInt32(68) == 10 ? 3 : request.ReadInt32(24);
            if (first is < 0 or > 5 || prior is < 0 or > 5) throw new NotSupportedException("Six-column category cache required.");
            correction = matrix[current][currentArc][first + prior * 6];
        }
        var a = bounds[current][currentArc]; var b = bounds[previous][previousArc];
        float denominator = facts[current][currentArc].DescriptorLineId == -1 ? 100f : height;
        float movement = Math.Max(a.MinimumX - b.MinimumX, a.MaximumX - b.MaximumX) / denominator;
        float term = movement <= profile.TranslationThreshold ? (movement - profile.TranslationThreshold) * profile.GapSlope : 0;
        if (!float.IsFinite(term) || term >= 2147483648f || term < int.MinValue)
            throw new NotSupportedException("Native translation exceeds checked integer conversion domain.");
        int result = unchecked(before - (int)term * 8192 + correction);
        request.Cost.Value = result;
        bool accepted = result < request.ReadInt32(52);
        Write32(request, 48, accepted ? unchecked(request.ReadInt32(48) + result - before) : Impossible);
        cancellationToken.ThrowIfCancellationRequested();
        return accepted;
    }

    public bool Validate140(LegacyNativeWordRecombinationRequest request, CancellationToken cancellationToken = default)
    {
        Check(request, cancellationToken);
        // Main resource runtime50=0 -> graph192=0 ->1acfa0 returns true
        // before any pair geometry. Frontend1528c0 adds a zero cost delta to30.
        return true;
    }

    public (LegacyFirstTransitionBounds Bounds, float FutureMinimumX, float PrefixMaximumX, int JoinCost, int TripleJoinCost)
        GetPreparedState(int node, int arcOrdinal)
        => (bounds[node][arcOrdinal], futureMinimum[node][arcOrdinal], prefixMaximum[node][arcOrdinal], join[node][arcOrdinal], tripleJoin[node][arcOrdinal]);

    private void Check(LegacyNativeWordRecombinationRequest request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request); token.ThrowIfCancellationRequested();
        int current = request.ReadInt32(0), previous = request.ReadInt32(4);
        if (request.ReadByte(28) is not (0 or 2) || current < 0 || current >= graph.NodeCount || previous < 0 || previous >= graph.NodeCount ||
            request.ReadUInt16(12) >= bounds[current].Length || request.ReadUInt16(14) >= bounds[previous].Length || request.ReadUInt32(64) == 9)
            throw new NotSupportedException("Compatible prepared main source arcs and plain ordinary kinds0/2 required.");
        foreach (ushort symbol in new[] { request.ReadUInt16(36), request.ReadUInt16(38) })
            if (!alphabet.TryGetText(symbol, out string text) || text.Length != 1 ||
                !(text[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '\u0400' and <= '\u04ff' or >= '0' and <= '9'))
                throw new NotSupportedException("Special/Arabic/primitive graph status policy is outside this ordinary main scope.");
    }

    private static int LogCost(float value, ReadOnlySpan<float> table, bool bitRounded)
        => LegacyFirstOrdinaryTransitionGeometry.LogCost(value, table, bitRounded);
    private static void Write32(LegacyNativeWordRecombinationRequest request, int at, int value)
    {
        Span<byte> bytes = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        for (int i = 0; i < bytes.Length; i++) request.WriteByte(at + i, bytes[i]);
    }
    private static LegacyFirstTransitionBounds Box(LegacyCursivePreparedInk ink, ReadOnlySpan<LegacyInkRange> ranges, CancellationToken token)
    {
        float x0 = float.MaxValue, y0 = float.MaxValue, x1 = -float.MaxValue, y1 = -float.MaxValue;
        foreach (var range in ranges)
        {
            token.ThrowIfCancellationRequested();
            var counts = ink.StrokePointCounts.Span;
            if ((uint)range.StrokeIndex >= (uint)counts.Length || range.StartPoint < 0 || range.EndPointExclusive <= range.StartPoint ||
                range.EndPointExclusive > counts[range.StrokeIndex]) throw new ArgumentException("Native group lacks owned prepared range.");
            int offset = 0; for (int i = 0; i < range.StrokeIndex; i++) offset += counts[i];
            foreach (var point in ink.Points.Span.Slice(offset + range.StartPoint, range.EndPointExclusive - range.StartPoint))
            {
                x0 = Math.Min(x0, point.X); y0 = Math.Min(y0, point.Y); x1 = Math.Max(x1, point.X); y1 = Math.Max(y1, point.Y);
            }
        }
        return new(x0, y0, x1, y1);
    }
}
