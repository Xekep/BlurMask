# Performance probes

## Scope

Desktop probes measure CPU time and managed allocations on a single thread, not GPU frame time, process RSS or network latency. Each probe warms up, forces GC and then runs the reported iteration count. No timing thresholds run in CI: shared runners cannot produce stable performance comparisons.

Isolated desktop smoke tests suppress the OS tray icon/taskbar entry and do not register global hotkeys; synthetic hotkey messages address only the app's own HWND. Full desktop/mask smoke tests run on CI runners. After an Explorer crash on the development desktop, further local full UI stress runs were stopped. Windows recorded `explorer.exe` / `twinui.pcshell.dll` / `0xc0000005`. Offline analysis with the matching Microsoft PDBs located a null read in `IWindowTabView::Id`, called by `WindowTabHost::OnActiveTabChanged` and `WindowTabDataModel::UpdateActiveTab` through `UpdateTabsForWindow`. This identifies the Windows window-tab handler that failed; it does not establish which application/event triggered it, or verify a fix for Explorer. The recovered Explorer process remained running through the subsequent server-only load tests.

```powershell
dotnet build -c Release -p:PublishAot=false
dotnet bin/Release/net11.0/BlurMask.dll --performance-test
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

- Sender/receiver append only new points; packet lists and accumulator lists have separate ownership.
- Clients must negotiate `blurmask.live.v2`; no v1 or JSON point-delta fallback remains. A six-point delta is 138 bytes rather than 314 bytes of JSON. Saved patches also use binary double coordinates, with JSON metadata sent once. Coordinate precision is unchanged. Socket admission checks credentials/revision and retrieves only committed mark IDs when needed, rather than decoding the complete JSONB document per participant.
- Network work runs off the UI thread. An idle sender waits for input rather than waking at 20 Hz.
- Healthy shared canvases receive saved patches with base/current revision over the existing socket. Missed revisions and large/image updates fall back to an authenticated HTTP snapshot.
- Idle control reads occur every 25–35 seconds, spread across clients, with role-specific ETags. Unchanged reads authorize first and skip JSONB extraction/deserialization and the response body. Local cache files are not rewritten on unchanged reads.
- Server broadcast serializes once for all recipients. Each board has its own lock; IP admission uses counters. Most connections reserve a 4 KiB receive buffer, growing up to 64 KiB only when needed. Slow consumers have bounded queues and reconnect.
- Preview points use per-board counters and an atomic global budget (`Live__MaxPreviewPoints`, default 2,000,000). Cancel, commit, expiry, rejection and disconnect release that capacity. A large connection limit cannot allocate unbounded active-stroke memory.
- PostgreSQL access uses the board primary key and a shared Npgsql pool. A durable write still validates and rewrites the bounded board JSONB under a row lock. Very large, frequently edited boards remain the main storage bottleneck.

One socket per native client supports push, ink and notifications. Total connection count and active drawing load are different capacities. Network egress grows with the number of receivers on each board. The current single-process hub needs a shared broker and board routing before deploying multiple replicas; increasing a configured limit does not provide horizontal scaling.

## Opt-in load tests

`server/tests/load.mjs` checks every preview recipient, point order, cross-board isolation, persisted point equality, conditional reads and health latency. It creates uniquely prefixed test boards and writes their IDs separately for bounded cleanup. It never prints invitation keys. It paces public HTTP traffic below admission limits and stops on errors or health latency over two seconds.

```powershell
$env:PEN_TEST_URL='http://127.0.0.1:5095'
$env:PEN_LOAD_BOARDS='4'
$env:PEN_LOAD_USERS='4'
$env:PEN_LOAD_WRITERS='2'
$env:PEN_LOAD_SECONDS='6'
$env:PEN_LOAD_OUTPUT='.artifacts/load-local.json'
node server/tests/load.mjs
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

The expected aggregate traffic and active-writer count must accompany any capacity claim. Thousands of idle connections, hundreds of active writers, and thousands of users drawing on one board are separate cases. A sustained high-write workload additionally needs stronger CPU resources and, for multiple server processes, an event broker and board routing. The PostgreSQL row/JSONB snapshot write and full HTTP snapshot acknowledgement remain candidates for the next storage/protocol redesign.

The v2 external repeat kept all 512 connections (128 per board), delivered every expected preview and saved every completed stroke at 20 frames/s. Received bytes fell from 352,249,054 in the failed JSON test to 162,438,773 (about 155 MiB), including HTTP bodies and WebSocket framing but excluding HTTP/TLS headers. Its p99 was 713 ms, maximum 2.26 s; this is a short 15-second drawing phase, not a latency guarantee or long-duration soak. The v2 2,000-connection repeat reduced received bytes from 863,401,600 to 434,656,275 but still achieved only 4.84 Hz with its generator on the same host. No errors/restarts/OOMs occurred; the service's cgroup kernel peak after that repeat was 250.64 MiB. Reducing traffic alone did not establish the requested 20 Hz capacity for 200 simultaneous authors.

With `PEN_LOAD_ACTIVE_BOARDS=20`, all 2,000 sockets remained connected while 20 authors drew for 15 seconds at 20.002 frames/s. All 54,000 deliveries, 100 saves and 200 conditional reads passed; preview p95/p99 was 45.95/80.80 ms and health maximum 52.2 ms. This confirms that the connection count itself need not prevent responsive collaboration for a smaller set of active authors. It does not validate 2,000 simultaneous authors or a 2,000-participant single board; the default per-board limit remains 128.

The first large admission probe stopped at 993 established sockets because nginx logged `2048 worker_connections are not enough`. Each proxied socket needs a client and upstream connection; this is a per-worker limit, not an application failure. The host setting was raised to 16,384 per worker, keeping the existing 65,535 file-descriptor limit; `nginx -t` passed before reload. The site's connection limits are 4,224 total / 1,088 per address, leaving HTTP headroom above the application limits. These host settings live in the deployment configuration, outside this repository.
