using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyOriginalSelectedPublication.Probe")]
[assembly: InternalsVisibleTo("LegacyOriginalSelectedDocument.Probe")]
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
    internal long Maximum => Legacy?.MaximumRegistryId ?? OriginalSelected!.State.MaximumRegistryId;
    internal LegacyFreshRawInkCurrentView CurrentView => Legacy?.CurrentView ?? OriginalSelected!.CurrentView;
    internal LegacyLibrarySelectedFieldRegistryEntry? OriginalSelectedEntry => OriginalSelected?.State.Registry.Single(e => e.RegistryId == RegistryId);
    internal long[] CopyInitialRegistryIds() => Legacy is not null ? [Legacy.Input.RegistryId] :
        OriginalSelected!.State.Registry.Select(e => e.RegistryId).ToArray();
    internal LegacyNativeInkPositionRange[][] CopyInitialRegistryRanges() => Legacy is not null ? [Legacy.Input.CopyRanges()] :
        OriginalSelected!.State.Registry.Select(e => e.CopyRanges()).ToArray();
    internal LegacyLibrarySelectedInputIdentityExport ExportIdentity(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (Legacy is not null)
        {
            var export = Legacy.ExportInputIdentityTransforms(Legacy.Input, token);
            if (!ReferenceEquals(export.Owner, Legacy) || !ReferenceEquals(export.Input, Legacy.Input))
                throw new ArgumentException("Exact selected legacy exporter required.");
            return new(this, export, null, export.Geometry);
        }
        var selected = OriginalSelected!; var entry = OriginalSelectedEntry!;
        if (!selected.State.Succeeded || selected.State.IsFaulted || selected.NativeHistoryEnabled ||
            !ReferenceEquals(entry.Owner, selected) || !ReferenceEquals(entry.OriginalEntry, selected.OriginalInput))
            throw new ArgumentException("Exact current selected source-tag/C22 required.");
        return new(this, null, entry, selected.ExportInputGeometry(entry, token));
    }
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
    internal LegacyNativeInkPositionRange[] CopyRanges() => Legacy?.Input.CopyRanges() ?? OriginalSelectedEntry!.CopyRanges();
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

internal sealed record LegacyLibrarySelectedInputIdentityExport(
    LegacyLibrarySelectedInputBinding Binding, LegacyOwnedTextFieldInputIdentityExport? Legacy,
    LegacyLibrarySelectedFieldRegistryEntry? OriginalSelectedInput, LegacyOwnedRawInkIdentityExport Geometry);
