using System.Numerics;

namespace InkRecognition;

/// <summary>
/// Owns original Math AK XY input and the selected whole-input range projection
/// of MyScript2D00FB90. This is source geometry, not a classifier prediction,
/// processed13A3 owner, grammar match or old candidate-tree authority.
/// </summary>
public sealed partial class LegacyMathAkSourceGeometry
{
    public const int MaximumStrokes = 64;
    public const int MaximumPoints = 16_384;
    private readonly Vector2[][] original;
    private readonly LegacyInkPoint[][] normalized;
    private readonly int[] ends;
    private readonly LegacyMathAkFeatureProfile profile;
    public LegacyMathAkFeatureProfile FeatureProfile => profile;
    public int StrokeCount => original.Length;
    public int PointCount { get; }
    public int NormalizationTarget => 8192;
    public string SourceFingerprint => LegacyMathMixed107RawResource.ResourceFingerprint;
    public bool HasCandidateAuthority => false;
    public bool HasClassifierOutput => false;
    public Vector2 OriginalMinimum { get; }
    public Vector2 OriginalMaximum { get; }
    public float AppliedScale { get; }
    private LegacyMathAkSourceGeometry(Vector2[][] original, LegacyInkPoint[][] normalized,
        int[] ends, int count, Vector2 minimum, Vector2 maximum, float scale, LegacyMathAkFeatureProfile profile)
    {
        this.original = original; this.normalized = normalized; this.ends = ends;
        this.profile = profile;
        PointCount = count; OriginalMinimum = minimum; OriginalMaximum = maximum; AppliedScale = scale;
    }

    /// <summary>
    /// Authenticates a private bounded resource snapshot before reading the
    /// selected13A5 program. Copies caller input before normalization. Original
    /// stroke boundaries are preserved; no extra pen lifts are inferred.
    /// </summary>
    public static LegacyMathAkSourceGeometry CreateActual(ReadOnlySpan<byte> actualResource,
        IReadOnlyList<Vector2[]> strokes, CancellationToken token = default)
        => CreateCore(actualResource, null, strokes, token);
    internal static LegacyMathAkSourceGeometry CreateFromProfile(LegacyMathAkFeatureProfile profile,
        IReadOnlyList<Vector2[]> strokes, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return CreateCore(ReadOnlySpan<byte>.Empty, profile, strokes, token);
    }
    private static LegacyMathAkSourceGeometry CreateCore(ReadOnlySpan<byte> actualResource,
        LegacyMathAkFeatureProfile? existingProfile, IReadOnlyList<Vector2[]> strokes, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(strokes);
        token.ThrowIfCancellationRequested();
        if (existingProfile is null && actualResource.Length != LegacyMathMixed107RawResource.ResourceBytes)
            throw new ArgumentException("Exact actual Math AK resource required.", nameof(actualResource));
        int strokeCount = strokes.Count;
        if (strokeCount is < 1 or > MaximumStrokes)
            throw new ArgumentException("Whole source stroke bound exceeded.", nameof(strokes));
        //Snapshot all input references and preflight counts before any point copy.
        var borrowed = new Vector2[strokeCount][];
        int total = 0;
        for (int i = 0; i < strokeCount; i++)
        {
            token.ThrowIfCancellationRequested();
            Vector2[] row = strokes[i] ?? throw new ArgumentException("Null source stroke.", nameof(strokes));
            if (row.Length == 0 || row.Length > MaximumPoints - total)
                throw new ArgumentException("Nonempty bounded source strokes required.", nameof(strokes));
            total += row.Length; borrowed[i] = row;
        }
        //A shared profile skips all resource hashing and calibration decoding.
        var profile = existingProfile ?? LegacyMathAkFeatureProfile.PrepareActual(actualResource, token);
        var owned = new Vector2[strokeCount][];
        var result = new LegacyInkPoint[strokeCount][];
        var cumulative = new int[strokeCount];
        Vector2 minimum = new(float.MaxValue), maximum = new(-float.MaxValue);
        int end = 0;
        for (int s = 0; s < strokeCount; s++)
        {
            token.ThrowIfCancellationRequested();
            var row = new Vector2[borrowed[s].Length];
            for (int i = 0; i < row.Length; i++)
            {
                if ((i & 255) == 0) token.ThrowIfCancellationRequested();
                Vector2 point = borrowed[s][i];
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) ||
                    MathF.Abs(point.X) > 1_000_000_000f || MathF.Abs(point.Y) > 1_000_000_000f)
                    throw new ArgumentException("Finite bounded source coordinates required.", nameof(strokes));
                row[i] = point;
                //Literal ordered scalar comparisons, retaining original order.
                if (point.X <= minimum.X) minimum.X = point.X;
                if (maximum.X < point.X) maximum.X = point.X;
                if (point.Y <= minimum.Y) minimum.Y = point.Y;
                if (maximum.Y < point.Y) maximum.Y = point.Y;
            }
            owned[s] = row; end += row.Length; cumulative[s] = end;
        }
        float width = maximum.X - minimum.X, height = maximum.Y - minimum.Y;
        //Native00FB90 initializes scale1 and only recomputes when BOTH
        //extents are nonzero. A horizontal/vertical line is not rescaled.
        float scale = 1f;
        if (width != 0f && height != 0f)
        {
            scale = 8192f / width;
            float yScale = 8192f / height;
            if (yScale <= scale) scale = yScale;
        }
        if (!float.IsFinite(scale)) throw new NotSupportedException("Source scale outside finite subset.");
        for (int s = 0; s < strokeCount; s++)
        {
            var row = new LegacyInkPoint[owned[s].Length];
            for (int i = 0; i < row.Length; i++)
            {
                if ((i & 255) == 0) token.ThrowIfCancellationRequested();
                float x = (owned[s][i].X - minimum.X) * scale + 0f;
                float y = (owned[s][i].Y - minimum.Y) * scale + 0f;
                row[i] = new(RoundToSourceShort(x), RoundToSourceShort(y));
            }
            result[s] = row;
        }
        token.ThrowIfCancellationRequested();
        return new(owned, result, cumulative, total, minimum, maximum, scale, profile);
    }
    private static short RoundToSourceShort(float value)
    {
        //00FB90 uses <=0 then subtract0.5, otherwise add0.5, CVTT, short store.
        //This portable subset refuses out-of-short results rather than wrapping.
        float rounded = value <= 0f ? value - 0.5f : value + 0.5f;
        if (!float.IsFinite(rounded) || rounded < short.MinValue || rounded >= 32768f)
            throw new NotSupportedException("Normalized coordinate outside signed-short subset.");
        return checked((short)(int)rounded);
    }
    public Vector2[][] CopyOriginalStrokes(CancellationToken token = default)
        => Copy(original, token);
    public LegacyInkPoint[][] CopyNormalizedStrokes(CancellationToken token = default)
        => Copy(normalized, token);
    public int[] CopyCumulativeEnds(CancellationToken token = default)
    { token.ThrowIfCancellationRequested(); return (int[])ends.Clone(); }
    private static T[][] Copy<T>(T[][] rows, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); var result = new T[rows.Length][];
        for (int i = 0; i < rows.Length; i++)
        { token.ThrowIfCancellationRequested(); result[i] = (T[])rows[i].Clone(); }
        token.ThrowIfCancellationRequested(); return result;
    }
}
