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
picom --backend xrender > "$out/picom.log" 2>&1 & pids+=("$!")
xsetroot -solid '#315070'
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
toolbar="$(wait_visible 'BlurMask.*drawing tools')"
sleep 1
xwininfo -root -tree > "$out/startup-windows.txt"
assert_unmapped 'BlurMask.*ink overlay'
capture root startup-desktop
capture "$toolbar" toolbar-startup
# Pen maps the layer; pointer gestures must reach it.
click_toolbar 124
overlay="$(wait_visible 'BlurMask.*ink overlay')"
capture "$overlay" desktop-before
stroke "$overlay" 600 550 670 580 740 530 800 590
capture "$overlay" desktop-after
# A pointer click through the ink layer must reach the toolbar and open a menu.
click_toolbar 212
xwininfo -root -tree > "$out/toolbar-click-windows.txt"
flyout="$(wait_visible 'BlurMask.*menu')"
capture "$flyout" sizes-flyout
xdotool mousemove --window "$flyout" 88 20 click 1
sleep .5
assert_unmapped 'BlurMask.*menu'
xdotool windowactivate --sync "$toolbar"
xdotool key --clearmodifiers ctrl+shift+c
canvas="$(wait_visible 'BlurMask.*Canvas')"
sleep .8
assert_unmapped 'BlurMask.*ink overlay'
capture "$canvas" canvas-before
capture root canvas-desktop
xdotool getwindowgeometry --shell "$canvas" > "$out/canvas-geometry.txt"
xdotool windowactivate --sync "$canvas"
xdotool key --clearmodifiers ctrl+shift+3
stroke "$canvas" 360 310 430 360 510 330 590 410
capture "$canvas" canvas-after
capture root canvas-after-desktop
find "$XDG_DATA_HOME" -name canvas.json -exec cp {} "$out/canvas-state.json" \;
# Ask the WM to close, rather than destroying the native window directly.
wmctrl -ic "$(printf '0x%x' "$canvas")"
sleep .8
assert_unmapped 'BlurMask.*Canvas'
[[ "$(wait_visible 'BlurMask.*ink overlay')" == "$overlay" ]]
wmctrl -ic "$(printf '0x%x' "$overlay")"
sleep .8
assert_unmapped 'BlurMask.*ink overlay'
click_toolbar 124
[[ "$(wait_visible 'BlurMask.*ink overlay')" == "$overlay" ]]
click_toolbar 212
flyout="$(wait_visible 'BlurMask.*menu')"
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
assert any(m['tool'] == 'Pen' and len(m['points']) >= 3 and m['width'] == 8 for m in state['board']['marks'])
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
(p / 'pixel-proof.json').write_text(json.dumps(report, indent=2))
print(json.dumps(report, indent=2))
PY
echo 'Linux real pointer, opaque canvas, pixels, toolbar and WM lifecycle checks passed.'
