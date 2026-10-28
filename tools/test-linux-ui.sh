#!/usr/bin/env bash
# Real pointer/pixel regression checks; use an isolated Linux container, not a desktop session.
set -euo pipefail
[[ "$EUID" != 0 ]] || { echo 'Run as an unprivileged test user.' >&2; exit 2; }
exe="$(realpath "${1:?Pass the installed executable}")"
out="$(realpath -m "${2:?Pass a writable artifact directory}")"
shift 2
mkdir -p "$out"
if [[ "${BLURMASK_UI_XVFB:-}" != 1 ]]; then
  export BLURMASK_UI_XVFB=1
  exec xvfb-run -a -s '-screen 0 1440x1000x24 +extension GLX +extension RENDER' bash "$0" "$exe" "$out" "$@"
fi
export LANG=C.UTF-8 XDG_DATA_HOME="$out/data" XDG_CONFIG_HOME="$out/config" XDG_CACHE_HOME="$out/cache"
unset WAYLAND_DISPLAY
mkdir -p "$XDG_DATA_HOME" "$XDG_CONFIG_HOME" "$XDG_CACHE_HOME"
pids=()
cleanup() {
  result=$?
  xwininfo -root -tree > "$out/final-windows.txt" 2>&1 || true
  magick import -window root "$out/final-desktop.png" 2>/dev/null || true
  for pid in "${pids[@]}"; do kill "$pid" 2>/dev/null || true; done
  for pid in "${pids[@]}"; do wait "$pid" 2>/dev/null || true; done
  exit "$result"
}
trap cleanup EXIT
openbox > "$out/openbox.log" 2>&1 & pids+=("$!")
sleep 1
# Keep the root pixmap alive: picom uses _XROOTPMAP_ID rather than xsetroot's solid fill.
python - "$out" > "$out/background.log" 2>&1 <<'PY' &
import ctypes as c, sys, time
from pathlib import Path
x = c.CDLL('libX11.so.6')
def bind(name, result, args):
    f = getattr(x, name); f.restype = result; f.argtypes = args; return f
ptr, ul, ui, si = c.c_void_p, c.c_ulong, c.c_uint, c.c_int
bind('XOpenDisplay', ptr, [c.c_char_p]); bind('XDefaultScreen', si, [ptr])
bind('XRootWindow', ul, [ptr, si]); bind('XDefaultDepth', si, [ptr, si])
bind('XCreatePixmap', ul, [ptr, ul, ui, ui, ui]); bind('XCreateGC', ptr, [ptr, ul, ul, ptr])
bind('XSetForeground', si, [ptr, ptr, ul]); bind('XFillRectangle', si, [ptr, ul, ptr, si, si, ui, ui])
bind('XInternAtom', ul, [ptr, c.c_char_p, si])
bind('XChangeProperty', si, [ptr, ul, ul, ul, si, si, ptr, si])
bind('XSetWindowBackgroundPixmap', si, [ptr, ul, ul]); bind('XClearWindow', si, [ptr, ul])
bind('XSync', si, [ptr, si]); bind('XCloseDisplay', si, [ptr])
d = x.XOpenDisplay(None)
assert d, 'Cannot open isolated X display'
screen = x.XDefaultScreen(d); root = x.XRootWindow(d, screen)
pixmap = x.XCreatePixmap(d, root, 1440, 1000, x.XDefaultDepth(d, screen))
gc = x.XCreateGC(d, pixmap, 0, None)
for y in range(0, 1000, 8):
    for xx in range(0, 1440, 8):
        # Sharp alternating edges, with slow RGB variation to expose whole-screen blur.
        rgb = (0xE8DDA8 if (xx // 8 + y // 8) % 2 else 0x234D83) ^ ((xx // 160) << 8)
        x.XSetForeground(d, gc, rgb)
        x.XFillRectangle(d, pixmap, gc, xx, y, 8, 8)
value = ul(pixmap)
for name in (b'_XROOTPMAP_ID', b'ESETROOT_PMAP_ID'):
    x.XChangeProperty(d, root, x.XInternAtom(d, name, 0), x.XInternAtom(d, b'PIXMAP', 0),
                      32, 0, c.byref(value), 1)
x.XSetWindowBackgroundPixmap(d, root, pixmap); x.XClearWindow(d, root); x.XSync(d, 0)
Path(sys.argv[1], 'background-ready').touch()
try:
    while True: time.sleep(1)
finally: x.XCloseDisplay(d)
PY
pids+=("$!")
for i in {1..40}; do [[ ! -f "$out/background-ready" ]] || break; sleep .1; done
test -f "$out/background-ready"
start_picom() {
  local mode="$1"
  if [[ -n "${compositor:-}" ]]; then
    kill "$compositor"; wait "$compositor" || true
    for index in "${!pids[@]}"; do
      [[ "${pids[$index]}" != "$compositor" ]] || unset "pids[$index]"
    done
  fi
  local config="$out/picom-$mode.conf"
  printf 'backend = "xrender"; fading = false; shadow = false;\n' > "$config"
  if [[ "$mode" != normal ]]; then
    printf 'blur-method = "kernel"; blur-kern = "3x3box"; blur-background = true; blur-background-fixed = true;\n' >> "$config"
  fi
  if [[ "$mode" == excluded ]]; then
    cat >> "$config" <<'CONF'
blur-background-exclude = ["name *= 'ink overlay'"];
CONF
  fi
  picom --config "$config" > "$out/picom-$mode.log" 2>&1 & compositor="$!"; pids+=("$compositor")
  sleep .4
  kill -0 "$compositor"
}
start_picom normal
xdpyinfo > "$out/xdpyinfo.txt"
glxinfo -B > "$out/glxinfo.txt" 2>&1 || true
cd "$out"
"$exe" "$@" > "$out/app.log" 2>&1 & pids+=("$!")
app="${pids[-1]}"
visible() { xdotool search --onlyvisible --name "$1" 2>/dev/null | tail -1; }
wait_visible() {
  local id
  for i in {1..40}; do
    kill -0 "$app"
    id="$(visible "$1" || true)"
    if [[ -n "$id" ]]; then printf '%s\n' "$id"; return; fi
    sleep .1
  done
  echo "Window did not map: $1" >&2; return 1
}
assert_unmapped() {
  local id
  id="$(visible "$1" || true)"
  [[ -z "$id" ]] || { echo "Unexpected mapped window: $1 ($id)" >&2; return 1; }
}
# Compare actual composited pixels, allowing only the layer's 1/255 input alpha.
# UI, known strokes and their antialiasing margins have explicitly bounded exclusions.
cat > "$out/check-background.py" <<'PY'
from pathlib import Path
from PIL import Image, ImageChops, ImageDraw
import json, sys
p = Path(sys.argv[1]); before, after, mode = sys.argv[2:]
a = Image.open(p / f'{before}.png').convert('RGB')
b = Image.open(p / f'{after}.png').convert('RGB')
assert a.size == b.size == (1440, 1000)
mask = Image.new('L', a.size)
draw = ImageDraw.Draw(mask)
draw.rectangle((400, 0, 1439, 999), fill=255)
for box in ((580, 505, 830, 620), (875, 675, 1225, 735)): draw.rectangle(box, fill=0)
channels = ImageChops.difference(a, b).split()
error = ImageChops.lighter(ImageChops.lighter(channels[0], channels[1]), channels[2])
error = ImageChops.multiply(error, mask)
count = mask.histogram()[255]; changed = sum(error.histogram()[2:]); maximum = error.getextrema()[1]
report = {'before': before, 'after': after, 'mode': mode,
          'checked_pixels': count, 'changed_over_one_level': changed, 'max_channel_error': maximum}
if mode == 'negative':
    # The 3x3 blur changes checker edges, while each 8x8 tile's center stays intact.
    # A missing/black/stale compositor frame must not count as a successful blur control.
    centers = [(x, y) for y in range(4, 1000, 8) for x in range(404, 1440, 8)
               if mask.getpixel((x, y))]
    centers_changed = sum(error.getpixel(point) > 1 for point in centers)
    report['negative_control_tile_centers'] = len(centers)
    report['negative_control_centers_changed'] = centers_changed
with (p / 'background-proof.jsonl').open('a') as f: f.write(json.dumps(report) + '\n')
if mode == 'negative':
    assert centers_changed == 0, ('Negative control lost the background instead of blurring edges', report)
    # Exercise the same detector: enabling compositor blur must make its normal invariant fail.
    assert changed > 50000 and maximum > 20, ('Blur negative control was not detected', report)
else:
    assert changed == 0, ('Desktop background was altered outside ink/UI', report)
PY
background_check() { python "$out/check-background.py" "$out" "$1" "$2" "$3"; }
capture() { magick import -window "$1" "$out/$2.png"; }
click_toolbar() { xdotool mousemove --window "$toolbar" 26 "$1" click 1; sleep .4; }
stroke() {
  local window="$1"; shift
  xdotool mousemove --window "$window" "$1" "$2"; shift 2
  sleep .15
  xdotool mousedown 1
  while (( "$#" )); do
    sleep .15
    xdotool mousemove --window "$window" "$1" "$2"; shift 2
  done
  sleep .15
  xdotool mouseup 1
  sleep .6
}
toolbar="$(wait_visible 'NovaPen.*drawing tools')"
sleep 1
xwininfo -root -tree > "$out/startup-windows.txt"
assert_unmapped 'NovaPen.*ink overlay'
capture root startup-desktop
capture "$toolbar" toolbar-startup
# Pen maps the layer; pointer gestures must reach it.
click_toolbar 124
overlay="$(wait_visible 'NovaPen.*ink overlay')"
capture "$overlay" desktop-before
capture root desktop-pen-mapped
background_check startup-desktop desktop-pen-mapped normal
# A compositor with global blur reproduces the regression. This is a negative control,
# not automatic protection: picom needs its own narrowly scoped overlay exclusion.
start_picom negative
sleep .8
capture root desktop-blur-negative
background_check desktop-pen-mapped desktop-blur-negative negative
start_picom excluded
sleep .8
capture root desktop-blur-excluded
background_check desktop-pen-mapped desktop-blur-excluded excluded
stroke "$overlay" 600 550 670 580 740 530 800 590
capture "$overlay" desktop-after
capture root desktop-stroke-screen
background_check desktop-pen-mapped desktop-stroke-screen stroke
# A real highlighter stroke must blend with the patterned desktop rather than erase it.
xdotool windowactivate --sync "$toolbar"
xdotool key --clearmodifiers ctrl+shift+4
stroke "$overlay" 900 705 1000 705 1100 705 1200 705
capture root desktop-highlighter-screen
background_check desktop-stroke-screen desktop-highlighter-screen highlighter
python - "$out" <<'PY'
from pathlib import Path
from PIL import Image
import json, sys
p = Path(sys.argv[1])
a = Image.open(p / 'desktop-stroke-screen.png').convert('RGB')
b = Image.open(p / 'desktop-highlighter-screen.png').convert('RGB')
# Interior samples across many checker edges must have one shared alpha/color blend.
samples = [(a.getpixel((x, y)), b.getpixel((x, y))) for y in range(700, 710) for x in range(930, 1170)]
changed = sum(max(abs(u-v) for u,v in zip(old,new)) > 5 for old,new in samples)
assert changed > 1500, ('Highlighter stroke did not reach the screen', changed)
# Within each checker pair, contrast must survive at ~65% of the original value.
ratios = []
for x in range(936, 1160, 16):
    old1, old2 = a.getpixel((x,705)), a.getpixel((x+8,705))
    new1, new2 = b.getpixel((x,705)), b.getpixel((x+8,705))
    for i in range(3):
        if abs(old1[i]-old2[i]) > 60: ratios.append(abs(new1[i]-new2[i])/abs(old1[i]-old2[i]))
assert ratios and all(.60 <= r <= .70 for r in ratios), ('Highlighter must preserve background through 35% alpha', ratios)
(p / 'highlighter-proof.json').write_text(json.dumps({'changed_pixels': changed, 'contrast_ratio_min': min(ratios), 'contrast_ratio_max': max(ratios)}, indent=2))
PY
# A pointer click through the ink layer must reach the toolbar and open a menu.
click_toolbar 212
xwininfo -root -tree > "$out/toolbar-click-windows.txt"
flyout="$(wait_visible 'NovaPen.*menu')"
capture "$flyout" sizes-flyout
xdotool mousemove --window "$flyout" 88 20 click 1
sleep .5
assert_unmapped 'NovaPen.*menu'
xdotool windowactivate --sync "$toolbar"
xdotool key --clearmodifiers ctrl+shift+c
canvas="$(wait_visible 'NovaPen.*Canvas')"
assert_unmapped 'NovaPen.*ink overlay'
# Native mapping does not mean that picom has presented the first frame yet.
# Wait for the real screen to paint without activating, raising or moving the window.
paint_start=$SECONDS
paint_deadline=$((SECONDS + 5))
paint_attempt=0
paint_ready=0
while (( SECONDS < paint_deadline )); do
  kill -0 "$app"
  paint_attempt=$((paint_attempt + 1))
  xdotool getwindowgeometry --shell "$canvas" > "$out/canvas-geometry.txt"
  capture "$canvas" canvas-before
  capture root canvas-desktop
  if (( paint_attempt == 1 )); then
    cp "$out/canvas-desktop.png" "$out/canvas-desktop-first-map.png"
  fi
  if python - "$out" "$paint_attempt" "$((SECONDS - paint_start))" >> "$out/canvas-paint-readiness.jsonl" <<'PY'
from pathlib import Path
from PIL import Image
import json, sys
p = Path(sys.argv[1])
geometry = dict(line.split('=', 1) for line in (p / 'canvas-geometry.txt').read_text().splitlines())
x, y = int(geometry['X']), int(geometry['Y'])
native = Image.open(p / 'canvas-before.png').convert('RGB').crop((250, 180, 900, 650))
screen = Image.open(p / 'canvas-desktop.png').convert('RGB').crop((x + 250, y + 180, x + 900, y + 650))
native_white = sum(min(pixel) >= 245 for pixel in native.getdata())
screen_white = sum(min(pixel) >= 245 for pixel in screen.getdata())
print(json.dumps({'attempt': int(sys.argv[2]), 'elapsed_seconds': int(sys.argv[3]),
                  'native_white_pixels': native_white, 'screen_white_pixels': screen_white}))
sys.exit(0 if native_white > 200000 and screen_white > 200000 else 1)
PY
  then
    paint_ready=1
    break
  fi
  sleep .2
done
[[ "$paint_ready" == 1 ]] || { echo 'Canvas did not paint an opaque board on screen within five seconds.' >&2; exit 1; }
xdotool windowactivate --sync "$canvas"
xdotool key --clearmodifiers ctrl+shift+3
stroke "$canvas" 360 310 430 360 510 330 590 410
capture "$canvas" canvas-after
capture root canvas-after-desktop
# Ask the WM to close, rather than destroying the native window directly.
wmctrl -ic "$(printf '0x%x' "$canvas")"
sleep .8
assert_unmapped 'NovaPen.*Canvas'
# Closing flushes the document; the periodic autosave has a 1300 ms cadence.
find "$XDG_DATA_HOME" -name canvas.json -exec cp {} "$out/canvas-state.json" \;
test -s "$out/canvas-state.json"
[[ "$(wait_visible 'NovaPen.*ink overlay')" == "$overlay" ]]
wmctrl -ic "$(printf '0x%x' "$overlay")"
sleep .8
assert_unmapped 'NovaPen.*ink overlay'
capture root desktop-wm-hidden
background_check startup-desktop desktop-wm-hidden hidden
click_toolbar 124
[[ "$(wait_visible 'NovaPen.*ink overlay')" == "$overlay" ]]
capture root desktop-wm-reopened
background_check desktop-highlighter-screen desktop-wm-reopened reopened
click_toolbar 212
flyout="$(wait_visible 'NovaPen.*menu')"
capture "$flyout" reopened-sizes-flyout
xwininfo -root -tree > "$out/reopened-windows.txt"
python - "$out" <<'PY'
from pathlib import Path
from PIL import Image, ImageChops
import json, sys
p = Path(sys.argv[1]); report = {}
for kind in ['desktop', 'canvas']:
    before = Image.open(p / f'{kind}-before.png').convert('RGB')
    after = Image.open(p / f'{kind}-after.png').convert('RGB')
    diff = ImageChops.difference(before, after)
    if kind == 'canvas':
        # Restrict to the board; exclude status/chrome changes.
        diff = diff.crop((250, 180, 900, 650))
        white = sum(min(pixel) >= 245 for pixel in before.crop((250, 180, 900, 650)).getdata())
        assert white > 200000, ('Canvas must paint an opaque white board', white)
        report['canvas_white_pixels'] = white
    changed = sum(any(pixel) for pixel in diff.getdata())
    report[kind] = {'changed_pixels': changed, 'bbox': diff.getbbox()}
    assert changed > 200, (kind, report[kind])
state = json.loads((p / 'canvas-state.json').read_text())
pens = [m for m in state['board']['marks'] if m['tool'] == 'Pen' and len(m['points']) >= 3]
assert pens
# The scripted stroke spans 230 screen pixels horizontally. Width is stored in
# world units, so the selected 8px nib must remain 8px after the fitted camera.
assert any(abs(m['width'] * 230 / (m['points'][-1]['x'] - m['points'][0]['x']) - 8) < .05 for m in pens)
report['persisted_canvas_marks'] = len(state['board']['marks'])
geometry = dict(line.split('=', 1) for line in (p / 'canvas-geometry.txt').read_text().splitlines())
x, y = int(geometry['X']), int(geometry['Y'])
board_box = (x + 250, y + 180, x + 900, y + 650)
screen_before = Image.open(p / 'canvas-desktop.png').convert('RGB').crop(board_box)
screen_after = Image.open(p / 'canvas-after-desktop.png').convert('RGB').crop(board_box)
white = sum(min(pixel) >= 245 for pixel in screen_before.getdata())
changed = sum(any(pixel) for pixel in ImageChops.difference(screen_before, screen_after).getdata())
assert white > 200000, ('Canvas must be visible and opaque on screen', white)
assert changed > 200, ('Canvas stroke must be visible on screen', changed)
report['canvas_screen_white_pixels'] = white
report['canvas_screen_changed_pixels'] = changed
report['startup_overlay_unmapped'] = True
report['toolbar_pointer_flyout'] = True
report['flyout_pointer_sets_pen_width'] = 8
report['wm_close_reuses_overlay'] = True
report['root_background_preserved'] = True
report['picom_blur_negative_control_detected'] = True
report['picom_scoped_blur_exclusion_preserves_background'] = True
report['highlighter_preserves_background_contrast'] = True
(p / 'pixel-proof.json').write_text(json.dumps(report, indent=2))
print(json.dumps(report, indent=2))
PY
echo 'Linux real pointer, opaque canvas, pixels, toolbar and WM lifecycle checks passed.'
