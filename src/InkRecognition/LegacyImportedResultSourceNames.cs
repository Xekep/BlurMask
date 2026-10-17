using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("LegacyImportedSourceNames.Probe")]

namespace InkRecognition;

/// <summary>Detached13C1D0 source-name bytes, bound to one completed imported
/// result. These are dictionary resource identifiers, not document text.
/// Primary/control/alternate glyph alphabets are not consulted.</summary>
public sealed class LegacyImportedResultSourceNames
{
    private readonly LegacyDetachedSourceNameTable table;
    public LegacyFreshMainGlobalControlImport Owner { get; }
    public int NativeSourceCount => table.SourceCount;
    public long NativeAllocationByteCount => table.NativeAllocationByteCount;
    private LegacyImportedResultSourceNames(LegacyFreshMainGlobalControlImport owner, LegacyDetachedSourceNameTable table)
    { Owner = owner; this.table = table; }

    public static LegacyImportedResultSourceNames Capture(LegacyFreshMainGlobalControlImport owner,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(owner);
        owner.ValidateCompletedProjection(token);
        var policy = owner.CommonSourcePolicy;
        var symbols = policy.Owner.Symbols;
        //236820 adds implicit source0;236830 enumerates the selected descriptors.
        //20D8F0 installs this count in240.22D8. This is NOT selected.Count alone.
        if (policy.HasAssociatedSource || symbols.SelectedSources.Count != 1 ||
            symbols.CopySourceHeaders().Length != 2 ||
            symbols.ResourcePrefix.Identifier != symbols.Descriptor.Identifier ||
            symbols.ResourcePrefix.Identifier != "ru_RU-lk-text.lite.lex")
            throw new NotSupportedException("The actual fresh implicit0/one selected Russian source is required.");
        byte[] name = Encoding.UTF8.GetBytes(symbols.ResourcePrefix.Identifier);
        //The pinned133 loader already validated the resource's NUL-terminated
        //identifier; ASCII here makes its string roundtrip byte-exact.
        if (name.Any(b => b is 0 or > 127))
            throw new NotSupportedException("The pinned ASCII source identifier is required.");
        var table = LegacyDetachedSourceNameTable.Copy(symbols.SelectedSources.Count + 1,
            id => id == 1 ? name : id == -1 ? null : throw new ArgumentOutOfRangeException(nameof(id)), token);
        owner.ValidateCompletedProjection(token);
        return new(owner, table);
    }

    /// <summary>1458F0 detached WORD lookup. NULL is distinct from a present
    /// empty name. The native -2 path skips its optional length output; this
    /// byte-returning API does not model that ABI output parameter.</summary>
    public byte[]? ReadWord(LegacyImportedControlWord word, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(word); token.ThrowIfCancellationRequested();
        if (!Owner.Words.Any(candidate => ReferenceEquals(candidate, word)))
            throw new ArgumentException("The exact imported WORD owner is required.", nameof(word));
        short id = BinaryPrimitives.ReadInt16LittleEndian(word.CopyStorage120().AsSpan(0x30));
        return table.Read(id, token);
    }

    /// <summary>Semantic offsets into CopyWrittenPayload, -1 for native NULL.
    /// These are not native pointers. Slot0 remains NULL.</summary>
    public int[] CopySlotOffsets() => table.CopySlotOffsets();
    /// <summary>Only initialized payload bytes, including the NUL consumed by
    /// each NULL slot. The final native allocation byte is unwritten and omitted.</summary>
    public byte[] CopyWrittenPayload() => table.CopyWrittenPayload();
}
