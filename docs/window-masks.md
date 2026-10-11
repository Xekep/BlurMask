# Masks attached to windows

With attachment disabled, masks remain above all ordinary windows. With
attachment enabled, a mask is placed immediately above its target in the
native stacking order, below windows that cover the target. Moving/restoring
the target does not activate it. Multiple masks attached to the same target
share a contiguous stack without repeatedly raising each other.

Windows uses `SetWindowPos` without activation or owner changes, and watches
target geometry plus foreground/top-level stacking events. X11 uses the EWMH
restacking request when supported, otherwise the actual root-level frames.
macOS orders the mask relative to the target WindowServer number.
References: [Win32 ordering](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos),
[EWMH](https://specifications.freedesktop.org/wm/latest-single/),
[AppKit ordering](https://developer.apple.com/documentation/appkit/nswindow/order(_:relativeto:)).

Moving a target translates the original mask without changing its dimensions.
Resizing can reflow content unpredictably: ordinary attached masks therefore
latch a full-window opaque black cover at the first observed size change,
including nonuniform growth or shrinkage. Returning to the old dimensions does
not remove it. Move/resize the mask explicitly to configure a new protected
region, or delete it. This uses asynchronous foreign-window notifications;
it does not provide an atomic protection guarantee during compositor frames.

With attachment enabled, **Fill window region** starts a point selection.
Click a native child region to cover all its actual bounds with black;
right-click/Escape cancels. Its dimensions follow native bounds rather than a
scaled rectangle. If the child disappears, is hidden, or is replaced, the
parent remains fully covered until explicit removal. Closing the parent removes
the mask. Middle click/the eraser still remove masks.

Native child discovery is available for Win32 and X11 children, not arbitrary
DOM elements or controls drawn inside a single surface. macOS fills the chosen
whole window without requiring Accessibility access. Native Wayland does not
expose arbitrary foreign window geometry or stacking; desktop masks remain
unavailable there. Under XWayland, only X11 clients can be enumerated.

The headless overlay probe checks resize protection, translation and stack
policy. CI desktop smoke checks native occlusion/restoration order and native
child geometry, including deletion fallback. No local desktop is opened by
headless tests.
