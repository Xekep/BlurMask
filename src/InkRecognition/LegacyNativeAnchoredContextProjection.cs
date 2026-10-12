namespace InkRecognition;

public sealed record LegacyNativeAnchoredContextTail(ulong ReferenceNode, ulong ReferenceParent,
    ulong MatchedNode, ReadOnlyMemory<ushort> KeysInParentOrder);

/// <summary>Literal211eb0 reference-pair traversal at21222a..212278. This
/// uses actual same-factory identities and retains the current path's tail;
/// it does not replace the native pair stepping with a generic ancestor query.</summary>
public static class LegacyNativeAnchoredContextProjection
{
    public static LegacyNativeAnchoredContextTail Extract(LegacySymbolContextFactory contexts,
        ulong currentMappedContext, ulong referenceMappedContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        cancellationToken.ThrowIfCancellationRequested();
        if (currentMappedContext != 0) _ = contexts.ReadNode(currentMappedContext);
        var seenReferences = new HashSet<ulong>();
        ulong reference = referenceMappedContext;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Native dereferences reference00 before the scan. A null reference
            // here violates its input invariant; never invent a zero-node read.
            if (reference == 0 || !seenReferences.Add(reference) || seenReferences.Count > 4096)
                throw new NotSupportedException("Reference-pair traversal requires a nonzero finite native lineage.");
            ulong referenceParent = contexts.ReadNode(reference).ParentLink;
            var tail = new List<ushort>();
            var seenCurrent = new HashSet<ulong>();
            ulong current = currentMappedContext;
            while (current != reference && current != referenceParent)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (current == 0) break;
                if (!seenCurrent.Add(current) || tail.Count == 4096)
                    throw new NotSupportedException("Bounded current symbol lineage exceeds4096 or is cyclic.");
                var node = contexts.ReadNode(current);
                tail.Add(node.Symbol);
                current = node.ParentLink;
            }
            // ASM compares the null scan result too, so a zero referenceParent
            // is a valid match. This comparison was omitted in the C export.
            if (current == reference || current == referenceParent)
                return new(reference, referenceParent, current, tail.ToArray());
            reference = contexts.ReadNode(referenceParent).ParentLink;
        }
    }

    /// <summary>Direct0918d0 replay, unlike1f7e30/1f7ff0 control composition.
    /// Parent-order keys are replayed in reverse without mapping or control folds.</summary>
    public static ulong Replay(LegacySymbolContextFactory contexts, ulong prefixIdentity,
        ReadOnlySpan<ushort> keysInParentOrder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        cancellationToken.ThrowIfCancellationRequested();
        if (keysInParentOrder.Length > 4096)
            throw new NotSupportedException("Bounded retained context tail exceeds4096 symbols.");
        if (prefixIdentity != 0) _ = contexts.ReadNode(prefixIdentity);
        ulong current = prefixIdentity;
        for (int index = keysInParentOrder.Length - 1; index >= 0; index--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var resolved = current == 0 ? contexts.ResolveInitial(keysInParentOrder[index], cancellationToken)
                : contexts.ResolveChild(current, keysInParentOrder[index], cancellationToken);
            if (resolved.Created && current != 0) contexts.LinkCreatedContext(resolved.Identity, current);
            current = resolved.Identity;
        }
        return current;
    }
}
