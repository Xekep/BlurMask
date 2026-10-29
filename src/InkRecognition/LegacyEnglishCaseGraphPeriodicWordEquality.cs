using System.Buffers.Binary;
namespace InkRecognition;
internal static class LegacyEnglishPeriodicWordEquality
{
    internal static bool Compare(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,LegacyNativeGlyphRecordPool actualPool,
        ReadOnlySpan<byte> leftWord120, ReadOnlySpan<byte> rightWord120,
        Span<byte> leftCallerStorage4, Span<byte> rightCallerStorage4,
        ushort actualIgnoredPrimaryLabel, Func<ushort, ushort>? actualPrimaryMapper2360,
        Func<string, ushort>? actualControlLookup, CancellationToken token)
    {
        stage.Owner.RequireGraphPeriodicEquality(stage,actualPool,token);ArgumentNullException.ThrowIfNull(actualPool); token.ThrowIfCancellationRequested();
        if (leftWord120.Length != 120 || rightWord120.Length != 120 ||
            leftCallerStorage4.Length != 4 || rightCallerStorage4.Length != 4 ||
            leftCallerStorage4.Overlaps(rightCallerStorage4) ||
            leftWord120.Overlaps(leftCallerStorage4) || leftWord120.Overlaps(rightCallerStorage4) ||
            rightWord120.Overlaps(leftCallerStorage4) || rightWord120.Overlaps(rightCallerStorage4))
            throw new ArgumentException("Complete WORD120 and separate four-byte scalar scratch required.");
        if (((U16(leftWord120, 0x32) ^ U16(rightWord120, 0x32)) & 0x300) != 0) return false;
        ulong left = U64(leftWord120, 8), right = U64(rightWord120, 8);
        int work = 0;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            if (left == ulong.MaxValue && right == ulong.MaxValue) return true;
            byte[]? a = null, b = null;
            bool skippedLeft = false;
            //Preserve the native nested skip order: a right skipped row can
            //cause the held left row to be read again before the outer retry.
            while (true)
            {
                if (left != ulong.MaxValue)
                {
                    a = Read(left);
                    if ((a[6] & 2) != 0) { left = U64(a, 0x20); skippedLeft = true; }
                }
                if (right != ulong.MaxValue)
                {
                    b = Read(right);
                    if ((b[6] & 2) != 0) { right = U64(b, 0x20); continue; }
                }
                break;
            }
            if (skippedLeft) continue;
            if (left == ulong.MaxValue) return right == ulong.MaxValue;
            if (right == ulong.MaxValue) return false;
            LegacyNativeGlyphUtf8Tokens.ReadScalar(new(U16(a!, 4), U16(a!, 8)),
                leftCallerStorage4, actualControlLookup, token);
            LegacyNativeGlyphUtf8Tokens.ReadScalar(new(U16(b!, 4), U16(b!, 8)),
                rightCallerStorage4, actualControlLookup, token);
            ushort al = U16(leftCallerStorage4, 0), bl = U16(rightCallerStorage4, 0);
            if (leftCallerStorage4[2] == 0 && actualPrimaryMapper2360 is not null) al = actualPrimaryMapper2360(al);
            if (rightCallerStorage4[2] == 0 && actualPrimaryMapper2360 is not null) bl = actualPrimaryMapper2360(bl);
            token.ThrowIfCancellationRequested();
            if (al == actualIgnoredPrimaryLabel) left = U64(a!, 0x20);
            else if (bl == actualIgnoredPrimaryLabel) right = U64(b!, 0x20);
            else
            {
                if (al != bl || leftCallerStorage4[2] != rightCallerStorage4[2]) return false;
                left = U64(a!, 0x20); right = U64(b!, 0x20);
            }
        }

        byte[] Read(ulong reference)
        {
            token.ThrowIfCancellationRequested();
            if (++work > 131072) throw new NotSupportedException("Portable linked-WORD comparison work budget exceeded; cycles are unsupported.");
            stage.Owner.RequireGraphPeriodicEquality(stage,actualPool,token);
            stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,40);stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,40);stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,64);
            return actualPool.CopyRecord(reference,token);
        }
    }
    private static ushort U16(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);
    private static ulong U64(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(bytes[offset..]);
}
