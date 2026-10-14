using System.Buffers;
using System.Text;

namespace InkRecognition;

/// <summary>The authoritative021000 fields of a fresh40-byte text-source record. Source endpoints/accepted winner are separate.</summary>
public readonly record struct LegacyNativePublishedTextGlyph(uint Flags, LegacyNativeInternedUtf8 Text, uint ByteCount);

/// <summary>Actual2DA+118 builtin3E→021000→019030/018b40 ordinary default publication, including first-cluster tailoring.</summary>
public static class LegacyNativeTextGlyphPublication
{
    public static LegacyNativePublishedTextGlyph Publish(LegacyNativeUtf8StringPool actualSourceStringOwner,
        ushort actualGlyphFlags, ReadOnlySpan<byte> actualEmittedUtf8, LegacyNativeTextPublicationData properties,
        LegacyNativeUnicodeNormalizationData combiningClasses, uint existingFlags = 0, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actualSourceStringOwner); ArgumentNullException.ThrowIfNull(properties);
        ArgumentNullException.ThrowIfNull(combiningClasses);
        var scalarValues = Decode(actualEmittedUtf8, cancellationToken);
        uint flags = (existingFlags & ~0x3ffu) | (uint)(actualGlyphFlags & 0x3ff);
        int count = FirstCluster(scalarValues, properties, combiningClasses, cancellationToken);
        bool special = count == 0;
        if (count > 0)
        {
            int first = scalarValues[0], script = properties.ReadScript(first);
            if (script is >= 7 and <= 9) flags |= 0x40000;
            else if (script == 10) flags |= 0x80000;
            else if (first is >= 0x3000 and <= 0x303f) flags |= 0x40000;
            if (count == 2 && first == 13 && scalarValues[1] == 10) { flags |= 0x2000; special = true; }
            else if (count == 1)
            {
                uint bit = first switch
                {
                    13 or 10 or 0x2028 or 0x2029 => 0x2000u,
                    >= 9 and <= 13 or 0x20 or 0xa0 => 0x1000u,
                    0xad => 0x4000u, 0x200c => 0x8000u,
                    0x61c or 0x200e or 0x200f or >= 0x202a and <= 0x202e or >= 0x2066 and <= 0x2069 => 0x20000u,
                    _ => 0
                };
                if (bit != 0) { flags |= bit; special = true; }
            }
        }
        if (!special) flags = (flags & ~0x10000u) | (properties.ReadLiteralClass(actualEmittedUtf8) < 0 ? 0 : 0x10000u);
        var interned = actualSourceStringOwner.Intern(actualEmittedUtf8, cancellationToken);
        return new(flags, interned, (uint)actualEmittedUtf8.Length);
    }

    /// <summary>Engine05fa80/05faf0 default tailored first-cluster scalar count; unlike output capacity2, cardinality is unbounded.</summary>
    public static int ReadFirstClusterScalarCount(ReadOnlySpan<byte> utf8, LegacyNativeTextPublicationData properties,
        LegacyNativeUnicodeNormalizationData combiningClasses, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(properties); ArgumentNullException.ThrowIfNull(combiningClasses);
        return FirstCluster(Decode(utf8, cancellationToken), properties, combiningClasses, cancellationToken);
    }

    private static int[] Decode(ReadOnlySpan<byte> bytes, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); if (bytes.Length > 1_048_576) throw new ArgumentOutOfRangeException(nameof(bytes));
        // Default charset5 constructor094500 consumes one initial UTF8 BOM. The interned interval remains untouched.
        if (bytes.StartsWith(new byte[] { 0xef, 0xbb, 0xbf })) bytes = bytes[3..];
        var scalars = new List<int>();
        while (!bytes.IsEmpty)
        {
            token.ThrowIfCancellationRequested();
            if (Rune.DecodeFromUtf8(bytes, out var rune, out int used) != OperationStatus.Done)
                throw new NotSupportedException("Publication scope requires valid Unicode UTF8; native malformed/legacy5-6-byte decoding remains separate.");
            if (scalars.Count == 65536) throw new ArgumentOutOfRangeException(nameof(bytes), "Scalar budget exceeded.");
            scalars.Add(rune.Value); bytes = bytes[used..];
        }
        return scalars.ToArray();
    }

    private static int FirstCluster(int[] values, LegacyNativeTextPublicationData data,
        LegacyNativeUnicodeNormalizationData ccc, CancellationToken token)
    {
        if (values.Length == 0) return 0;
        int first = values[0], previous = first, beforePrevious = -1, property = data.ReadGraphemeProperty(first);
        bool joinedEligibleZwj = false; int count = 1;
        for (int index = 1; index < values.Length; index++)
        {
            token.ThrowIfCancellationRequested(); int next = values[index], nextProperty = data.ReadGraphemeProperty(next);
            bool join;
            if (((property & 1) != 0 && (nextProperty & 2) != 0) || previous == 0x83 || next == 0x83) join = true;
            else if (((property | nextProperty) & 7) != 0) break;
            else if (((property & 0x10) != 0 && (nextProperty & 0x1b0) != 0) ||
                     ((property & 0xa0) != 0 && (nextProperty & 0x60) != 0) ||
                     ((property & 0x140) != 0 && (nextProperty & 0x40) != 0)) join = true;
            else if (next == 0x200c) break;
            else if (next == 0x200d)
            {
                if (!joinedEligibleZwj && !data.InJoinerRange(first)) break;
                join = true;
            }
            else if (next == 0x651) break;
            else if ((nextProperty & 8) != 0) { if (next == 0xe33) break; join = true; }
            else if ((property & 0x200) != 0) join = true;
            else
            {
                join = false;
                if (previous == 0x200d && data.InJoinerRange(next)) { joinedEligibleZwj = true; first = next; join = true; }
                if (!join && count == 1 && previous is >= 0x1f1e6 and <= 0x1f1ff && next is >= 0x1f1e6 and <= 0x1f1ff) join = true;
                if (!join && previous == 0x644 && (next is 0x622 or 0x623 or 0x625 or 0x627 or 0x671)) join = true;
                if (!join && property == 8 && ccc.ReadCombiningClass(previous) == 9 && beforePrevious >= 0)
                {
                    int script = data.ReadScript(beforePrevious);
                    join = script is 0x16 or 0x1b or 0x35 && data.ReadScript(next) == script;
                }
                if (!join) break;
            }
            if (join) { beforePrevious = previous; previous = next; property = nextProperty; count++; }
        }
        return count;
    }
}
