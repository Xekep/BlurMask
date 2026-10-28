namespace InkRecognition.Math;

// Opaque operands supplied by the SAME live CB8 owner. This record is not an
// ownership certificate, a selected result, or a public acceptance API.
internal readonly record struct Math2DomCb8Pending(long TemporaryId, object Name, object? Selection);

internal interface IMath2DomCb8Services
{
    int CompareName(object left, object right);
    object? FindOldSelection(long id);
    object? FindOldName(long id);
    int CountRanges(object selection);
    bool EqualRanges(object oldSelection, object pendingSelection);
}

internal enum Math2DomCb8Stage { ConvertOld, ConvertNew, ParseOld, ParseNew }

// Document032800 semantic kernel. No production nonfresh owner is yet wired;
// a true result here does NOT authorize DOM edits, IDs or recognition acceptance.
internal static class Math2DomCb8EditMatcher
{
    internal static bool Matches(byte operation, byte envelopeKind, object? nameAt10, object? nameAt30,
        object tagName, Math2DomValueView oldValue, Math2DomValueView newValue,
        IReadOnlyList<Math2DomCb8Pending> pending, IMath2DomCb8Services services,
        Action<Math2DomCb8Stage>? observe = null, IMath2DomFloatingFormatter? floating = null,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(pending);
        object? name = envelopeKind switch { 0 => nameAt10, 3 => nameAt30, _ => null };
        if (operation != 7 || name is null || services.CompareName(name, tagName) != 0) return false;

        Span<byte> oldScratch = stackalloc byte[50], newScratch = stackalloc byte[50];
        observe?.Invoke(Math2DomCb8Stage.ConvertOld);
        var oldText = Math2DomValueConversion.Text(oldValue, oldScratch, floating);
        observe?.Invoke(Math2DomCb8Stage.ConvertNew);
        var newText = Math2DomValueConversion.Text(newValue, newScratch, floating);
        // BOTH conversions precede the first parse, even for malformed old text.
        observe?.Invoke(Math2DomCb8Stage.ParseOld);
        if (!ParseCString(oldText, out long oldId)) return false;
        observe?.Invoke(Math2DomCb8Stage.ParseNew);
        if (!ParseCString(newText, out long newId) || newId >= 0) return false;

        long temporaryId = unchecked(-newId);
        for (int at = 0; at < pending.Count; at++)
        {
            token.ThrowIfCancellationRequested();
            var entry = pending[at];
            if (entry.TemporaryId != temporaryId) continue;
            // First matching ID is decisive, including a failed name/range match.
            object? oldSelection = services.FindOldSelection(oldId);
            if (oldSelection is null) return false;
            object? oldName = services.FindOldName(oldId);
            if (oldName is null || services.CompareName(oldName, entry.Name) != 0) return false;
            return entry.Selection is null ? services.CountRanges(oldSelection) == 0
                : services.EqualRanges(oldSelection, entry.Selection);
        }
        return false;
    }

    private static bool ParseCString(Math2DomTextView text, out long value)
    {
        // Native supplies endPointer=null: declared DOM length is NOT an end
        // pointer here. Read the covered storage, stopping on the first nondigit.
        // Never replace an unknown native read beyond that storage by success.
        var bytes = text.Storage;
        if (!text.HasPointer || bytes.IsEmpty || (bytes.Length == 1 && bytes[0] is (byte)'+' or (byte)'-'))
            throw new NotSupportedException("CB8 parsing requires covered native string storage.");
        value = 0;
        int end = Math2DomStringService.ParseDecimal(bytes, 0, ref value);
        if (end == bytes.Length)
            throw new NotSupportedException("CB8 numeric prefix has no covered terminating byte.");
        return end >= 0;
    }
}
