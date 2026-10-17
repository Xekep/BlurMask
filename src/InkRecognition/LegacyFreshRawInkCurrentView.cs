using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyRawInkCurrentView.Probe")]

namespace InkRecognition;

/// <summary>A library-owned ordinary C49 curve using the native C1C/C32
/// float32 X/Y layout. Identity is portable ownership, never a native pointer.
/// This does not create an SDK page, input tag, or native C51 registration.</summary>
public sealed class LegacyOwnedRawInkCurve
{
    private readonly byte[] xy;
    public LegacyOwnedRawInkDocument Document { get; }
    public int OriginalSourceIndex { get; }
    public int PointCount => xy.Length / 8;
    public int NativeConcreteType => 0xc49;
    internal LegacyOwnedRawInkCurve(LegacyOwnedRawInkDocument document, int index)
    {
        Document = document; OriginalSourceIndex = index;
        var points = document.Strokes[index].Points; xy = new byte[checked(points.Count * 8)];
        for (int i = 0; i < points.Count; i++)
        {
            BinaryPrimitives.WriteSingleLittleEndian(xy.AsSpan(i * 8), points[i].X);
            BinaryPrimitives.WriteSingleLittleEndian(xy.AsSpan(i * 8 + 4), points[i].Y);
        }
    }
    public byte[] CopyPointStorage() => xy.ToArray();
    /// <summary>The complete assigned raw channel-layout header, excluding
    /// the native allocation's pointer/count header and allocator residue.</summary>
    public byte[] CopyChannelLayout()
    {
        var bytes = new byte[36];
        uint[] values = [2, 0, 0, 8, 0x10420, 4, 0, 8, 0x10420];
        for (int i = 0; i < values.Length; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(i * 4), values[i]);
        return bytes;
    }
}

/// <summary>One actual current-view link. The retained concrete C49 and its
/// original source ID remain distinct from reordered recognizer physical IDs.</summary>
public sealed class LegacyOwnedRawInkCurrentViewEntry
{
    public LegacyOwnedRawInkCurve Curve { get; }
    public int CurrentViewIndex { get; }
    public LegacyNativeSelectedInkRange Descriptor { get; }
    internal LegacyOwnedRawInkCurrentViewEntry(LegacyOwnedRawInkCurve curve, int index,
        LegacyNativeSelectedInkRange descriptor)
    { Curve = curve; CurrentViewIndex = index; Descriptor = descriptor; }
    /// <summary>The eight assigned scalar bytes after the native pointer
    /// lane: original source ID and zero view flags. No fabricated pointers.</summary>
    public byte[] CopyScalarStorage()
    {
        var bytes = new byte[8];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, Descriptor.SourceIndex);
        return bytes;
    }
}

/// <summary>Owned kind0 raw append and full selected-view lineage. It covers
/// this library's immutable XY root only: no external source objects, C25,
/// C4C transforms, page observers, tag registry, or prior selected views.
/// Native06B7F0 matching is checked before identity can be exported.</summary>
public sealed class LegacyFreshRawInkCurrentView
{
    private readonly LegacyOwnedRawInkCurve[] curves;
    private readonly LegacyOwnedRawInkCurrentViewEntry[] views;
    public LegacyOwnedRawInkDocument Document { get; }
    public IReadOnlyList<LegacyOwnedRawInkCurve> Curves { get; }
    public IReadOnlyList<LegacyOwnedRawInkCurrentViewEntry> Views { get; }
    private LegacyFreshRawInkCurrentView(LegacyOwnedRawInkDocument document,
        LegacyOwnedRawInkCurve[] curves, LegacyOwnedRawInkCurrentViewEntry[] views)
    { Document = document; this.curves = curves; this.views = views;
      Curves = Array.AsReadOnly(curves); Views = Array.AsReadOnly(views); }

    public static LegacyFreshRawInkCurrentView Prepare(LegacyOwnedRawInkDocument actualDocument,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualDocument); token.ThrowIfCancellationRequested();
        var curves = new LegacyOwnedRawInkCurve[actualDocument.ObjectCount];
        var selections = actualDocument.CopySelectedInputs();
        // The raw entrypoint constructs one full current view per original
        // object. Do not silently widen a later clipped/reused document.
        if (selections.Length != curves.Length)
            throw new NotSupportedException("Complete fresh ordinary raw/current-view coverage is required.");
        var views = new LegacyOwnedRawInkCurrentViewEntry[curves.Length];
        for (int i = 0; i < curves.Length; i++)
        {
            token.ThrowIfCancellationRequested(); var input = selections[i];
            if (input.SourceIndex != (uint)i || input.PointCount != actualDocument.Strokes[i].Points.Count ||
                input.IsKnownText || input.FirstPoint != 0 || input.LastPoint != -1 ||
                input.FirstFraction != 0 || input.LastFraction != 0 || !input.FirstClosed || !input.LastClosed)
                throw new NotSupportedException("This factory requires the actual full kind0 raw append and selected-view lineage.");
            curves[i] = new(actualDocument, i);
            // C49.V28 resolves the default -1 endpoint to raw count minus one.
            views[i] = new(curves[i], i, input with { LastPoint = input.PointCount - 1 });
        }
        return new(actualDocument, curves, views);
    }

    public LegacyOwnedRawInkIdentityExport ExportIdentityTransforms(
        ReadOnlySpan<LegacyNativeInkPositionRange> actualRanges, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var objects = curves.Select(c => new LegacyNativeSourceInkObject(c.PointCount)).ToArray();
        var selected = LegacyNativeSelectedInkProducer.Select(actualRanges, objects, token);
        var descriptors = views.Select(v => v.Descriptor).ToArray();
        var matches = new int[selected.Length];
        for (int i = 0; i < selected.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            matches[i] = LegacyRawInkCurrentViewMatching.RequireCover(descriptors, selected[i]);
        }
        return new(this, actualRanges.ToArray(), selected, matches);
    }
}

/// <summary>06B7F0's non-null raw-base/current-view branch and06F080's exact
/// identity exclusion. Nonempty source ranges can produce zero transform
/// records. This is not the037E80 caller, DAF input32, or document acceptance.</summary>
public sealed class LegacyOwnedRawInkIdentityExport
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    private readonly LegacyNativeSelectedInkRange[] fragments;
    private readonly int[] matches;
    public LegacyFreshRawInkCurrentView Owner { get; }
    public int MatricesResolved => fragments.Length;
    public int NativeNonidentityTransformCount => 0;
    internal LegacyOwnedRawInkIdentityExport(LegacyFreshRawInkCurrentView owner,
        LegacyNativeInkPositionRange[] ranges, LegacyNativeSelectedInkRange[] fragments, int[] matches)
    { Owner = owner; this.ranges = ranges; this.fragments = fragments; this.matches = matches; }
    public LegacyNativeInkPositionRange[] CopyRanges() => ranges.ToArray();
    public LegacyNativeSelectedInkRange[] CopyFragments() => fragments.ToArray();
    public int[] CopyMatchedViewIndices() => matches.ToArray();
    public byte[] CopyResolvedMatrices()
    {
        var bytes = new byte[checked(fragments.Length * 48)];
        for (int i = 0; i < fragments.Length; i++)
        {
            BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(i * 48), 1);
            BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(i * 48 + 32), 1);
        }
        return bytes;
    }
}

internal static class LegacyRawInkCurrentViewMatching
{
    internal static int RequireCover(ReadOnlySpan<LegacyNativeSelectedInkRange> views, LegacyNativeSelectedInkRange request)
    {
        for (int i = 0; i < views.Length; i++) if (Covers(views[i], request)) return i;
        throw new NotSupportedException("No same-source current view covers this fragment; native06B7F0 can leave the matrix unwritten.");
    }
    // Selected descriptors are original-owner coordinates. Native first
    // performs rounded endpoint equality, then a float containment test.
    internal static bool Covers(LegacyNativeSelectedInkRange view, LegacyNativeSelectedInkRange request)
    {
        if (view.SourceIndex != request.SourceIndex || view.PointCount != request.PointCount ||
            view.IsKnownText || request.IsKnownText) return false;
        Validate(view); Validate(request);
        int first = request.FirstPoint, last = request.LastPoint == -1 ? request.PointCount - 1 : request.LastPoint;
        int viewLast = view.LastPoint;
        bool exact = first == view.FirstPoint && Round(request.FirstFraction) == Round(view.FirstFraction) &&
            request.FirstClosed == view.FirstClosed &&
            Sentinel(last) == Sentinel(viewLast) && Round(request.LastFraction) == Round(view.LastFraction) &&
            request.LastClosed == view.LastClosed;
        if (exact) return true;
        return view.FirstPoint <= first && (first != view.FirstPoint ||
            (view.FirstFraction <= request.FirstFraction &&
             (request.FirstFraction != view.FirstFraction || !request.FirstClosed || view.FirstClosed || first != 0))) &&
            Sentinel(last) <= Sentinel(viewLast) && (viewLast != last ||
            (request.LastFraction <= view.LastFraction &&
             (view.LastFraction != request.LastFraction || view.LastClosed || !request.LastClosed || last != view.PointCount - 1)));
    }
    private static int Sentinel(int n) => n == -1 ? int.MaxValue : n;
    private static int Round(float f) => (int)(f * 200f + .5f);
    private static void Validate(LegacyNativeSelectedInkRange value)
    {
        int last = value.LastPoint == -1 ? value.PointCount - 1 : value.LastPoint;
        if (value.PointCount is < 1 or > 4096 || value.FirstPoint < 0 || last < value.FirstPoint ||
            last >= value.PointCount || !float.IsFinite(value.FirstFraction) || !float.IsFinite(value.LastFraction) ||
            value.FirstFraction is < 0 or >= 1 || value.LastFraction is < 0 or >= 1 ||
            (last == value.PointCount - 1 && value.LastFraction != 0) ||
            (value.FirstPoint == last && value.FirstFraction > value.LastFraction))
            throw new ArgumentException("A bounded original-owner source fragment is required.");
    }
}
