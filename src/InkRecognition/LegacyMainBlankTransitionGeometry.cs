using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Owned main2971941B0 cache10 and195C40 kind5 branch. The actual
/// post-first source owns its graph, guide and descriptor facts.
/// This prepares an explicit240 invocation, not its case-wrapper factory.</summary>
public sealed class LegacyMainBlankTransitionGeometry
{
    private readonly int[][] costs;
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly SourceBinding source;
    public LegacyFreshSingleGlyphMainSampled Source => source.Single ??
        throw new InvalidOperationException("The multi-physical owner uses MainLineSource.");
    public LegacyFreshMainSingleLineSampled MainLineSource => source.Line ??
        throw new InvalidOperationException("The single-glyph owner uses Source.");
    public float Threshold70 { get; }
    public bool RequireWholePhysicalStarts10c { get; }

    public LegacyMainBlankTransitionGeometry(ReadOnlySpan<byte> actualResource,
        LegacyFreshSingleGlyphMainSampled actualSource, LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken = default)
        :this(actualResource,SourceBinding.FromSingle(actualSource),table,cancellationToken) { }

    /// <summary>Same native cache10/kind5 geometry over an owned multi-physical
    /// single descriptor and its actual guide. No singleton frame is synthesized.</summary>
    public LegacyMainBlankTransitionGeometry(ReadOnlySpan<byte> actualResource,
        LegacyFreshMainSingleLineSampled actualSource, LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken=default)
        :this(actualResource,SourceBinding.FromLine(actualSource),table,cancellationToken) { }

    private LegacyMainBlankTransitionGeometry(ReadOnlySpan<byte> actualResource,
        SourceBinding actualSource,LegacyCursiveSeparationTable table,CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);
        cancellationToken.ThrowIfCancellationRequested();
        var categories=LegacySourceSamplerCategoryOwner.ReadV10dSource(actualResource,0,cancellationToken);
        if(actualSource.Categories.SelectedSourceOffset!=0||categories.SourceFingerprint!=actualSource.Categories.SourceFingerprint)
            throw new ArgumentException("Actual owned main0 resource and sampled source required.");
        int sampler = actualSource.Categories.SamplerNodeOffset;
        var profile = LegacyMainWordTransitionProfile.ReadV106Node(actualResource, sampler, cancellationToken);
        _ = LegacyCursiveCategoryProfile.Load(actualResource[(sampler + 65)..], sampler + 65,
            out int categoryBytes, cancellationToken);
        int tail = checked(sampler + 65 + categoryBytes);
        int end = checked(sampler + 8 + (int)BinaryPrimitives.ReadUInt32LittleEndian(actualResource[(sampler + 4)..]));
        if (tail + 214 != end || actualResource[tail + 189] != 0)
            throw new NotSupportedException("Owned main106 no10C whole-start filter branch required.");
        Threshold70 = BinaryPrimitives.ReadSingleLittleEndian(actualResource[(tail + 33)..]);
        RequireWholePhysicalStarts10c = false;
        source = actualSource;
        graph = actualSource.Graph;
        var ordinary = actualSource.CreateGeometry(table,cancellationToken);
        costs = new int[graph.NodeCount][];
        for (int node = 0; node < graph.NodeCount; node++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var arcs = graph.GetNode(node);
            costs[node] = new int[arcs.Count];
            for (int ordinal = 0; ordinal < arcs.Count; ordinal++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var arc = arcs[ordinal];
                if (node == arc.Span || actualSource.LineForArc(node,ordinal) == -1)
                { costs[node][ordinal] = 0x4000000; continue; }
                int previous = node - arc.Span - 1;
                float prefix = -float.MaxValue;
                foreach (int preceding in arc.PredecessorArcIndices.Span)
                    prefix = Math.Max(prefix, ordinary.GetPreparedState(previous, preceding).PrefixMaximumX);
                var box = ordinary.GetPreparedState(node, ordinal).Bounds;
                if (prefix == -float.MaxValue) prefix = box.MaximumX;
                costs[node][ordinal] = LegacyCursiveBlankGapCost.Evaluate(box.MinimumX, box.MaximumX,
                    prefix, actualSource.Height, Threshold70, profile.GapSlope, false, table.Values.Span);
            }
        }
    }

    public int ReadPreparedBlankCost(int node, ushort ordinal)
    {
        var arcs = graph.GetNode(node);
        if (ordinal >= arcs.Count) throw new ArgumentOutOfRangeException(nameof(ordinal));
        return costs[node][ordinal];
    }

    public bool Calculate118(LegacyNativeWordRecombinationRequest request,
        CancellationToken cancellationToken = default)
    {
        Check(request, cancellationToken);
        //1A82A0: actual owned same-descriptor groups resolve the same descriptor0, low
        //descriptor type0, protocolindex-1 and no arc8; status is literal0.
        //Main10C=false: no graph188(current/next) condition is introduced.
        int before = request.Cost.Value;
        int result = unchecked(before + costs[request.ReadInt32(0)][request.ReadUInt16(12)]);
        request.Cost.Value = result;
        bool accepted = result < request.ReadInt32(52);
        Write32(request, 48, accepted ? unchecked(request.ReadInt32(48) + result - before) : 0x4000000);
        cancellationToken.ThrowIfCancellationRequested();
        return accepted;
    }

    public bool Validate140(LegacyNativeWordRecombinationRequest request,
        CancellationToken cancellationToken = default)
    {
        Check(request, cancellationToken);
        //Main runtime50=0 makes graph192=0;1ACFA0 accepts before pair geometry.
        return true;
    }

    private void Check(LegacyNativeWordRecombinationRequest request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        token.ThrowIfCancellationRequested();
        if (request.ReadByte(28) != 5 || request.ReadUInt16(36) != source.Alphabet.LabelCount)
            throw new NotSupportedException("Actual2136D0 kind5/count-symbol request required.");
        int node = request.ReadInt32(0), previous = request.ReadInt32(4);
        var arcs = graph.GetNode(node);
        var preceding = graph.GetNode(previous);
        if (request.ReadUInt16(12) >= arcs.Count || request.ReadUInt16(14) >= preceding.Count ||
            (arcs[request.ReadUInt16(12)].Flags & 8) != 0)
            throw new ArgumentException("Owned current/previous main graph ordinals required.");
    }

    private static void Write32(LegacyNativeWordRecombinationRequest request, int offset, int value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        for (int index = 0; index < bytes.Length; index++) request.WriteByte(offset + index, bytes[index]);
    }

    private sealed record SourceBinding(LegacyFreshSingleGlyphMainSampled? Single,LegacyFreshMainSingleLineSampled? Line)
    {
        public static SourceBinding FromSingle(LegacyFreshSingleGlyphMainSampled source)
        {ArgumentNullException.ThrowIfNull(source);return new(source,null);}
        public static SourceBinding FromLine(LegacyFreshMainSingleLineSampled source)
        {ArgumentNullException.ThrowIfNull(source);return new(null,source);}
        public LegacyOrdinaryCursiveCandidateGraph Graph=>Single is {} s?s.SourceRanges.Graph:Line!.SourceRanges.Graph;
        public LegacySourceSamplerCategoryOwner Categories=>Single is {} s?s.Owner.CategoryOwner:Line!.Owner.CategoryOwner;
        public LegacyInkAlphabet Alphabet=>Single is {} s?s.Owner.AcousticAlphabet:Line!.Owner.AcousticAlphabet;
        public float Height=>Single is {} s?s.SourceHeight:Line!.Owner.SourceHeight;
        public int LineForArc(int node,int ordinal)=>Single is {} s?s.GetSourceLineIdForArc(node,ordinal):Line!.Owner.SourceLineId;
        public LegacyMainWordTransitionGeometry CreateGeometry(LegacyCursiveSeparationTable table,CancellationToken token)
            =>Single is {} s?s.CreatePrepared240TransitionGeometry(table,token):Line!.CreatePrepared240TransitionGeometry(table,token);
    }

}
