# Drawing, masks and canvas

## Features

- system tray application;
- Epic Pen-style floating drawing toolbar, shown on manual launch; login startup stays in the tray without creating windows;
- left-click the tray icon to show/hide the toolbar and restore any open canvas;
- right-click tray menu contains only **Show/Hide window** and **Exit**;
- always-on-top frameless rectangle;
- drag the center with the left mouse button to move it;
- drag edges/corners to resize it;
- middle click the mask to destroy it;
- right click the mask to cycle privacy modes;
- hover-only outline so the editing border normally stays out of recordings;
- .NET 11 + Avalonia 12;
- Native AOT deployment;
- Native AOT-safe code path; tray resources are defined in compiled Avalonia XAML.

## Desktop drawing

The narrow white toolbar follows the classic Epic Pen layout: drag handle, visibility,
cursor, drawing tools, eraser, pen size, undo, clear, board, screenshot, menu, and colors.
Icons are drawn as local vector geometry. Reference: [Epic Pen toolbar screenshot](https://software.thaiware.com/13290.html)
and [official tool guide](https://epicpen.com/userguide).

- The pen menu contains **pen, AI pen, highlighter and text**. Shapes have their own button.
  Drawing, shape and cursor buttons remember their last mode for the current session.
  Click an inactive button once to restore that mode; click again to open its choices.
  Escape and right-click return to the ordinary cursor without forgetting the selected modes.
- **AI pen** starts recognition when a stroke ends and replaces a completed row after a 0.5-second writing pause.
  Symbols share a font size and baseline; position, bounding size and color are preserved. Conversion is automatic.
  Recognition processes stroke coordinates locally: English text uses native C#; **Formulas** uses the bundled native CPU runtime.
  English and experimental Russian resources are bundled; custom English packs are optional.
  The previous raster recognizers have been removed. Russian handwriting is not ready.
  Fonts are automatic: Inter for text, STIX Two Math for formulas and STIX Two Text Italic for variables; settings has no font selector.
  Uncertain results stay handwritten, and Undo restores the original strokes. See [model details and limits](../recognition.md).
- Draw directly over the desktop and applications. On Windows, cursor mode leaves the ink visible
  and passes clicks to applications underneath. Right-click the drawing surface or press Esc to return to cursor mode.
- In selection mode, Shift-click adds/removes an object; drag an empty area to select a group.
  Shift-drag adds an area to the selection. Ctrl+A (Cmd+A on macOS) selects all objects.
  Move, resize or rotate a group together; Delete removes the selection in one undoable operation.
  These operations work on the desktop and editable shared canvases.
- The eraser removes entire strokes and closes privacy masks touched on the desktop.
  Each ink eraser gesture, drawing operation, and clear can be undone.
  Right-click Undo or use the menu for Redo.
- Hold Shift while drawing shapes for a square/circle or 45° line/arrow angles.
- Click to place text; Enter commits, Shift+Enter adds a line, and Esc cancels.
- Choose five stroke widths, four quick colors, 24 palette colors, or enter a custom hex color.
- Switch between the desktop, whiteboard, and blackboard without losing annotations.
- The eye button collapses the toolbar with an animation and hides ink; the chalk logo remains above the closed eye. Click the eye to restore the toolbar. **Minimize to tray** hides it completely; click the tray icon to restore it.
- The cursor menu offers two modes: normal cursor and **Select and move**. Click a drawn object and drag it on the screen or canvas; Delete removes it. Moves support undo/redo.
- Shapes have a separate button. The mask button opens a horizontal strip: each icon creates a new independent mask, and the last button removes all masks.
- **Follow window** below the mask icons automatically detects the window under each mask's centre. Attached masks share the target's stacking order, move with it, hide while it is minimized, and close with it. A target resize latches a full opaque cover until the mask is explicitly reconfigured, protecting content that reflows. **Fill window region** covers a clicked native child region with black and follows its real bounds (Windows/X11; macOS covers the whole window). Windows uses out-of-context geometry/stacking events; macOS and X11 poll every 32 ms. Native Wayland disables attachment because it does not expose foreign window geometry. See [window masks and platform limits](../window-masks.md).
- The analog timer opens a horizontal strip: off, quick (2 s), long (10 s), very long (30 s), with progressively filled timer icons. Temporary ink fades locally and is excluded from saved/shared boards.
- The canvas has a collapsible floating icon toolbar for new/connect/share/background/export/screen/fit actions. Click the chalk logo or press **Tab** to collapse/expand it. **Ctrl+V** pastes a screenshot from the clipboard as a movable image that is saved, shared and included in PNG exports.
- Pasted images are normalized to PNG (up to 2048 px on the longest side); shared documents are limited to 4 MiB and 32 megapixels of raster content.
- Interface language follows the system UI language: English, French, German or Russian; other languages fall back to English.
- Screenshots include all monitors and visible annotations, exclude the toolbar, and save PNG files to `Pictures/BlurMask`.
- The menu includes **New mask**, text size, history actions, visibility, shortcuts, and exit.
- Multiple monitors use separate transparent windows and a shared document with physical desktop coordinates.

Shortcuts follow the installed Epic Pen layout used as the reference: **Ctrl+Shift+1** ink visibility,
**2** cursor, **3** pen, **4** highlighter, **5** eraser, **6** undo, **7** clear,
**PrintScreen** screenshot, **[ / ]** decrease/increase width, **F** fading ink, **0** toolbar visibility,
**L/E/R/A/T** line/ellipse/rectangle/arrow/text, **W/B** white/black screen board, **9** previous color.
**Alt+Shift+1–6** select quick colors. BlurMask additions: **Ctrl+Shift+Y** redo, **C** independent canvas, **D** screen input.
Open **Menu → Settings** to record, disable or reset shortcuts and select the interface language.
Shortcut changes apply immediately; language changes apply after restarting. Settings are saved in the user's
local application-data directory as `BlurMask/settings.json`. The settings window suspends shortcuts while
recording combinations and rejects duplicates. Windows shortcuts are global; on Linux/macOS they work while
a BlurMask window is focused. Conflicts with another app appear in Settings; the toolbar remains available.

Windows supports click-through ink and desktop screenshots. Other platforms currently hide the
ink overlay in cursor mode; desktop screenshots and global hotkeys are Windows-only.
Desktop annotations and toolbar settings are session-local. The separate canvas is saved locally.

## Independent canvas and shared boards

Use the board flyout or **Ctrl+Shift+C** to open a separate canvas that starts at 2400×1600 and grows in all directions as ink approaches its edges.
**Ctrl+Shift+D** returns drawing input to the screen. Screen and canvas drawings have independent
documents and undo histories. Both use the same drawing tools, sizes and colors. The canvas
supports panning, white/black backgrounds, PNG export and automatic local persistence.
The wheel zooms around the pointer; bottom-right controls and **Ctrl +/-** change zoom,
**Ctrl+0** fits the board. Middle-button dragging or cursor mode pans. Growing never shifts
object coordinates, including negative coordinates and shared peer drawings. PNG export
includes extended content and scales large boards to at most 4096 pixels on either side.

The canvas toolbar's **Connect** icon accepts an invitation code or link.
**Collaborate** creates a shared copy on the built-in `https://pen.kiiko.ru` service on first click and displays separate **Draw together** and view codes,
with buttons to copy the code or invitation link. Links use one opaque fragment (`/#<code>`)
without visible board/key parameters. The code retains the full 256-bit access key.
Only this invitation format is supported. Pens, highlighters and shapes appear for other participants
while they are being drawn: an authenticated WebSocket sends new points about every 50 ms.
Finished strokes save immediately through the PostgreSQL API; a 1.3-second polling fallback preserves
concurrent strokes and pending offline edits. Unfinished previews stay outside saved documents,
exports and undo history; cancellation or disconnection clears them. View-only participants also
receive live previews. Screen annotations and fading ink remain local.

The minimalist software download page, ASP.NET Core API, PostgreSQL storage and Docker/Caddy configuration for
`pen.kiiko.ru` are in [`src/BlurMask.Server/`](../../../src/BlurMask.Server/README.md). Deployment requires a server and DNS
configuration; see that README for launch commands. Accounts are not required: shared links
carry permission to edit or view a particular board. The website has no drawing editor;
opening an invitation there offers software downloads and a button to copy the invitation into BlurMask.

Epic Pen's [official guide](https://epicpen.com/userguide) describes desktop annotation and
white/black screen boards. The independent persistent canvas and shared native drawing
are BlurMask additions.

## Privacy modes

Right-click the rectangle to cycle:

1. **Blur** - compositor blur-behind when available.
2. **Pixel Blur** - transparent coarse pixel tiles over compositor blur.
3. **Scramble** - on Windows, live 8×8 pixel source tiles swap diagonally in adjacent groups using four Magnification planes. Unsupported platforms use an opaque mosaic.
4. **Glass Refraction** - on Windows, fixed 30×30 px optical cells on a 32 px pitch. The cells are assigned to four persistent Magnification planes with slightly different zoom/offset/color transforms, so lines and text genuinely break at cell boundaries. Resizing adds/removes square cells instead of stretching them. Other platforms currently use the translucent glass-block fallback.
5. **Blackout** - solid black privacy mask.

No mask writes desktop captures to disk. On Windows, Glass Refraction and Scramble use the system Magnification API with exactly four child Magnifier windows per mask, regardless of cell count. Each plane is clipped by a disjoint Win32 region; sources refresh at 10 Hz while visible and immediately on move/resize. Blackout provides a solid cover; other effects preserve some visual context. Native optical effects are disabled in RDP/software mode, with opaque fallback. The minimum mask size is 60×40.
The tray tooltip shows the current mode.


### Tray and single-instance behavior

- Right-click the tray icon to open the application menu: drawing tools, create a mask, close the mask, or exit BlurMask.
- Only one BlurMask process is allowed per OS user/session. Launching it again exits silently with code `0`; it does not show a dialog, notification, or second tray icon.
