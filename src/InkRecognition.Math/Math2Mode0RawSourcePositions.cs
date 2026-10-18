using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathSourcePositions.Probe")]

namespace InkRecognition.Math;

// Exact raw V18 scalar channel, separate from the initial00DB00 Pack codec.
internal static class Math2Mode0SourcePositionCoordinates
{
    internal static ulong Compose(ulong left, ulong right, byte fraction)
    {
        if (fraction is 0 or 255) return left;
        static float Coordinate(ulong p)
        {
            byte q = (byte)(p >> 8);
            float part = q == 255 ? 0f : q * .005f;
            return (float)(ushort)(p >> 16) + part;
        }
        float a = Coordinate(left), b = Coordinate(right);
        float x = b - a; x *= fraction; x *= .005f; x += a;
        int integral = (int)MathF.Floor(x);
        float remainder = x - (float)integral; remainder += .0025f; remainder *= 200f;
        byte q = unchecked((byte)(int)remainder);
        return (left & 0xffffffff000000ffUL) | ((ulong)unchecked((ushort)integral) << 16) | ((ulong)q << 8);
    }
}

/// <summary>Private original-source identity plus the SAME processed feature
/// positions. No decoder labels, caller range matrix or external SDK page IDs.</summary>
internal sealed class Math2Mode0RawSourcePositions
{
    internal LegacyOwnedRawInkDocument Document { get; }
    internal int InputFrameCount { get; }
    internal long PayloadReservation { get; }
    private readonly LegacyNativeInkPositionSnapshot snapshot;
    private readonly int[] sourceOrder;
    private readonly WeakReference<MathFeatures> featureLease;
    private Math2Mode0RawSourcePositions(LegacyOwnedRawInkDocument document, MathFeatures features,
        LegacyNativeInkPositionSnapshot snapshot, int[] sourceOrder, long payload)
    { Document = document; InputFrameCount = features.Frames; featureLease = new(features);
      this.snapshot = snapshot; this.sourceOrder = sourceOrder; PayloadReservation = payload; }
    internal void ValidateFeatures(MathFeatures actual)
    {
        if (!featureLease.TryGetTarget(out var prepared) || !ReferenceEquals(prepared, actual))
            throw new ArgumentException("The exact same actual feature preparation is required.");
    }
    internal ulong[] CopyPositions() => snapshot.Positions.ToArray().Select(p => p.Value).ToArray();
    internal int[] CopySeparators() => snapshot.Separators.ToArray();
    internal int[] CopySourceOrder() => sourceOrder.ToArray();
    internal LegacyNativeInkPositionRange[] ReadRange(int first, int last, CancellationToken token)
        => snapshot.ReadRange(first, last, token);

    internal static (MathFeatures Features, Math2Mode0RawSourcePositions Positions) PrepareFeatures(
        IReadOnlyList<InkStroke> strokes, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(strokes); token.ThrowIfCancellationRequested();
        if (strokes.Count is < 1 or > 256) throw new ArgumentException("One to256 original raw source objects required.");
        int count = 0;
        foreach (var stroke in strokes)
        {
            token.ThrowIfCancellationRequested();
            var points = stroke?.Points ?? throw new ArgumentException("Null raw source object.");
            if (points.Count < 1 || points.Count > 4096 - count)
                throw new NotSupportedException("Source-bound Math currently admits at most4096 complete original points.");
            count += points.Count;
            foreach (var point in points)
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) || MathF.Abs(point.X) > 100000 || MathF.Abs(point.Y) > 100000)
                    throw new ArgumentException("Finite bounded original raw source points required.");
        }
        var document = LegacyOwnedRawInkDocument.Create(strokes, token);
        return MathPreprocessor.ProcessWithSourcePositions(document, token);
    }

    internal sealed class Builder
    {
        private readonly LegacyOwnedRawInkDocument document;
        private readonly LegacyNativeInkPosition[] prior;
        private readonly int[] priorEnds, order;
        private readonly List<LegacyNativeInkPosition> result = [];
        private readonly int[] ends;
        private int completed;
        internal Builder(LegacyOwnedRawInkDocument document, int[] actualOrder, CancellationToken token)
        {
            this.document = document; order = actualOrder.ToArray();
            var initial = LegacyNativeInkPositionSnapshot.Create(document.CopySelectedInputs(),
                [new(LegacyNativeInkPosition.Pack(0, 0, 3, document.Strokes[0].Points.Count),
                    LegacyNativeInkPosition.Pack((uint)(document.ObjectCount - 1), document.Strokes[^1].Points.Count - 1,
                        5, document.Strokes[^1].Points.Count))], token);
            if (order.Length != document.ObjectCount || order.Distinct().Count() != order.Length ||
                order.Any(index => (uint)index >= (uint)document.ObjectCount))
                throw new ArgumentException("Actual complete source permutation required.");
            var original = initial.Positions; var originalEnds = initial.Separators;
            prior = new LegacyNativeInkPosition[original.Length]; priorEnds = new int[order.Length]; ends = new int[order.Length];
            int at = 0;
            for (int i = 0; i < order.Length; i++)
            {
                int source = order[i], begin = source == 0 ? 0 : originalEnds[source - 1];
                int count = originalEnds[source] - begin;
                original.Slice(begin, count).CopyTo(prior.AsSpan(at)); at += count; priorEnds[i] = at;
            }
        }
        internal void AppendCoordinate(int input, int point, byte fraction)
        {
            if (input != completed || (uint)input >= (uint)order.Length)
                throw new InvalidOperationException("Source coordinates must follow the same actual processing order.");
            int begin = input == 0 ? 0 : priorEnds[input - 1], count = priorEnds[input] - begin;
            if ((uint)point >= (uint)count || (fraction is not (0 or 255) && point + 1 >= count))
                throw new ArgumentOutOfRangeException(nameof(point));
            if (result.Count >= MathPreprocessor.MaxFrames) throw new NotSupportedException("Bounded feature-position payload required.");
            ulong left = prior[begin + point].Value;
            ulong right = fraction is 0 or 255 ? left : prior[begin + point + 1].Value;
            result.Add(new(Math2Mode0SourcePositionCoordinates.Compose(left, right, fraction)));
        }
        internal void CompleteStroke(int input)
        {
            int first = input == 0 ? 0 : ends[input - 1];
            if (input != completed || result.Count <= first) throw new InvalidOperationException("Nonempty source segment required.");
            //017060 always rewrites the last temporary coordinate to the raw
            //terminal, even when that frame's XY sample remains interior.
            result[^1] = prior[priorEnds[input] - 1];
            ends[input] = result.Count; completed++;
        }
        internal Math2Mode0RawSourcePositions Finish(MathFeatures features, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (completed != order.Length || features.Frames != result.Count ||
                !features.StrokeEnds.SequenceEqual(ends.Select(end => (long)end)))
                throw new InvalidOperationException("Positions and actual feature/strk_ends owner must coincide.");
            var fields = result.ToArray();
            var snapshot = LegacyNativeInkPositionSnapshot.CreateProcessedForMath(document, fields, ends, order, token);
            long rawPoints = document.Strokes.Sum(stroke => (long)stroke.Points.Count);
            //Retained XY and packed positions, plus a conservative128 bytes
            //per source for both document/map selected-row copies, source
            //references, separators and permutation. Two enclosing ranges
            //cover document and snapshot. This is not CLR/RSS accounting.
            long payload = checked(rawPoints * 8 + fields.Length * 8L + order.Length * 128L + 32);
            return new(document, features, snapshot, order.ToArray(), payload);
        }
    }
}
