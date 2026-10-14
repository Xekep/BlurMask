using System.Buffers;
using System.Text;

namespace InkRecognition;

/// <summary>
/// Complete bounded ordinary273 glyph UTF8 emission, including0491a0's proven no-circled-start passthrough.
/// The actual primary/control/alternate owners remain distinct. Accepted021000 text-source publication is separate.
/// </summary>
public static class LegacyNativeOrdinaryGlyphUtf8
{
    public static byte[] Emit(LegacyNativeGlyphSymbolDescriptor descriptor, LegacyInkAlphabet actualPrimaryAlphabet,
        LegacyNativeGlyphAlternateUtf8Table? alternateTable = null, Func<string, ushort>? actualControlLabelLookup = null,
        CancellationToken cancellationToken = default)
    {
        var tokens = LegacyNativeGlyphUtf8Tokens.Expand(descriptor, actualControlLabelLookup, cancellationToken);
        var bytes = LegacyNativeGlyphUtf8Tokens.EmitBeforeCanonicalComposition(tokens, actualPrimaryAlphabet, alternateTable,
            cancellationToken);
        ValidateOrdinary(bytes, cancellationToken);
        return bytes;
    }

    /// <summary>
    /// Engine0491a0 keeps the original interval when no F0000 start marker exists in its canonical scratch.
    /// The pinned canonical decomposition/composition tables cannot introduce that marker; Hangul cannot either.
    /// Returns an owned copy without normalizing, dropping BOMs, changing line endings or replacing noncharacters.
    /// </summary>
    public static byte[] CompleteWithoutCircledStart(ReadOnlySpan<byte> utf8,
        CancellationToken cancellationToken = default)
    {
        ValidateOrdinary(utf8, cancellationToken);
        return utf8.ToArray();
    }

    private static void ValidateOrdinary(ReadOnlySpan<byte> utf8, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (utf8.Length > 1_048_576) throw new ArgumentOutOfRangeException(nameof(utf8));
        while (!utf8.IsEmpty)
        {
            token.ThrowIfCancellationRequested();
            if (Rune.DecodeFromUtf8(utf8, out Rune value, out int consumed) != OperationStatus.Done)
                throw new InvalidDataException("Bounded ordinary emission requires valid Unicode UTF8; native legacy malformed/5-6-byte codec paths are separate.");
            if (value.Value == 0xf0000)
                throw new NotSupportedException("Actual circled-start marker requires the unfinished native circled helper, not framework normalization.");
            utf8 = utf8[consumed..];
        }
    }
}
