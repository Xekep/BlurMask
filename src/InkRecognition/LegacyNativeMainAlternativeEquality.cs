using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainAlternativeAssembly.Probe")]

namespace InkRecognition;

/// <summary>279.V28/1421B0 plain comparison over actual275 flat-reference
/// streams in279.Head/Next order. Not decoded-string equality. Detailed
/// auxiliary-vector comparison remains unsupported when that branch is reached.</summary>
public static class LegacyNativeMainAlternativeEquality
{
    public static bool Compare(LegacyNativeGlyphRecordPool actualPool,
        IReadOnlyList<LegacyNativePlainWordRecord> leftWords, IReadOnlyList<LegacyNativePlainWordRecord> rightWords,
        Span<byte> leftCallerStorage4, Span<byte> rightCallerStorage4,
        Func<string, ushort>? actualControlLookup = null, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualPool); token.ThrowIfCancellationRequested();
        if (leftCallerStorage4.Length != 4 || rightCallerStorage4.Length != 4 || leftCallerStorage4.Overlaps(rightCallerStorage4))
            throw new ArgumentException("Two separate complete native scalar scratch slots required.");
        var left = ReadWords(actualPool, leftWords, token);
        var right = ReadWords(actualPool, rightWords, token);
        int leftWord = 0, rightWord = 0, leftOrdinal = 0, rightOrdinal = 0, work = 0;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            while (leftWord < left.Length && leftOrdinal >= left[leftWord].References.Length) { leftWord++; leftOrdinal = 0; }
            while (rightWord < right.Length && rightOrdinal >= right[rightWord].References.Length) { rightWord++; rightOrdinal = 0; }
            //Native tests end-of-stream BEFORE the skippable-label branches.
            //An unmatched trailingFFFA is therefore not filtered away.
            if (leftWord == left.Length) { if (rightWord != right.Length) return false; break; }
            if (rightWord == right.Length) return false;
            if (++work > 1_048_576) throw new NotSupportedException("Portable alternative comparison work budget exceeded.");
            byte[] a = actualPool.CopyRecord(left[leftWord].References[leftOrdinal], token);
            byte[] b = actualPool.CopyRecord(right[rightWord].References[rightOrdinal], token);
            Read(a, leftCallerStorage4, actualControlLookup, token);
            Read(b, rightCallerStorage4, actualControlLookup, token);
            token.ThrowIfCancellationRequested();
            ushort al = BinaryPrimitives.ReadUInt16LittleEndian(leftCallerStorage4);
            ushort bl = BinaryPrimitives.ReadUInt16LittleEndian(rightCallerStorage4);
            if (left[leftWord].Field3c != -1 && al == 0xfffc) { leftOrdinal++; continue; }
            if (right[rightWord].Field3c != -1 && bl == 0xfffc) { rightOrdinal++; continue; }
            if (al == 0xfffa) { leftOrdinal++; continue; }
            if (bl == 0xfffa) { rightOrdinal++; continue; }
            if (leftCallerStorage4[2] != rightCallerStorage4[2] || al != bl || ((a[6] ^ b[6]) & 0x40) != 0)
                return false;
            leftOrdinal++; rightOrdinal++;
        }
        bool any = left.Any(w => w.ActiveAuxiliary) || right.Any(w => w.ActiveAuxiliary);
        if (!any) return true;
        if (left.Any(w => !w.ActiveAuxiliary) || right.Any(w => !w.ActiveAuxiliary)) return false;
        throw new NotSupportedException("Native all-active WORD54/58/60 auxiliary-vector comparison requires its actual detailed owner.");
    }

    private sealed record Word(ulong[] References, int Field3c, bool ActiveAuxiliary);
    private static Word[] ReadWords(LegacyNativeGlyphRecordPool pool, IReadOnlyList<LegacyNativePlainWordRecord> words,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(words);
        if (words.Count is < 1 or > 4096)
            throw new ArgumentException("Native comparator requires nonnull initial WORD heads and bounded lists.");
        var result = new Word[words.Count];
        for (int i = 0; i < words.Count; i++)
        {
            token.ThrowIfCancellationRequested(); var word = words[i]; ArgumentNullException.ThrowIfNull(word);
            if (!ReferenceEquals(word.Pool, pool)) throw new ArgumentException("Both alternatives must retain the same actual TOP glyph pool.");
            byte[] bytes = word.CopyStorage();
            if (bytes.Length != 120 || word.FlatReferences.Count > 4096 ||
                BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(0x28)) != word.FlatReferences.Count)
                throw new ArgumentException("Complete actual WORD120 and its native count/flat-reference vector required.");
            int field3c = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(0x3c));
            bool auxiliary = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(0x60)) != 0 &&
                BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(0x54)) >= 0;
            result[i] = new(word.FlatReferences.ToArray(), field3c, auxiliary);
        }
        return result;
    }
    private static void Read(byte[] row, Span<byte> scratch, Func<string, ushort>? lookup, CancellationToken token)
        => LegacyNativeGlyphUtf8Tokens.ReadScalar(new(BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(4)),
            BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(8))), scratch, lookup, token);
}
