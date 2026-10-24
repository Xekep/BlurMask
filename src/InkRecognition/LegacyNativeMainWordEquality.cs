using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>276.V60/13F970 linked-WORD equality. The explicit scalar scratch,
/// controller2360 mapper,24E0 label and control2A lookup remain caller-owned.
/// This is distinct from279.V28 alternative equality and from UTF8 equality.</summary>
public static class LegacyNativeMainWordEquality
{
    public static bool Compare(LegacyNativeGlyphRecordPool actualPool,
        ReadOnlySpan<byte> leftWord120, ReadOnlySpan<byte> rightWord120,
        Span<byte> leftCallerStorage4, Span<byte> rightCallerStorage4,
        ushort actualIgnoredPrimaryLabel, Func<ushort, ushort>? actualPrimaryMapper2360 = null,
        Func<string, ushort>? actualControlLookup = null, CancellationToken token = default)
        =>CompareCore(actualPool,leftWord120,rightWord120,leftCallerStorage4,rightCallerStorage4,actualIgnoredPrimaryLabel,actualPrimaryMapper2360,actualControlLookup,token,null);

    internal static bool CompareWithReadObserver(LegacyNativeGlyphRecordPool actualPool,
        ReadOnlySpan<byte> leftWord120, ReadOnlySpan<byte> rightWord120,
        Span<byte> leftCallerStorage4, Span<byte> rightCallerStorage4,
        ushort actualIgnoredPrimaryLabel, Func<ushort, ushort>? actualPrimaryMapper2360,
        Func<string, ushort>? actualControlLookup, CancellationToken token, Action<ulong,byte[]> observer)
        =>CompareCore(actualPool,leftWord120,rightWord120,leftCallerStorage4,rightCallerStorage4,actualIgnoredPrimaryLabel,actualPrimaryMapper2360,actualControlLookup,token,observer??throw new ArgumentNullException(nameof(observer)));

    private static bool CompareCore(LegacyNativeGlyphRecordPool actualPool,
        ReadOnlySpan<byte> leftWord120, ReadOnlySpan<byte> rightWord120,
        Span<byte> leftCallerStorage4, Span<byte> rightCallerStorage4,
        ushort actualIgnoredPrimaryLabel, Func<ushort, ushort>? actualPrimaryMapper2360,
        Func<string, ushort>? actualControlLookup, CancellationToken token, Action<ulong,byte[]>? observer)
    {
        ArgumentNullException.ThrowIfNull(actualPool); token.ThrowIfCancellationRequested();
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
            var record=actualPool.CopyRecord(reference, token);observer?.Invoke(reference,record);return record;
        }
    }
    private static ushort U16(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);
    private static ulong U64(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(bytes[offset..]);
}

/// <summary>Fresh no-property main controller binding of13F970. Both words
/// retain the same actual TOP274 pool; scalar scratch stays explicit. This
/// does not rank/prune its275 rows or complete TOP.V68.</summary>
public sealed class LegacyFreshMainWordEquality
{
    public LegacyFreshMainGlyphUtf8Emission Emission { get; }
    public LegacyFreshMainSingleLinePlainRecognitionResult RawResult => Emission.RawResult;
    public LegacyFreshMainPlainResultOwnership TopOwner => Emission.TopOwner;
    public ushort IgnoredPrimaryLabel { get; }
    private LegacyFreshMainWordEquality(LegacyFreshMainGlyphUtf8Emission emission, ushort ignored)
    { Emission = emission; IgnoredPrimaryLabel = ignored; }

    public static LegacyFreshMainWordEquality Prepare(LegacyFreshMainSingleLinePlainRecognitionResult raw,
        LegacyFreshMainGlyphUtf8Emission actualEmission, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(raw); ArgumentNullException.ThrowIfNull(actualEmission);
        token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(raw, actualEmission.RawResult) ||
            !ReferenceEquals(raw.Session.Ownership.TopOwner, actualEmission.TopOwner) ||
            raw.Session.Plan.SourcePolicy.HasAssociatedSource ||
            raw.Session.Plan.SourcePolicy.Controller.Configuration.Trace2368 != 0)
            throw new ArgumentException("Identical completed fresh raw main owner and native control/emission owner required.");
        //Fresh00C860 zero11/12 is retained by this sealed no-property source;
        //2144F0 installs identity1F8300 in2360. Secondary-count property does
        //not configure these bytes; other property/reuse sources are excluded.
        ushort ignored = actualEmission.PrimaryAlphabet.TryGetLabel("\u200d", out ushort label) ? label : ushort.MaxValue;
        token.ThrowIfCancellationRequested(); return new(actualEmission, ignored);
    }

    public bool Compare(LegacyNativePlainWordRecord left, LegacyNativePlainWordRecord right,
        Span<byte> leftCallerStorage4, Span<byte> rightCallerStorage4, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(left); ArgumentNullException.ThrowIfNull(right); token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(left.Pool, TopOwner.GlyphPool) || !ReferenceEquals(right.Pool, TopOwner.GlyphPool))
            throw new ArgumentException("Both linked WORDs must belong to this actual TOP glyph pool.");
        return CompareStorage120(left.CopyStorage(), right.CopyStorage(), leftCallerStorage4, rightCallerStorage4, token);
    }

    internal bool CompareStorage120(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right,
        Span<byte> leftScratch, Span<byte> rightScratch, CancellationToken token = default)
        => LegacyNativeMainWordEquality.Compare(TopOwner.GlyphPool, left, right, leftScratch, rightScratch,
            IgnoredPrimaryLabel, static symbol => symbol, text => Emission.ReadControlLabel(text, token), token);
}
