# Performance probes

## Scope

Desktop probes measure CPU time and managed allocations on a single thread, not GPU frame time, process RSS or network latency. Each probe warms up, forces GC and then runs the reported iteration count. No timing thresholds run in CI: shared runners cannot produce stable performance comparisons.

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

## Collaboration changes

- Sender/receiver append only new points; packet lists and accumulator lists have separate ownership.
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

Local 16-client / four-board run: eight simultaneous writers, 960 preview packets, all 2,880 expected deliveries; p95 1.22 ms, p99 1.42 ms, 16 successful saves and four HTTP 304s. This is a loopback integration measurement, not an Internet latency promise.

Online capacity results are recorded after staged production probes. The production host has three vCPUs and about 3.3 GiB RAM; the application service is constrained to 150% CPU and 768 MiB memory. Load generation also consumes resources on that host for loopback TLS tests.

The first large admission probe stopped at 993 established sockets because nginx logged `2048 worker_connections are not enough`. Each proxied socket needs a client and upstream connection; this is a per-worker limit, not an application failure. The host setting was raised to 16,384 per worker, keeping the existing 65,535 file-descriptor limit; `nginx -t` passed before reload. The site's connection limits are 4,224 total / 1,088 per address, leaving HTTP headroom above the application limits. These host settings live in the deployment configuration, outside this repository.
