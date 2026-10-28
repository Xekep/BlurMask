using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NovaPen;

// Out-of-context WinEvents need no code injection into the target process.
internal sealed unsafe partial class WindowsWindowEvents : IDisposable
{
    private static readonly ConcurrentDictionary<nint, WindowsWindowEvents> Instances = new();
    private readonly nint _window;
    private readonly Action _changed;
    private readonly List<nint> _hooks = [];
    private bool _disposed;
    private WindowsWindowEvents(TrackedWindow target, Action changed)
    {
        _window = (nint)target.Id; _changed = changed;
        foreach (var (first, last, process) in new[] { (0x16u, 0x17u, (uint)target.ProcessId),
            (0x8001u, 0x800Bu, (uint)target.ProcessId), (3u, 3u, 0u), (0x8002u, 0x8004u, 0u) })
        {
            var hook = SetWinEventHook(first, last, 0, &OnEvent, process, 0, 0);
            if (hook == 0) continue;
            _hooks.Add(hook); Instances[hook] = this;
        }
    }
    internal static WindowsWindowEvents? TryCreate(TrackedWindow target, Action changed)
    {
        if (!OperatingSystem.IsWindows()) return null;
        var watcher = new WindowsWindowEvents(target, changed);
        if (watcher._hooks.Count == 4) return watcher;
        watcher.Dispose(); return null;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void OnEvent(nint hook, uint kind, nint window, int objectId, int childId, uint thread, uint time)
    {
        try
        {
            if (objectId == 0 && childId == 0 && Instances.TryGetValue(hook, out var watcher) &&
                !watcher._disposed && window != 0)
            {
                bool targetEvent = watcher._window == window || EventRoot(window, 2) == watcher._window;
                _ = EventProcess(window, out var process);
                bool stackingEvent = kind is 3 or 0x8002 or 0x8003 or 0x8004 && process != Environment.ProcessId &&
                    window != EventDesktop() && EventRoot(window, 2) == window;
                if (targetEvent || stackingEvent) watcher._changed();
            }
        }
        catch { /* Exceptions must never escape a native event callback. */ }
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var hook in _hooks) { Instances.TryRemove(hook, out _); _ = UnhookWinEvent(hook); }
        _hooks.Clear();
    }
    [LibraryImport("user32.dll")]
    private static partial nint SetWinEventHook(uint first, uint last, nint module,
        delegate* unmanaged[Stdcall]<nint, uint, nint, int, int, uint, uint, void> callback, uint process, uint thread, uint flags);
    [LibraryImport("user32.dll")] private static partial int UnhookWinEvent(nint hook);
    [LibraryImport("user32.dll", EntryPoint = "GetAncestor")] private static partial nint EventRoot(nint window, uint flags);
    [LibraryImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")] private static partial uint EventProcess(nint window, out uint process);
    [LibraryImport("user32.dll", EntryPoint = "GetDesktopWindow")] private static partial nint EventDesktop();
}
