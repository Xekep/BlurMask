# Performance probes

## Scope

Desktop probes measure CPU time and managed allocations on a single thread, not GPU frame time, process RSS or network latency. Each probe warms up, forces GC and then runs the reported iteration count. No timing thresholds run in CI: shared runners cannot produce stable performance comparisons.

Isolated desktop smoke tests suppress the OS tray icon/taskbar entry and do not register global hotkeys; synthetic hotkey messages address only the app's own HWND. Full desktop/mask smoke tests run on CI runners. After an Explorer crash on the development desktop, further local full UI stress runs were stopped. Windows recorded `explorer.exe` / `twinui.pcshell.dll` / `0xc0000005`. Offline analysis with the matching Microsoft PDBs located a null read in `IWindowTabView::Id`, called by `WindowTabHost::OnActiveTabChanged` and `WindowTabDataModel::UpdateActiveTab` through `UpdateTabsForWindow`. This identifies the Windows window-tab handler that failed; it does not establish which application/event triggered it, or verify a fix for Explorer. The recovered Explorer process remained running through the subsequent server-only load tests.

```powershell
dotnet build src/BlurMask -c Release -p:PublishAot=false -p:SelfContained=false
dotnet src/BlurMask/bin/Release/net11.0/BlurMask.dll --performance-test
# Or use the published Native AOT executable with --performance-test.
```

A 20,000-point stroke is received in 400 packets of 50 points. Before/after measurements use the same Release managed runtime on the development PC. Allocations are cumulative, not peak live memory:

| Probe | Before | After |
|---|---:|---:|
| Merge packets, allocations per stroke | 30.68 MiB | 0.42 MiB |
| Build remote drawable ink, allocations per stroke | 213.53 MiB | 1.43 MiB |
| Capture an already complete stroke 400 times, allocations | 305.34 MiB | 0.76 MiB |
| Read bounds 400 times, time per iteration | 189.06 ms | 0.011 ms |

The capture probe measures redundant capture calls with no additional points; it does not model 400 different full-sized strokes. CPU timings vary between runs. Bounds caching invalidates on geometry/text/font changes; clipping skips offscreen committed/remote marks. Active ink remains visible at the viewport boundary. Undo, movement, fading, raster and collaboration tests exercise the optimized code.

## Attached optical masks

Glass/scramble source refresh no longer runs synchronously for every window position notification. Movement bursts share a 16 ms timer that samples the latest HWND rectangle; native child planes follow the parent immediately. Stationary sources refresh every 100 ms, and hidden masks stop both timers. Pure movement does not rebuild tile regions. This bounds requested work; it does not guarantee 60 rendered frames/s from Windows Magnification or DWM.

Tile regions are now submitted with one `ExtCreateRegion` call per plane using pooled RGNDATA instead of creating/combining/deleting a GDI region for each tile. `--mask-performance-test` verifies exact equivalence with `EqualRgn`, including empty regions, without creating any windows or desktop captures. Local managed Release timings for all four planes (2026-10-11):

| Mask size / effect | Previous region construction | Batched construction |
|---|---:|---:|
| 1280×720 glass | 5.12 ms | 0.89 ms |
| 1920×1080 glass | 14.63 ms | 1.78 ms |
| 1280×720 scramble | 246.93 ms | 12.53 ms |
| 1920×1080 scramble | 910.79 ms | 28.03 ms |

These are warmed CPU/GDI measurements of region rebuilding during resize, not end-to-end drag latency or GPU frame time. CI separately verifies that 120 attached-window moves do not synchronously capture desktop pixels, that the final source follows the final mask coordinates without rebuilding regions, and that hidden masks stop capture.

## Collaboration changes

Pen/highlighter rendering uses bounded quadratic rounding of gentle turns;
intentional sharp corners and all original samples stay intact. The same cached
vector geometry is used for local and remote ink, without an enlarged framebuffer.
Raster marks explicitly use high-quality downsampling. Manual text now uses the
embedded Inter face, including legacy empty-font marks, so peers do not select
different OS default fonts. Idle live canvases no longer wake a 20 Hz preview timer;
the timer runs only when local ink changes.

The isolated native collaboration probe opens two separate CanvasWindow/session
clients with independent caches against the real loopback PostgreSQL API. It checks
live previews before commit, alpha, cancellation, committed preview deduplication,
all saved mark types, raster HTTP fallback, rotation/nonuniform scaling, text/math,
read-only controls/server rejection, reconnect, and pixel parity at 50%, 25% and
10% zoom. PNG pairs/diff images and per-region metrics are archived by CI. This
does not cover physical mixed-OS/GPU rendering. Fading ink is currently deliberately
local and is tested as such; typing and selection transforms appear to peers on
commit. Privacy masks and desktop ink are not part of a shared canvas.

- Sender/receiver append only new points; packet lists and accumulator lists have separate ownership.
- Clients must negotiate `blurmask.live.v2`; no v1 or JSON point-delta fallback remains. A six-point delta is 138 bytes rather than 314 bytes of JSON. Saved patches also use binary double coordinates, with JSON metadata sent once. Coordinate precision is unchanged. Socket admission checks credentials/revision and retrieves only committed mark IDs when needed, rather than decoding the complete JSONB document per participant.
- Network work runs off the UI thread. An idle sender waits for input rather than waking at 20 Hz.
- Healthy shared canvases receive saved patches with base/current revision over the existing socket. Missed revisions and large/image updates fall back to an authenticated HTTP snapshot.
- Idle control reads occur every 25–35 seconds, spread across clients, with role-specific ETags. Unchanged reads authorize first and skip JSONB extraction/deserialization and the response body. Local cache files are not rewritten on unchanged reads.
- Server broadcast serializes once for all recipients. Each board has its own lock; IP admission uses counters. Most connections reserve a 4 KiB receive buffer, growing up to 64 KiB only when needed. Slow consumers have bounded queues and reconnect.
- Preview points use per-board counters and an atomic global budget (`Live__MaxPreviewPoints`, default 2,000,000). Cancel, commit, expiry, rejection and disconnect release that capacity. A large connection limit cannot allocate unbounded active-stroke memory.
- PostgreSQL access uses the board primary key and a shared Npgsql pool. A durable write still validates and rewrites the bounded board JSONB under a row lock. Very large, frequently edited boards remain the main storage bottleneck.

One socket per native client supports push, ink and notifications. Total connection count and active drawing load are different capacities. Network egress grows with the number of receivers on each board. The current single-process hub needs a shared broker and board routing before deploying multiple replicas; increasing a configured limit does not provide horizontal scaling.

## Process memory regression (2026-10-11)

Isolated Xvfb/picom/Openbox runs sampled `/proc/<own-pid>/status`,
`smaps_rollup` and full `smaps` for the actual v1.2.4 package and 811725b.
1600×1000 / 3840×2160 and default / CPU rendering were tested. No user's desktop
was used. Numbers below are MiB, not decimal MB, and exclude compositor/Xvfb processes.

| Scenario | RSS | PSS | Peak RSS |
|---|---:|---:|---:|
| v1.2.4, 4K, desktop pen before AI | 169.7 | 156.5 | 200.9 |
| v1.2.4, 4K, after AI pen | 471.8 | 458.6 | 502.9 |
| 811725b, 4K, desktop pen, optional models absent | 171.6 | — | — |

The old AI increase is 302.1 MiB: 205.7 MiB anonymous allocations and 96.3 MiB
resident executable/model pages. English and Russian float32 tensor dimensions
account for 183.3 MiB of the anonymous increase. These old embedded raster models
have been removed. The new model-free measurement is **not** a comparison of two
loaded recognition engines. Six close/reopen canvas cycles showed no accumulating
RSS (−0.1 MiB). In this software-driver environment, forcing CPU rendering saved
about 48–50 MiB; physical GPU/Hyprland memory was not measured.

The new headless Native AOT Windows probe with both English and formula models,
30 recognitions each, measured 88.9 MiB after GC, 111.9 MiB peak, and 24.3 MiB
after model disposal/GC. It excludes window and framebuffer memory. Formula pack
graphs now borrow validated slices instead of duplicating the pack during loading;
managed diagnostic peak fell from 145.2 to 127.7 MiB on the same input. Diagnostic
GC is confined to probes and is not a production memory policy.

In the 1600×1000 CPU probe of bb78e1c, ten-second idle intervals measured
0–0.10% of one core in both default and CPU modes (timer ticks have finite
resolution). A bounded short stroke plus settling over about two seconds measured
1.97% / 4.35%; Picom measured 0% in that interval. This short, model-free Xvfb
workload is not a sustained drawing benchmark, a physical Hyprland/GPU measurement
or a temperature test. CPU and full-frame blur on the user's laptop still require
measurement on that hardware. Run [38130801420](https://github.com/Xekep/BlurMask/actions/runs/38130801420)
contains the raw per-process stat timestamps/ticks and CPU interval summaries.

Reproduce installed-app measurements with `tools/test-linux-memory.sh` in an
isolated unprivileged container. The headless app command is:

```sh
BlurMask --recognition-memory-test --text-model /path/en-US.ikrpack --math-model /path/math2.ikrmath --input /path/strokes.json
```

The full UI matrix and old AI confirmation are archived in GitHub Actions runs
[38129120038](https://github.com/Xekep/BlurMask/actions/runs/38129120038) and
[38129441464](https://github.com/Xekep/BlurMask/actions/runs/38129441464).

## Opt-in server load tests

`src/BlurMask.Server/tests/load.mjs` checks every preview recipient, point order, cross-board isolation, persisted point equality, conditional reads and health latency. It creates uniquely prefixed test boards and writes their IDs separately for bounded cleanup. It never prints invitation keys. It paces public HTTP traffic below admission limits and stops on errors or health latency over two seconds.

```powershell
$env:PEN_TEST_URL='http://127.0.0.1:5095'
$env:PEN_LOAD_BOARDS='4'
$env:PEN_LOAD_USERS='4'
$env:PEN_LOAD_WRITERS='2'
$env:PEN_LOAD_SECONDS='6'
$env:PEN_LOAD_OUTPUT='.artifacts/load-local.json'
node src/BlurMask.Server/tests/load.mjs
```

Online probes require explicit `PEN_LOAD_ONLINE=yes`. `PEN_LOAD_HOST=127.0.0.1` runs from the server through its HTTPS nginx vhost with the public SNI name and normal certificate validation. Only the transport is loopback: the API, WebSocket proxy, auth and PostgreSQL are real. Different bound loopback source addresses model different clients while retaining IP limits. This does not measure Internet bandwidth or distant users' latency. `PEN_LOAD_STOP_FILE` lets a resource monitor request a graceful stop.

For an external fan-out probe, `PEN_LOAD_ACCESS_FILE` can reference a private JSON seed `{prefix, rooms}` containing newly created, empty `BoardAccess` objects. The probe verifies the unique Load-prefixed titles and empty marks. Keep this file outside git and never publish it: it contains test access keys. This permits normal creation from server loopback without relaxing the public per-IP creation quota.

Local 16-client / four-board run: eight simultaneous writers, 960 preview packets, all 2,880 expected deliveries; p95 1.22 ms, p99 1.42 ms, 16 successful saves and four HTTP 304s. This is a loopback integration measurement, not an Internet latency promise.

The v2 repeat delivered the same 2,880 previews without errors at 19.94 frames/s per writer (p95 1.38 ms, p99 1.79 ms), with all 16 saves and four 304s. The website image is a real screenshot: `--website-preview --software-rendering`, with `BLURMASK_SITE_PREVIEW` naming the output PNG, draws a reproducible rocket sketch using the app's actual ink objects and captures its window and floating toolbar.

The production host has three vCPUs and about 3.3 GiB RAM; the application service is constrained to 150% CPU and 768 MiB memory. Load generation also consumes resources on that host for loopback TLS tests. Results from 2026-10-10 UTC:

| Route | Boards / connections / writers | Deliveries | Preview p95 / p99 | Result |
|---|---|---:|---:|---|
| External HTTPS from development PC | 8 / 16 / 16 | 9,600 | 49 / 184 ms | All delivered; 160 saves, 8 conditional 304s |
| Server TLS proxy, loopback generator | 20 / 200 / 40 | 86,400 | 78 / 108 ms | All delivered; 160 saves, 20 conditional 304s |
| Server TLS proxy after nginx tuning | 200 / 2,000 / 200 | 1,080,000 | 555 / 683 ms | All delivered; 2,000 saves, 200 conditional 304s |
| External HTTPS, before binary protocol | 4 / 512 / 16 | 553,332 / 609,600 | 14.74 s / — | Failed: slow recipients disconnected; 336 MiB received |
| External HTTPS, required v2 protocol | 4 / 512 / 16 | 609,600 | 369 / 713 ms | All delivered at 20 Hz; 80 saves, 4 conditional 304s |
| Server TLS proxy, required v2 protocol | 200 / 2,000 / 200 | 1,080,000 | 544 / 651 ms | All delivered; 2,000 saves, 200 conditional 304s; actual 4.84 Hz |
| Server TLS proxy, v2 with 20 active boards | 200 / 2,000 / 20 | 54,000 | 46 / 81 ms | All delivered at 20 Hz; 100 saves, 200 conditional 304s |

No unexpected disconnects, cross-board ink, point-order errors, OOMs or application restarts occurred in these successful runs. The 2,000-connection run targeted 20 frames/s per writer but took 117.07 seconds for 600 frames per writer: **only 5.13 frames/s were actually generated**. It demonstrates connection admission and delivery integrity under this workload; it does not demonstrate 200 writers at 20 Hz or smooth drawing at that scale. Its generator shared the production host's three CPUs, so the result mixes generator/proxy/application costs. Health latency remained below 457 ms. The service cgroup's kernel-recorded memory peak was 242.82 MiB; this includes its charged file/socket memory, not just managed allocations.

The expected aggregate traffic and active-writer count must accompany any capacity claim. Thousands of idle connections, hundreds of active writers, and thousands of users drawing on one board are separate cases. A sustained high-write workload additionally needs stronger CPU resources and, for multiple server processes, an event broker and board routing. The PostgreSQL row/JSONB snapshot write remains a candidate for a storage redesign. Clients now request a compact write receipt when their exact base revision is known; a concurrent edit or lost-response retry falls back to a full snapshot.

## Screenshot rendering at reduced zoom

`tools/BlurMask.RenderProbe` creates no windows and uses the production ink document
with Avalonia Headless and CPU Skia. A 200-frame Windows run compared `ccc6f91` with
the corrected reduced-image cache. The synthetic input is a 3840×2160 PNG with fine
detail. Values below are milliseconds per offscreen render, not compositor FPS.

| Zoom | Repeated render before / after | Changed snapshot + render before / after |
|---|---:|---:|
| 100% | 10.47 / 9.21 | 30.70 / 8.90 |
| 25% | 17.70 / 0.26 | 42.83 / 0.30 |
| 10% | 2.76 / 1.58 | 30.67 / 1.53 |

The original raster remains available for export and zooming in. One reduced bitmap
is retained per image; transforms and synchronization of unchanged PNGs preserve
the decoded raster. The first 25% draw increased from 41.8 to 53.4 ms because it also
creates the reduced bitmap. These results do not establish physical pointer latency,
GPU speed or lower laptop temperature. A standalone pen render showed no speedup.

Full-resolution pixels and 25% image pixels matched the earlier renderer. At 10%,
mean RGB difference was 2.66/255; the independent area-reference detail gate passed
and deliberately blurred images failed. CI checks image detail and thin-stroke
coverage; actual pointer input and compositor output are tested separately on Arch
and Void. `BoardSurface.BitmapCache` was removed after those installed-app tests
caught saved strokes failing to appear despite successful input/persistence.

An unchanged 187-mark, 20,072-point snapshot imported in 0.85 ms instead of 3.52 ms,
allocating 8,930 managed bytes instead of 1,016,117. Unchanged mark instances are
reused. The private snapshot was read only; neither its contents nor rendered pixels
are included in the probe's public artifacts.

The v2 external repeat kept all 512 connections (128 per board), delivered every expected preview and saved every completed stroke at 20 frames/s. Received bytes fell from 352,249,054 in the failed JSON test to 162,438,773 (about 155 MiB), including HTTP bodies and WebSocket framing but excluding HTTP/TLS headers. Its p99 was 713 ms, maximum 2.26 s; this is a short 15-second drawing phase, not a latency guarantee or long-duration soak. The v2 2,000-connection repeat reduced received bytes from 863,401,600 to 434,656,275 but still achieved only 4.84 Hz with its generator on the same host. No errors/restarts/OOMs occurred; the service's cgroup kernel peak after that repeat was 250.64 MiB. Reducing traffic alone did not establish the requested 20 Hz capacity for 200 simultaneous authors.

With `PEN_LOAD_ACTIVE_BOARDS=20`, all 2,000 sockets remained connected while 20 authors drew for 15 seconds at 20.002 frames/s. All 54,000 deliveries, 100 saves and 200 conditional reads passed; preview p95/p99 was 45.95/80.80 ms and health maximum 52.2 ms. This confirms that the connection count itself need not prevent responsive collaboration for a smaller set of active authors. It does not validate 2,000 simultaneous authors or a 2,000-participant single board; the default per-board limit remains 128.

The first large admission probe stopped at 993 established sockets because nginx logged `2048 worker_connections are not enough`. Each proxied socket needs a client and upstream connection; this is a per-worker limit, not an application failure. The host setting was raised to 16,384 per worker, keeping the existing 65,535 file-descriptor limit; `nginx -t` passed before reload. The site's connection limits are 4,224 total / 1,088 per address, leaving HTTP headroom above the application limits. These host settings live in the deployment configuration, outside this repository.
