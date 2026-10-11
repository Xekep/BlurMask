#!/usr/bin/env bash
# Measure an installed Linux app in an isolated container. Never run on a user's desktop.
set -euo pipefail
[[ "$EUID" != 0 ]] || { echo 'Run as an unprivileged test user.' >&2; exit 2; }
exe="$(realpath "${1:?Pass the installed executable}")"
out="$(realpath -m "${2:?Pass a writable artifact directory}")"
shift 2
screen="${BLURMASK_MEMORY_SCREEN:-1600x1000}"
[[ "$screen" =~ ^[0-9]+x[0-9]+$ ]] || { echo 'Invalid screen geometry.' >&2; exit 2; }
[[ "${BLURMASK_MEMORY_CPU_ONLY:-}" != 1 || "${BLURMASK_MEMORY_AI_ONLY:-}" != 1 ]] || {
  echo 'Choose CPU_ONLY or AI_ONLY, not both.' >&2; exit 2;
}
mkdir -p "$out"
[[ ! -e "$out/memory.jsonl" ]] || { echo 'Use a fresh artifact directory.' >&2; exit 2; }
if [[ "${BLURMASK_MEMORY_XVFB:-}" != 1 ]]; then
  export BLURMASK_MEMORY_XVFB=1
  exec xvfb-run -a -s "-screen 0 ${screen}x24 +extension GLX +extension RENDER" bash "$0" "$exe" "$out" "$@"
fi
export LANG=C.UTF-8 XDG_DATA_HOME="$out/data" XDG_CONFIG_HOME="$out/config" XDG_CACHE_HOME="$out/cache"
unset WAYLAND_DISPLAY
mkdir -p "$XDG_DATA_HOME" "$XDG_CONFIG_HOME" "$XDG_CACHE_HOME"
pids=()
cleanup() {
  result=$?
  xwininfo -root -tree > "$out/final-windows.txt" 2>&1 || true
  for pid in "${pids[@]}"; do kill "$pid" 2>/dev/null || true; done
  for pid in "${pids[@]}"; do wait "$pid" 2>/dev/null || true; done
  exit "$result"
}
trap cleanup EXIT
openbox > "$out/openbox.log" 2>&1 & wm="$!"; pids+=("$wm")
sleep 1
picom --backend xrender --config /dev/null > "$out/picom.log" 2>&1 & compositor="$!"; pids+=("$compositor")
sleep .5
xdpyinfo > "$out/xdpyinfo.txt"
glxinfo -B > "$out/glxinfo.txt" 2>&1 || true
uname -a > "$out/kernel.txt"
sha256sum "$exe" > "$out/executable.sha256"
printf '%s\n' "$screen" > "$out/screen.txt"
printf '%s\n' "$@" > "$out/arguments.txt"
cd "$out"
"$exe" "$@" > "$out/app.log" 2>&1 & app="$!"; pids+=("$app")
printf '%s\n' "$app" > "$out/app.pid"
# All reads concern processes created for this isolated X display. kB in proc means KiB.
cat > "$out/sample.py" <<'PY'
from pathlib import Path
from collections import defaultdict
import json, os, re, sys, time
out, phase, app, wm, compositor = sys.argv[1:]
p = Path(out); dest = p / phase; dest.mkdir(exist_ok=True)
def fields(text):
    return {m[0]: int(m[1]) for m in re.findall(r'^([A-Za-z_]+):\s+(\d+)\s+kB$', text, re.M)}
def sample(pid, name):
    proc = Path('/proc') / pid
    # comm can contain spaces and parentheses: fields after the final ')' start at field 3.
    stat = proc.joinpath('stat').read_text()
    measured_ns = time.monotonic_ns()
    columns = stat.rpartition(')')[2].split()
    assert len(columns) >= 20, 'Incomplete /proc/PID/stat'
    user_ticks, system_ticks, start_ticks = int(columns[11]), int(columns[12]), int(columns[19])
    clock_ticks = os.sysconf('SC_CLK_TCK')
    (dest / f'{name}-stat.txt').write_text(stat)
    status = proc.joinpath('status').read_text()
    rollup = proc.joinpath('smaps_rollup').read_text()
    (dest / f'{name}-status.txt').write_text(status)
    (dest / f'{name}-smaps_rollup.txt').write_text(rollup)
    a, b = fields(status), fields(rollup)
    result = {'pid': int(pid), 'monotonic_ns': measured_ns, 'clock_ticks_per_second': clock_ticks,
              'cpu_user_ticks': user_ticks, 'cpu_system_ticks': system_ticks,
              'cpu_total_ticks': user_ticks + system_ticks, 'process_start_ticks': start_ticks,
              'cpu_seconds': (user_ticks + system_ticks) / clock_ticks,
              'rss_kib': b['Rss'], 'pss_kib': b['Pss'],
              'private_kib': b['Private_Clean'] + b['Private_Dirty'], 'anonymous_kib': b['Anonymous'],
              'pss_anon_kib': b.get('Pss_Anon', 0), 'pss_file_kib': b.get('Pss_File', 0),
              'pss_shmem_kib': b.get('Pss_Shmem', 0), 'hwm_kib': a['VmHWM'], 'virtual_kib': a['VmSize'],
              'swap_kib': b.get('Swap', 0), 'threads': int(re.search(r'^Threads:\s+(\d+)', status, re.M)[1])}
    if name == 'app':
        smaps = proc.joinpath('smaps').read_text(); (dest / 'app-smaps.txt').write_text(smaps)
        groups = defaultdict(lambda: defaultdict(int)); current = None
        for line in smaps.splitlines():
            if re.match(r'^[0-9a-f]+-[0-9a-f]+ ', line):
                parts = line.split(None, 5); current = parts[5] if len(parts) == 6 else '[anonymous]'
            elif current is not None and (m := re.match(r'^(Rss|Pss|Anonymous|Private_Clean|Private_Dirty):\s+(\d+) kB$', line)):
                groups[current][m[1]] += int(m[2])
        result['top_mappings_kib'] = sorted([dict(name=k, **v) for k,v in groups.items()], key=lambda m:m['Rss'], reverse=True)[:16]
    return result
system_stat = Path('/proc/stat').read_text(); (dest / 'system-stat.txt').write_text(system_stat)
row = {'phase': phase, 'monotonic_seconds': time.monotonic(), 'monotonic_ns': time.monotonic_ns(),
       'logical_cpus': os.cpu_count(), 'system_cpu_ticks': list(map(int, system_stat.splitlines()[0].split()[1:])), 'unit': 'KiB',
       'app': sample(app, 'app'), 'openbox': sample(wm, 'openbox'), 'picom': sample(compositor, 'picom')}
# xvfb-run started the server as this same test user; read its per-display lock, never kill it.
display = __import__('os').environ['DISPLAY'].split(':')[1].split('.')[0]
lock = Path(f'/tmp/.X{display}-lock')
if lock.is_file():
    server = lock.read_text().strip()
    if b'Xvfb' in Path('/proc',server,'cmdline').read_bytes(): row['xvfb'] = sample(server, 'xvfb')
(dest / 'memory.json').write_text(json.dumps(row, indent=2))
with (p / 'memory.jsonl').open('a') as f: f.write(json.dumps(row) + '\n')
print(json.dumps({k:v for k,v in row['app'].items() if k != 'top_mappings_kib'}))
PY
sample() { kill -0 "$app"; python "$out/sample.py" "$out" "$1" "$app" "$wm" "$compositor"; xwininfo -root -tree > "$out/$1/windows.txt"; }
visible() { xdotool search --onlyvisible --name "$1" 2>/dev/null | tail -1; }
wait_visible() {
  local id
  for i in {1..80}; do
    kill -0 "$app"; id="$(visible "$1" || true)"
    if [[ -n "$id" ]]; then printf '%s\n' "$id"; return; fi
    sleep .1
  done
  echo "Window did not map: $1" >&2; return 1
}
assert_unmapped() { [[ -z "$(visible "$1" || true)" ]] || { echo "Still mapped: $1" >&2; return 1; }; }
key() { xdotool windowactivate --sync "$toolbar"; xdotool key --clearmodifiers "$1"; sleep .5; }
stroke() {
  local window="$1"; shift
  xdotool mousemove --window "$window" "$1" "$2"; shift 2
  sleep .1; xdotool mousedown 1
  while (( "$#" )); do sleep .1; xdotool mousemove --window "$window" "$1" "$2"; shift 2; done
  sleep .1; xdotool mouseup 1; sleep .5
}
write_summary() {
python - "$out" <<'PY'
from pathlib import Path
import json, os, sys
p = Path(sys.argv[1]); rows = [json.loads(line) for line in (p / 'memory.jsonl').read_text().splitlines()]
keys = ['rss_kib','pss_kib','private_kib','anonymous_kib','hwm_kib','pss_shmem_kib']
intervals = []
idle_pairs = {('startup-cursor', 'startup-idle'), ('desktop-pen-mapped', 'desktop-pen-idle'),
              ('canvas-open', 'canvas-idle')}
for previous, current in zip(rows, rows[1:]):
    pair = previous['phase'], current['phase']
    interval = {'from_phase': pair[0], 'to_phase': pair[1],
                'kind': 'idle' if pair in idle_pairs else 'active-stroke' if pair == ('desktop-active-start', 'desktop-active-stroke') else 'transition',
                'processes': {}}
    for name in ('app', 'picom', 'xvfb', 'openbox'):
        if name not in previous or name not in current: continue
        before, after = previous[name], current[name]
        assert (before['pid'], before['process_start_ticks']) == (after['pid'], after['process_start_ticks']), 'Process changed during sampling'
        elapsed = (after['monotonic_ns'] - before['monotonic_ns']) / 1_000_000_000
        ticks = after['cpu_total_ticks'] - before['cpu_total_ticks']
        assert elapsed > 0 and ticks >= 0
        seconds = ticks / after['clock_ticks_per_second']
        interval['processes'][name] = {'elapsed_seconds': round(elapsed, 6), 'cpu_seconds': round(seconds, 6),
                                        'cpu_percent_one_core': round(100 * seconds / elapsed, 3), 'cpu_ticks': ticks}
    intervals.append(interval)
scenario = 'cpu-only' if os.environ.get('BLURMASK_MEMORY_CPU_ONLY') == '1' else 'ai-only' if os.environ.get('BLURMASK_MEMORY_AI_ONLY') == '1' else 'window-lifecycle'
summary = {'unit': 'MiB', 'scenario': scenario, 'phases': [{ 'phase': r['phase'], **{k.removesuffix('_kib') + '_mib': round(r['app'][k]/1024,3) for k in keys}} for r in rows],
           'cpu_unit': 'percent of one logical CPU (100%=one core)', 'cpu_intervals': intervals,
           'idle_cpu_intervals': [r for r in intervals if r['kind'] == 'idle'],
           'limitations': ['No separate optional model pack is installed. The v1.2.4 baseline embeds legacy weights; the current candidate does not. AI-only compares legacy model loading with missing-model behavior, not two loaded engines.', 'Software Xvfb/Mesa is not a hardware GPU or Hyprland memory measurement.',
                          'RSS includes shared/file pages; PSS apportions shared pages. Native and managed heaps are both part of anonymous memory.',
                          'CPU deltas use each process own /proc/PID/stat user+system ticks and monotonic timestamps. They exclude child CPU and are not normalized by machine core count.',
                          'CPU_ONLY keeps static windows without optional model loading; its short intervals do not measure hardware GPU power, temperature, fan speed or long-term thermal behavior.',
                          'No forced GC, malloc_trim or renderer cache purge. Six reuse cycles in the full window-lifecycle scenario can reveal growth, not prove absence of long-term leaks.']}
(p / 'summary.json').write_text(json.dumps(summary, indent=2)); print(json.dumps(summary, indent=2))
PY

}
toolbar="$(wait_visible 'BlurMask.*drawing tools')"
sleep 3
assert_unmapped 'BlurMask.*ink overlay'
sample startup-cursor
if [[ "${BLURMASK_MEMORY_CPU_ONLY:-}" == 1 ]]; then sleep 10; else sleep 7; fi
sample startup-idle
if [[ "${BLURMASK_MEMORY_CPU_ONLY:-}" == 1 ]]; then
  key ctrl+shift+3
  overlay="$(wait_visible 'BlurMask.*ink overlay')"
  sleep 2
  sample desktop-pen-mapped
  sleep 10
  sample desktop-pen-idle
  sample desktop-active-start
  # One real pointer stroke; include a bounded two-second draw/settle interval.
  active_start="$(python -c 'import time; print(time.monotonic())')"
  stroke "$overlay" 600 550 670 580 740 530 800 590 850 560 900 600 950 530 1000 580
  python - "$active_start" <<'PY'
import sys, time
time.sleep(max(0, 2 - (time.monotonic() - float(sys.argv[1]))))
PY
  sample desktop-active-stroke
  key ctrl+shift+c
  canvas="$(wait_visible 'BlurMask.*Canvas')"
  sleep 2
  sample canvas-open
  sleep 10
  sample canvas-idle
  write_summary
  exit 0
fi
if [[ "${BLURMASK_MEMORY_AI_ONLY:-}" == 1 ]]; then
  key ctrl+shift+3
  overlay="$(wait_visible 'BlurMask.*ink overlay')"
  sleep 2
  sample ai-before-selection
  xdotool mousemove --window "$toolbar" 26 124 click 1
  flyout="$(wait_visible 'BlurMask.*menu')"
  xdotool mousemove --window "$flyout" 54 20 click 1
  sleep .5
  assert_unmapped 'BlurMask.*menu'
  sample ai-selected
  stroke "$overlay" 620 650 660 550 700 650
  stroke "$overlay" 640 615 680 615
  sample ai-after-strokes
  sleep 1
  sample ai-idle-1s
  sleep 4
  sample ai-idle-5s
  sleep 15
  sample ai-idle-20s
  stroke "$overlay" 840 650 880 550 920 650
  stroke "$overlay" 860 615 900 615
  sleep 5
  sample ai-repeat-idle
  key ctrl+shift+2
  sample ai-cursor-hidden
  write_summary
  exit 0
fi
key ctrl+shift+3
overlay="$(wait_visible 'BlurMask.*ink overlay')"
sleep 2
sample desktop-pen-mapped
stroke "$overlay" 600 550 670 580 740 530 800 590
sample desktop-stroke
key ctrl+shift+2
assert_unmapped 'BlurMask.*ink overlay'
sample cursor-after-desktop
key ctrl+shift+c
canvas="$(wait_visible 'BlurMask.*Canvas')"
sleep 2
sample canvas-open
xdotool windowactivate --sync "$canvas"
xdotool key --clearmodifiers ctrl+shift+3
stroke "$canvas" 360 310 430 360 510 330 590 410
sample canvas-stroke
wmctrl -ic "$(printf '0x%x' "$canvas")"
sleep 1
assert_unmapped 'BlurMask.*Canvas'
key ctrl+shift+2
sample canvas-hidden
# Reuse the same native windows, retain the same stroke count: isolate caches from document growth.
for cycle in {1..6}; do
  key ctrl+shift+3
  [[ "$(wait_visible 'BlurMask.*ink overlay')" == "$overlay" ]]
  sleep .5
  sample "cycle-$cycle-desktop"
  key ctrl+shift+c
  [[ "$(wait_visible 'BlurMask.*Canvas')" == "$canvas" ]]
  sleep .7
  sample "cycle-$cycle-canvas"
  wmctrl -ic "$(printf '0x%x' "$canvas")"
  sleep .6
  key ctrl+shift+2
  sample "cycle-$cycle-hidden"
done
# Create every privacy mode through the real flyout, then destroy them through the WM.
for mode in {0..4}; do
  xdotool mousemove --window "$toolbar" 26 358 click 1
  flyout="$(wait_visible 'BlurMask.*menu')"
  xdotool mousemove --window "$flyout" "$((20 + mode * 34))" 20 click 1
  mask="$(wait_visible '^BlurMask$')"
  sleep .7
  sample "mask-$mode-created"
  if (( mode == 0 )); then
    xdotool windowmove "$mask" 0 0
    xdotool windowsize "$mask" "${screen%x*}" "${screen#*x}"
    sleep 1.5
    sample mask-blur-fullscreen
  fi
  wmctrl -ic "$(printf '0x%x' "$mask")"
  sleep .8
  assert_unmapped '^BlurMask$'
  sample "mask-$mode-closed"
done
sleep 10
sample final-idle
write_summary
