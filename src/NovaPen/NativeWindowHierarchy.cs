using Avalonia;

namespace NovaPen;

// The limit bounds foreign-window work, not the supported nesting depth.
// Exhaustion/cycles fail selection instead of silently selecting a wider ancestor.
internal sealed class WindowTraversalBudget
{
    internal const int MaximumWindows = 4096;
    private int _remaining = MaximumWindows;
    internal bool Spend()
    {
        if (_remaining == 0) return false;
        _remaining--;
        return true;
    }
}

internal readonly record struct NativeChildHit(bool Valid, TrackedWindow? Child)
{
    internal static NativeChildHit End => new(true, null);
    internal static NativeChildHit Invalid => new(false, null);
}

internal readonly record struct NativeParentClip(nuint Id, PixelRect ClientBounds, bool Visible);

internal static class NativeWindowHierarchy
{
    internal static TrackedWindow? FindDeepest(TrackedWindow root, PixelPoint point,
        Func<TrackedWindow, WindowTraversalBudget, NativeChildHit> childAt)
    {
        if (root.Id == 0 || !root.Visible || !root.Bounds.Contains(point)) return null;
        var budget = new WindowTraversalBudget();
        var visited = new HashSet<nuint> { root.Id };
        var current = root;
        while (budget.Spend())
        {
            var hit = childAt(current, budget);
            if (!hit.Valid) return null;
            if (hit.Child is not { } child) return current;
            if (child.Id == 0 || !visited.Add(child.Id) || !child.Visible ||
                child.StackingId != root.Id || !child.Bounds.Contains(point)) return null;
            current = child;
        }
        return null;
    }

    internal static bool TryClipToRoot(nuint child, nuint root, PixelRect initial,
        Func<nuint, NativeParentClip?> parentOf, out PixelRect bounds, out bool visible)
    {
        bounds = initial;
        visible = true;
        if (child == 0 || root == 0) return false;
        var current = child;
        var visited = new HashSet<nuint> { child };
        var budget = new WindowTraversalBudget();
        while (current != root)
        {
            if (!budget.Spend() || parentOf(current) is not { } parent || parent.Id == 0 ||
                !visited.Add(parent.Id)) return false;
            bounds = bounds.Intersect(parent.ClientBounds);
            visible &= parent.Visible;
            current = parent.Id;
        }
        return true;
    }
}
