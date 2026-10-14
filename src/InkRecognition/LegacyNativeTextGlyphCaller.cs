using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeTextGlyphCallerResult(IReadOnlyList<LegacyNativePublishedTextGlyph> Records,
    ReadOnlyMemory<byte> RetainedSelectionRows, int AdjustedMarkerIndex);

/// <summary>Exact bounded fresh0213A0 caller: actual274 lookup, HeaderE filter, empty removal, smiley replacement and021000 publication.</summary>
public static class LegacyNativeTextGlyphCaller
{
    public static LegacyNativeTextGlyphCallerResult Produce(LegacyNativeGlyphRecordOwner actualGlyphOwner,
        LegacyInkAlphabet actualPrimaryAlphabet, ReadOnlySpan<byte> actualSerializedAlphabetHeader,
        ReadOnlySpan<byte> actualSelectionRows4, ulong actualReferenceBase, bool wrapperHeaderEFilter,
        int markerIndex, LegacyNativeUtf8StringPool actualSourceStringOwner,
        LegacyNativeTextPublicationData properties, LegacyNativeUnicodeNormalizationData combiningClasses,
        LegacyNativeGlyphAlternateUtf8Table? actualAlternateOwner = null,
        Func<string, ushort>? actualControlLabelLookup = null,
        Func<LegacyNativeOwnedGlyphRecord, byte[]>? actualFullGlyphEmitter = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actualGlyphOwner); ArgumentNullException.ThrowIfNull(actualPrimaryAlphabet);
        ArgumentNullException.ThrowIfNull(actualSourceStringOwner); ArgumentNullException.ThrowIfNull(properties);
        ArgumentNullException.ThrowIfNull(combiningClasses); cancellationToken.ThrowIfCancellationRequested();
        if (actualSerializedAlphabetHeader.Length != 16 ||
            BinaryPrimitives.ReadUInt16LittleEndian(actualSerializedAlphabetHeader[14..]) != actualPrimaryAlphabet.LabelCount ||
            actualSelectionRows4.Length % 4 != 0 || actualSelectionRows4.Length > 65536 * 4)
            throw new ArgumentException("Actual16-byte alphabet header and bounded complete4-byte selection rows required.");
        //053F20 forwards serialized header field3 to alphabet runtime+E. It is not inferred from a text-space lookup.
        ushort headerE = BinaryPrimitives.ReadUInt16LittleEndian(actualSerializedAlphabetHeader[6..]);
        if (headerE != ushort.MaxValue && headerE >= actualPrimaryAlphabet.LabelCount)
            throw new ArgumentException("HeaderE is outside the actual primary alphabet.");
        var records = new List<LegacyNativePublishedTextGlyph>(); using var retained = new MemoryStream();
        int currentIndex = 0, tokenWork = 0, totalBytes = 0;
        for (int sourceIndex = 0; sourceIndex < actualSelectionRows4.Length / 4; sourceIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested(); ReadOnlySpan<byte> selected = actualSelectionRows4.Slice(sourceIndex * 4, 4);
            ulong reference = actualReferenceBase | ((ulong)BinaryPrimitives.ReadUInt16LittleEndian(selected) << 40);
            var glyph = actualGlyphOwner.Resolve(reference); var descriptor = glyph.Symbol;
            bool removed = wrapperHeaderEFilter && (descriptor.KindFlags & 7) == 0 && descriptor.SymbolOrRunLength == headerE;
            if (!removed)
            {
                tokenWork = checked(tokenWork + LegacyNativeGlyphUtf8Tokens.ReadTokenCount(descriptor));
                if (tokenWork > 1_048_576) throw new ArgumentOutOfRangeException(nameof(actualSelectionRows4), "Caller token-work budget exceeded.");
                //Full273.virtual50 remains explicit for circled-start input until its native replacement stage is available.
                byte[] bytes = actualFullGlyphEmitter is null
                    ? LegacyNativeOrdinaryGlyphUtf8.Emit(descriptor, actualPrimaryAlphabet, actualAlternateOwner, actualControlLabelLookup, cancellationToken)
                    : (actualFullGlyphEmitter(glyph) ?? throw new InvalidDataException("Actual glyph emitter returned no buffer."));
                if (bytes.Length > 1_048_576 || bytes.Length > 8_388_608 - totalBytes)
                    throw new ArgumentOutOfRangeException(nameof(actualSelectionRows4), "Caller emitted-byte budget exceeded.");
                totalBytes += bytes.Length; removed = bytes.Length == 0;
                if (!removed)
                {
                    //The caller changes exactly one emitted2639/263A, not substrings or repeated/multi-symbol runs.
                    ReadOnlySpan<byte> published = bytes;
                    if (bytes.AsSpan().SequenceEqual("\u2639"u8)) published = "\U0001f641"u8;
                    else if (bytes.AsSpan().SequenceEqual("\u263a"u8)) published = "\U0001f642"u8;
                    var record = LegacyNativeTextGlyphPublication.Publish(actualSourceStringOwner, glyph.PublicationFlags,
                        published, properties, combiningClasses, cancellationToken: cancellationToken);
                    if (published.Length == 4 && published[0] == 0xf3 && published[1] == 0xb0 && published[2] == 0x80 && published[3] < 0x82)
                        record = record with { Flags = record.Flags | 0x100000 };
                    records.Add(record);
                }
            }
            if (removed) { if (currentIndex < markerIndex) markerIndex--; }
            else { retained.Write(selected); currentIndex++; }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(records.AsReadOnly(), retained.ToArray(), markerIndex);
    }
}
