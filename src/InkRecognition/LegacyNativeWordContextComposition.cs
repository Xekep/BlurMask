using System.Collections.Generic;

namespace InkRecognition;

/// <summary>Literal nontracing 1f7e30 / 1f7ff0 symbol-path concatenation over
/// the shared 0918d0 context index. Identities are portable IDs, not pointers.</summary>
public static class LegacyNativeWordContextComposition
{
    // Native 1804a7b60 - 4 * (current + previous * 6). Both symbols descend
    // from FFFC to FFF7. Positive3 replaces the previous symbol with FFF9.
    private static ReadOnlySpan<sbyte> ControlActions =>
    [
        -1, -2, -1, -2, 3, 3,
        -1, -1, -1, -2, 3, 3,
        -2, -2, -1, -2, 3, 3,
        -1, -1, -1, -1, -1, -1,
         3,  3,  3, -2, 3, 3,
         3,  3,  3, -2, 3, 3
    ];

    public static ulong CombineMapped(LegacySymbolContextFactory contexts, ulong prefixIdentity,
        ulong suffixIdentity, Func<ushort, ushort> mapSymbol2360,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapSymbol2360);
        return Combine(contexts, prefixIdentity, suffixIdentity, mapSymbol2360, cancellationToken);
    }

    public static ulong CombineRaw(LegacySymbolContextFactory contexts, ulong prefixIdentity,
        ulong suffixIdentity, CancellationToken cancellationToken = default)
        => Combine(contexts, prefixIdentity, suffixIdentity, null, cancellationToken);

    private static ulong Combine(LegacySymbolContextFactory contexts, ulong prefixIdentity,
        ulong suffixIdentity, Func<ushort, ushort>? mapSymbol, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        token.ThrowIfCancellationRequested();
        if (prefixIdentity != 0)
            _ = contexts.ReadNode(prefixIdentity);

        // Native recursion first visits suffix->parent. Capture that exact
        // lineage before inserting into the same live index. No numeric ID sort.
        var suffix = new List<ushort>();
        var visited = new HashSet<ulong>();
        for (ulong current = suffixIdentity; current != 0;)
        {
            token.ThrowIfCancellationRequested();
            if (suffix.Count == 4096)
                throw new NotSupportedException("Bounded symbol path exceeds 4096 nodes.");
            if (!visited.Add(current))
                throw new InvalidDataException("Symbol context parent lineage is cyclic.");
            var node = contexts.ReadNode(current);
            suffix.Add(node.Symbol);
            current = node.ParentLink;
        }

        ulong result = prefixIdentity;
        for (int index = suffix.Count - 1; index >= 0; index--)
        {
            token.ThrowIfCancellationRequested();
            ushort symbol = suffix[index];
            ushort outputSymbol = symbol;
            ulong parent = result;
            if (result == 0)
            {
                if (mapSymbol is not null)
                {
                    // Only mapped1f7e30 rejects these controls at an empty prefix.
                    if (symbol is >= 0xfff7 and <= 0xfffc)
                        continue;
                    outputSymbol = mapSymbol(symbol);
                }
            }
            else
            {
                if (symbol == 0xfffd)
                    continue;
                var previous = contexts.ReadNode(result);
                if (previous.Symbol is >= 0xfff7 and <= 0xfffc &&
                    symbol is >= 0xfff7 and <= 0xfffc)
                {
                    int action = ControlActions[(0xfffc - previous.Symbol) * 6 + 0xfffc - symbol];
                    if (action == -1)
                        continue;
                    parent = previous.ParentLink;
                    // -2 pops the old symbol, then inserts current unmodified.
                    // Other entries use native ushort(-(short)action - 4).
                    if (action != -2)
                        outputSymbol = unchecked((ushort)(-(short)action - 4));
                }
                else if (mapSymbol is not null)
                    outputSymbol = mapSymbol(symbol);
            }

            var resolved = parent == 0
                ? contexts.ResolveInitial(outputSymbol, token)
                : contexts.ResolveChild(parent, outputSymbol, token);
            result = resolved.Identity;
            if (resolved.Created && parent != 0)
                contexts.LinkCreatedContext(result, parent);
        }
        return result;
    }
}
