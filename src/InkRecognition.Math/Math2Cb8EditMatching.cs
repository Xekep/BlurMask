using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Math2Cb8EditMatching.Probe")]

namespace InkRecognition.Math;

// Prepared semantic operands only. They confer no DOM, Ink or selection ownership.
internal readonly record struct Math2Cb8PendingEdit(long TemporaryId, object? Name, object? SourceRange);

// A future owning adapter must provide conversion/parser and lookups from the
// same retained CB8/Ink/range services. There is deliberately no default parser
// or fabricated empty selection in this decision kernel.
internal interface IMath2Cb8EditMatchServices
{
    int CompareName(object? left, object? right);
    object? AttributeText(object? value);
    bool TryParseTag(object? text, out long value);
    object? InkRange(long selectionId);
    object? InkName(long selectionId);
    int RangeCount(object sourceRange);
    bool RangesEqual(object left, object right);
}

/// <summary>Document032800's ordered edit-match decision. Internal prepared
/// kernel; does not create a CB8 owner, mutate pending tags or accept a result.</summary>
internal static class Math2Cb8EditMatching
{
    internal static bool Matches(int editKind, object? attributeName, object? tagName,
        object? oldValue, object? newValue, ReadOnlySpan<Math2Cb8PendingEdit> pending,
        IMath2Cb8EditMatchServices services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);
        cancellationToken.ThrowIfCancellationRequested();
        if (editKind != 7 || attributeName is null || services.CompareName(attributeName, tagName) != 0)
            return false;

        // Native converts BOTH variants before testing the old parser result.
        var oldText = services.AttributeText(oldValue);
        var newText = services.AttributeText(newValue);
        if (!services.TryParseTag(oldText, out long oldId) ||
            !services.TryParseTag(newText, out long newId) || newId >= 0)
            return false;

        long temporaryId = unchecked(-newId); // Native x64 NEG wraps INT64_MIN.
        for (int i = 0; i < pending.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ref readonly var entry = ref pending[i];
            if (entry.TemporaryId != temporaryId) continue;
            var oldRange = services.InkRange(oldId);
            if (oldRange is null) return false;
            var oldName = services.InkName(oldId);
            if (oldName is null || services.CompareName(oldName, entry.Name) != 0) return false;
            // First matching ID wins even when its names/ranges do not match.
            return entry.SourceRange is null ? services.RangeCount(oldRange) == 0 :
                services.RangesEqual(oldRange, entry.SourceRange);
        }
        return false;
    }
}
