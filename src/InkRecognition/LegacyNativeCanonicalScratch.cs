using System.Buffers;
using System.Text;

namespace InkRecognition;

/// <summary>
/// Engine048190/04ba10/04b870 ->04aa10 canonical scratch for the pinned default Unicode/UTF8 services.
/// Ordinary0491a0 does not write this scratch back without a successful circled-marker replacement.
/// </summary>
public static class LegacyNativeCanonicalScratch
{
    /// <summary>Input is already decoded by the caller's actual input iterator; this method does not remove a leading BOM.</summary>
    public static int[] PrepareDecodedCodePoints(ReadOnlySpan<int> input, LegacyNativeUnicodeNormalizationData model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model); cancellationToken.ThrowIfCancellationRequested();
        if (input.Length > 65536) throw new ArgumentOutOfRangeException(nameof(input));
        foreach (int cp in input) if ((uint)cp > 0x10ffff) throw new ArgumentException("Bounded native numeric code-point input required.");
        var decomposed = new List<int>(input.Length); int boundary = 0, visits = 0;
        foreach (int cp in input)
        {
            Visit();
            if (model.ReadCombiningClass(cp) == 0) { boundary = decomposed.Count; AppendPlain(cp, 0); }
            else _ = InsertOrdered(cp, boundary, 0);
        }
        var result = new List<int>(decomposed.Count); int starter = -1, previousClass = -1;
        foreach (int cp in decomposed)
        {
            Visit(); int currentClass = model.ReadCombiningClass(cp);
            if (starter >= 0 && currentClass > previousClass && model.TryCompose(result[starter], cp, out int combined))
                result[starter] = combined;
            else
            {
                result.Add(cp);
                if (currentClass == 0) { starter = result.Count - 1; previousClass = -1; }
                else previousClass = currentClass;
            }
        }
        return result.ToArray();

        void Visit()
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (++visits > 1_048_576) throw new ArgumentOutOfRangeException(nameof(input), "Portable canonical scratch work budget exceeded.");
        }
        void GuardDepth(int depth) { if (depth > 32) throw new InvalidDataException("Bounded canonical decomposition recursion exceeded."); }
        void AppendPlain(int cp, int depth)
        {
            Visit(); GuardDepth(depth);
            if (model.TryReadDirectCanonicalDecomposition(cp, out var parts))
                foreach (int part in parts) AppendPlain(part, depth + 1);
            else
            {
                GuardOutput(); decomposed.Add(EncodeDefaultScalar(cp));
            }
        }
        int InsertOrdered(int cp, int hint, int depth)
        {
            Visit(); GuardDepth(depth);
            if (model.TryReadDirectCanonicalDecomposition(cp, out var parts))
            {
                foreach (int part in parts) hint = InsertOrdered(part, hint, depth + 1);
                return hint;
            }
            int insertion = hint, previousReadClass = 256, currentClass = model.ReadCombiningClass(cp);
            //04b870 scans forward from a COPY of the last original class0 boundary.
            //The outer048190 boundary does not advance after a nonstarter insertion.
            for (int scan = hint; scan < decomposed.Count; scan++)
            {
                Visit(); int existingClass = model.ReadCombiningClass(decomposed[scan]);
                if (existingClass <= currentClass || existingClass == previousReadClass)
                { insertion = scan + 1; previousReadClass = existingClass; }
            }
            GuardOutput(); decomposed.Insert(insertion, EncodeDefaultScalar(cp));
            return insertion + 1;
        }
        void GuardOutput()
        {
            if (decomposed.Count >= 262144) throw new ArgumentOutOfRangeException(nameof(input), "Portable canonical scratch scalar budget exceeded.");
        }
    }

    /// <summary>
    /// Actual builtin UTF8 initializer094500 skips an initial BOM;094960 writes surrogate/FFFE/FFFF values as default1A.
    /// This checked adapter supports valid Unicode UTF8, rather than the native extended malformed/5-6-byte codec.
    /// </summary>
    public static byte[] PrepareUtf8Scratch(ReadOnlySpan<byte> utf8, LegacyNativeUnicodeNormalizationData model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model); cancellationToken.ThrowIfCancellationRequested();
        if (utf8.Length > 1_048_576) throw new ArgumentOutOfRangeException(nameof(utf8));
        if (utf8.StartsWith("\ufeff"u8)) utf8 = utf8[3..];
        var points = new List<int>();
        while (!utf8.IsEmpty)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Rune.DecodeFromUtf8(utf8, out Rune rune, out int consumed) != OperationStatus.Done)
                throw new InvalidDataException("Checked scratch input requires valid Unicode UTF8.");
            if (points.Count >= 65536) throw new ArgumentOutOfRangeException(nameof(utf8));
            points.Add(rune.Value); utf8 = utf8[consumed..];
        }
        int[] prepared = PrepareDecodedCodePoints(points.ToArray(), model, cancellationToken);
        using var output = new MemoryStream(); Span<byte> encoded = stackalloc byte[4];
        foreach (int cp in prepared)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int count = new Rune(cp).EncodeToUtf8(encoded);
            if (output.Length + count > 1_048_576) throw new ArgumentOutOfRangeException(nameof(utf8));
            output.Write(encoded[..count]);
        }
        return output.ToArray();
    }

    private static int EncodeDefaultScalar(int cp) => cp is >= 0xd800 and <= 0xdfff or 0xfffe or 0xffff ? 0x1a : cp;
}
