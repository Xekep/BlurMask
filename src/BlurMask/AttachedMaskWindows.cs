using Avalonia.Controls;

namespace BlurMask;

internal static class AttachedMaskWindows
{
    private static readonly Dictionary<nuint, nuint> Targets = [];
    internal static nuint Id(Window mask) => mask.TryGetPlatformHandle() is { } handle
        ? OperatingSystem.IsMacOS() ? MacWindowCatalog.WindowNumber(handle.Handle) : (nuint)handle.Handle : 0;
    internal static void Register(Window mask, nuint target) { var id = Id(mask); if (id != 0) Targets[id] = target; }
    internal static void Remove(Window mask) => Targets.Remove(Id(mask));
    internal static bool SharesTarget(nuint id, nuint target) => Targets.TryGetValue(id, out var owner) && owner == target;
    internal static bool IsAbove(IReadOnlyList<nuint> bottomToTop, nuint mask, nuint target)
    {
        bool found = false;
        foreach (var id in bottomToTop)
        {
            if (id == target) { found = true; continue; }
            if (!found) continue;
            if (id == mask) return true;
            if (!SharesTarget(id, target)) return false;
        }
        return false;
    }
}
