# Mask controller and native picker regression probe

This standalone project source-links the production attachment controller,
coverage policy, window catalog and picker. Its minimal `NovaPenWindow` records
privacy/visibility requests; it does not load the main application, render blur,
capture the desktop, or execute a recognition SDK.

```powershell
dotnet run --project tools/NovaPen.MaskProbe -c Release
```

Use `--headless-only` or `--native-only` after `--` to select a group. Native
picking runs on Windows; other systems explicitly skip that group. A failed
check exits with code 1.

The headless checks exercise initial attachment, target translation, a genuine
position notification, the quiet-period rebind, replacement-target following,
and switching an established 500 ms idle poll back to 32 ms. The timer interval
is set/read through diagnostic reflection because the fake catalog cannot
establish real WinEvent hooks. Calls to `Update` are explicit; this checks the
controller policy, not dispatcher scheduling or WinEvent delivery latency.

Avalonia.Headless 12.1.2 does not map `PointToScreen` using the window position.
The fake catalog therefore supplies the intended next target and counts picker
calls. Physical screen-point picking is tested separately below. A guarded
adapter invokes the backend's position callback only if its setter did not
already notify; the tested backend emitted its own notifications, so the
recorded synthetic callback count was zero. Earlier failures based on assuming
headless screen-coordinate mapping were fixture errors, not product bugs.

Child coverage checks move an actual controller's 8×6 headless mask, temporarily
hide or remove the child, verify the existing opaque parent fallback, and then
restore the small child. They also check root hide/restore. No fail-closed
privacy expectation is relaxed.

The Windows helper runs in another process so the catalog's own-process filter
remains active. It creates only no-activate tool windows near `(-25000,-25000)`,
at most 200×200 pixels: an untitled application with an 8×6 native child, a
visible `WS_EX_LAYERED | WS_EX_TRANSPARENT` covering window initialized with
alpha 255, a locally registered shell-class fixture, and a separate nonlayered
`WS_EX_TRANSPARENT` window that must remain eligible. The latter distinguishes
painting order from the documented layered mouse-pass-through behavior.
Neither Explorer nor any user application's windows are modified. Stdin closes
the helper; a bounded wait/kill and its `finally` destroy the fixtures.

The transparency distinction follows [Microsoft's layered-window
documentation](https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features#layered-windows).
These checks do not exercise real foreground dragging, rendered blackout,
display scaling, all application child hierarchies, or macOS/X11 native picking.

For a controlled historical build, set `MaskSourceRoot` to a directory with the
seven linked production files. The recorded before variant came from commit
`63b0cd0`; only its namespace and window type were adapted to `NovaPen`. All
assertions and probe code were identical between before and after builds.

Windows CPU2/affinity3 verification used a 600-second build watchdog and
45-second run watchdog. The historical variant failed the timer, child recovery
and native-picker regressions; the corrected variant passed 14 headless and
4 native checks. Source hashes, before/after portable PDB proofs (8 authored
files plus 4 generated), commands and stdout/stderr are retained at
`E:/BlurMask-build-artifacts-20261014/novapen-mask-probe-review-20261029`.
