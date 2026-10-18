using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyOriginalSelectedPublication.Probe")]
namespace InkRecognition;

// Internal discriminated ownership, never a caller-created selected C20 or
// a cast from the original-field branch into the older one-INPUT context.
internal sealed class LegacyLibrarySelectedInputBinding
{
    internal LegacyFreshTextFieldInputContext? Legacy { get; }
    internal LegacyLibraryOriginalSelectedFieldContext? OriginalSelected { get; }
    internal object Identity => (object?)Legacy ?? OriginalSelected!;
    internal LegacyOwnedRawInkDocument Document => Legacy?.Document ?? OriginalSelected!.Document;
    internal long RegistryId => Legacy?.Input.RegistryId ?? OriginalSelected!.OriginalInput.RegistryId;
    internal long Next => Legacy?.NextRegistryId ?? OriginalSelected!.State.NextRegistryId;
    internal string ElementName => Legacy?.ElementName ?? "textField";
    internal string ElementNamespace => Legacy?.ElementNamespace ?? LegacyLibraryOriginalTextFieldBootstrap.PageNamespace;
    internal int ElementKind => Legacy?.NativeElementKind ?? 0;
    internal LegacyFreshTextFieldInputContext LegacyOrThrow => Legacy ??
        throw new InvalidOperationException("This route retains an original-bootstrap selected owner, not the older one-INPUT context.");
    private LegacyLibrarySelectedInputBinding(LegacyFreshTextFieldInputContext input) { Legacy = input; }
    private LegacyLibrarySelectedInputBinding(LegacyLibraryOriginalSelectedFieldContext input) { OriginalSelected = input; }
    internal static LegacyLibrarySelectedInputBinding FromLegacy(LegacyFreshTextFieldInputContext input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Parent is null || !ReferenceEquals(input.Input.Owner, input) || input.Input.InputSpanCount != 0 || input.Input.CopyRanges().Length == 0)
            throw new NotSupportedException("Actual full selected textField INPUT/C22 owner required.");
        return new(input);
    }
    internal static LegacyLibrarySelectedInputBinding FromOriginal(LegacyLibraryOriginalSelectedFieldContext input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.State.Succeeded || input.State.IsFaulted || input.HasNativeReaderContext || input.NativeHistoryEnabled ||
            !input.State.Registry.Any(e => e.RegistryId == input.OriginalInput.RegistryId && ReferenceEquals(e.OriginalEntry, input.OriginalInput)))
            throw new NotSupportedException("Complete same-original selected worker C20/BCD required.");
        return new(input);
    }
    internal LegacyNativeInkPositionRange[] CopyRanges() => Legacy?.Input.CopyRanges() ??
        OriginalSelected!.State.Registry.Single(e => e.RegistryId == RegistryId).CopyRanges();
    internal byte[] CopyDafInputRows32()
    {
        if (Legacy is not null) return Legacy.Input.CopyDafInputRows32();
        var s = OriginalSelected!.State;
        var input = s.Nodes.Single(n => n.Identity == s.SelectedInputIdentity);
        if (input.Name != "input" || input.Children.Count != 0)
            throw new NotSupportedException("Actual new selected INPUT without span children required.");
        return [];
    }
}
