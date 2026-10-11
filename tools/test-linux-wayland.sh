#!/usr/bin/env bash
# Native Wayland canvas test in our own headless compositor; never use a desktop session.
# Ubuntu 24.04+ (Weston 13+): weston libwayland-client0 libwayland-cursor0 libwayland-egl1 libxkbcommon0
#         libegl1 libgles2 libgl1-mesa-dri python3-pil
set -euo pipefail
[[ "$EUID" != 0 ]] || { echo 'Run as an unprivileged test user.' >&2; exit 2; }
exe="$(realpath "${1:?Pass the published or installed executable}")"
out="$(realpath -m "${2:?Pass a fresh writable artifact directory}")"
shift 2
for tool in weston weston-screenshooter timeout python3; do command -v "$tool" >/dev/null; done
python3 -c 'from PIL import Image'
[[ -x "$exe" ]] || { echo "Executable is missing: $exe" >&2; exit 2; }
mkdir -p "$out"
[[ ! -e "$out/app.log" && ! -e "$out/app/wayland-native-canvas.png" && ! -e "$out/app/wayland-compositor-verified.txt" ]] || {
  echo 'Use a fresh artifact directory to avoid accepting stale evidence.' >&2; exit 2;
}
export LANG=C.UTF-8 XDG_DATA_HOME="$out/data" XDG_CONFIG_HOME="$out/config" XDG_CACHE_HOME="$out/cache"
export XDG_PICTURES_DIR="$out/captures" BLURMASK_SMOKE_ARTIFACTS="$out/app"
export BLURMASK_WAYLAND_CAPTURE_ACK="$out/app/wayland-compositor-verified.txt"
export LIBGL_ALWAYS_SOFTWARE=1
# The EGL driver may use llvmpipe. This does not request Avalonia's X11 software backend.
unset DISPLAY WAYLAND_DISPLAY WAYLAND_SOCKET
mkdir -p "$XDG_DATA_HOME" "$XDG_CONFIG_HOME" "$XDG_CACHE_HOME" "$XDG_PICTURES_DIR" "$BLURMASK_SMOKE_ARTIFACTS"
runtime="$(mktemp -d "${TMPDIR:-/tmp}/blurmask-wayland.XXXXXX")"
chmod 700 "$runtime"
export XDG_RUNTIME_DIR="$runtime" WAYLAND_DISPLAY=wayland-blurmask-test
app_pid= compositor_pid=
cleanup() {
  result=$?
  trap - EXIT
  for pid in "$app_pid" "$compositor_pid"; do
    [[ -z "$pid" ]] || kill "$pid" 2>/dev/null || true
  done
  for pid in "$app_pid" "$compositor_pid"; do
    [[ -z "$pid" ]] || wait "$pid" 2>/dev/null || true
  done
  # Remove only our own empty socket directory after Weston has stopped.
  rm -f -- "$runtime/$WAYLAND_DISPLAY" "$runtime/$WAYLAND_DISPLAY.lock"
  rmdir -- "$runtime" 2>/dev/null || true
  if [[ "$result" != 0 ]]; then tail -80 "$out/app.log" "$out/weston.log" 2>/dev/null || true; fi
  exit "$result"
}
trap cleanup EXIT
weston --version > "$out/weston-version.txt"
weston_major="$(sed -nE 's/^weston ([0-9]+)\..*/\1/p' "$out/weston-version.txt")"
[[ "$weston_major" =~ ^[0-9]+$ && "$weston_major" -ge 13 ]] || {
  echo 'This native Wayland probe requires Weston 13+ (Ubuntu 24.04+); Weston 9 advertises xdg_wm_base v1, but Avalonia requires v3.' >&2
  exit 2
}
weston --help > "$out/weston-help.txt" 2>&1
cat > "$out/weston.ini" <<'INI'
[core]
idle-time=0
[shell]
background-color=0xff233344
background-image=
panel-position=none
locking=false
INI
options=(--backend=headless --renderer=pixman)
# --debug exposes capture on this private, newly created compositor only.
weston "${options[@]}" --width=1440 --height=1000 --socket="$WAYLAND_DISPLAY" \
  --idle-time=0 --debug --config="$out/weston.ini" --log="$out/weston.log" \
  > "$out/weston-stdout.log" 2>&1 & compositor_pid=$!
for attempt in {1..100}; do
  kill -0 "$compositor_pid" || { echo 'Weston exited before creating its socket.' >&2; exit 1; }
  [[ ! -S "$XDG_RUNTIME_DIR/$WAYLAND_DISPLAY" ]] || break
  sleep .1
done
[[ -S "$XDG_RUNTIME_DIR/$WAYLAND_DISPLAY" ]] || { echo 'Weston socket timed out.' >&2; exit 1; }
printf 'DISPLAY=<unset>\nWAYLAND_DISPLAY=%s\nXDG_RUNTIME_DIR=%s\nLIBGL_ALWAYS_SOFTWARE=1\n' \
  "$WAYLAND_DISPLAY" "$XDG_RUNTIME_DIR" > "$out/environment.txt"
cd "$out"
capture() {
  local directory="$1"
  mkdir -p "$directory"
  # Weston 13 uses weston_capture_v1; its client still takes no arguments and
  # writes XDG_PICTURES_DIR/wayland-screenshot-*.png. --debug above authorizes it
  # only on our private compositor. A protocol failure can return zero without
  # writing a PNG, so require actual capture output as well as a successful exit.
  (cd "$directory"; XDG_PICTURES_DIR="$directory" timeout --kill-after=1s 3s weston-screenshooter) \
    >> "$out/screenshooter.log" 2>&1
  compgen -G "$directory/wayland-screenshot-*.png" > /dev/null || {
    echo "Weston did not write a compositor screenshot in $directory." >&2
    return 1
  }
}
# A clean baseline lets the compositor assertion reject colors from its own shell.
capture "$out/captures/before"
WAYLAND_DEBUG=client timeout --kill-after=5s 60s "$exe" --wayland-smoke-test --display-backend wayland "$@" \
  > "$out/app.log" 2>&1 & app_pid=$!
ready=0
for attempt in {1..600}; do
  kill -0 "$compositor_pid" || { echo 'Weston exited during the test.' >&2; exit 1; }
  if [[ -s "$out/app/wayland-native-canvas.png" ]]; then ready=1; break; fi
  kill -0 "$app_pid" 2>/dev/null || break
  sleep .1
done
[[ "$ready" == 1 ]] || { echo 'Native canvas PNG was not produced.' >&2; exit 1; }
# An offscreen PNG can precede the canvas's first Wayland buffer commit. Keep
# the asynchronous smoke host alive until actual compositor pixels prove that
# its window has been mapped and presented. Preserve early frames for diagnosis.
presented=0
presentation_deadline=$((SECONDS + 25))
for attempt in {1..100}; do
  [[ "$SECONDS" -lt "$presentation_deadline" ]] || break
  kill -0 "$app_pid" 2>/dev/null || break
  frame_directory="$out/captures/canvas/attempt-$(printf '%03d' "$attempt")"
  capture "$frame_directory"
  if python3 - "$frame_directory" >> "$out/frame-pixels.log" <<'PY'
import sys
from pathlib import Path
from PIL import Image

visible = False
for path in sorted(Path(sys.argv[1]).glob('*.png')):
    with Image.open(path) as image:
        image.load()
        pixels = list(image.convert('RGB').getdata())
        red = sum(r > 180 and g < 100 and b < 100 for r, g, b in pixels)
        white = sum(min(r, g, b) > 235 for r, g, b in pixels)
        print(f'{path}: red={red} white={white}', flush=True)
        visible |= image.width >= 600 and image.height >= 400 and red > 500 and white > 100_000
sys.exit(0 if visible else 1)
PY
  then
    presented=1
    printf 'Real compositor red stroke and opaque canvas verified.\n' > "$BLURMASK_WAYLAND_CAPTURE_ACK"
    break
  fi
  sleep .1
done
[[ "$presented" == 1 ]] || { echo 'Canvas was not visible in fresh compositor frames within the bounded presentation wait.' >&2; exit 1; }
app_result=0
wait "$app_pid" || app_result=$?
app_pid=
[[ "$app_result" == 0 ]] || { echo "Native Wayland probe exited $app_result." >&2; exit 1; }
[[ "$ready" == 1 ]] || { echo 'Native canvas PNG was not produced.' >&2; exit 1; }
grep -q 'Linux window backend: Wayland;' "$out/app.log"
grep -q 'Native Wayland canvas: configured .*red stroke pixels=' "$out/app.log"
# DISPLAY was absent and XWayland was never launched; actual protocol traffic is
# additional evidence that a Wayland surface was configured and committed.
grep -Eq 'xdg_(wm_base|surface|toplevel)[@#]' "$out/app.log"
grep -Eq 'wl_surface[@#].*commit\(' "$out/app.log"
python3 - "$out" <<'PY'
import json, sys
from pathlib import Path
from PIL import Image

out = Path(sys.argv[1])
def inspect(path):
    with Image.open(path) as image:
        image.load()
        assert image.width >= 600 and image.height >= 400, (path, image.size)
        pixels = list(image.convert('RGB').getdata())
        red = sum(r > 180 and g < 100 and b < 100 for r, g, b in pixels)
        white = sum(min(r, g, b) > 235 for r, g, b in pixels)
        return dict(file=str(path.relative_to(out)), width=image.width, height=image.height,
                    red_pixels=red, white_pixels=white)

rendered = inspect(out / 'app/wayland-native-canvas.png')
before = [inspect(p) for p in sorted((out / 'captures/before').glob('*.png'))]
presented = [inspect(p) for p in sorted((out / 'captures/canvas').rglob('*.png'))]
report = dict(backend='native-wayland', compositor='Weston headless/Pixman',
              rendered=rendered, baseline=before, presented=presented,
              limitation='Programmatic canvas drawing; no physical input, GPU or Hyprland coverage.')
(out / 'result.json').write_text(json.dumps(report, indent=2) + '\n')
assert rendered['red_pixels'] > 500, rendered
assert before and all(p['red_pixels'] < 100 for p in before), before
assert any(p['red_pixels'] > 500 and p['white_pixels'] > 100_000 for p in presented), \
    'Canvas stroke must be visible and opaque in the compositor framebuffer: ' + str(presented)
print(json.dumps(report, indent=2))
PY
echo 'Native Wayland configured surface, rendered canvas and compositor pixels passed.'
