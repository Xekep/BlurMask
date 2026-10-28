Source-only RU preflight scalar-cache research packet, 2026-10-28. The candidate remains outside production and is not release eligible. Recognition readiness is not established.

All inputs needed to authenticate the patch and reproduce its synthetic helper tests are included here. `source-identity.json` pins the four unchanged candidate source files, their production preimage where applicable, and the local `fake-streams.json`. Resolve `expectedStreamsPath` relative to this packet directory. No E: drive is required for these checks or helper tests. The repository SDK pin and .NET 10 runtime are required for compilation/execution.

From the repository root, authenticate without building:

```powershell
python tools/handwriting/ports/ru-scalar-cache-20261028/verify_source.py
git apply --check tools/handwriting/ports/ru-scalar-cache-20261028/correction.diff
```

To reproduce the isolated helper probe on a clone, use the included local oracle:

```powershell
dotnet build tools/handwriting/ports/ru-scalar-cache-20261028/probe/Probe.csproj -c Release --artifacts-path "$env:TEMP/BlurMask-ru-cache-probe" -m:1 --disable-build-servers -p:UseSharedCompilation=false -nodeReuse:false
dotnet exec --fx-version 10.0.12 --roll-forward Disable "$env:TEMP/BlurMask-ru-cache-probe/bin/Probe/release/Probe.dll" tools/handwriting/ports/ru-scalar-cache-20261028/fake-streams.json
```

The isolated probe links the exact candidate helper and uses an authenticated extraction of the existing WorkBudget. It verifies 196 frozen old/new receipt/error-prefix streams plus key, collision, capacity, workspace, immutable-value and cancellation controls. It does not validate constructor integration or perform recognition.

Singleton all275 references repeat immutable owner-row scans, SourceMetadata.Read source DFS/range allocations, compact scans and ordinary replacement resolution. The candidate caches two successful scalar counts per canonical owner while retaining every reference's marker validation, selected-row skip, multiplicity and original row/range debit. Default/batch paths allocate no cache. Class fields and all post-constructor methods remain byte-exact. The synchronous constructor requires stable source graph/context and owned copies; concurrent mutation is not supported.

The paused draft's probe compile error and packed-owner hash clustering were fixed. The cache remains bounded to 4096 owners, 8192 slots and 131200 conservative bytes, with cancellation during probes. Its workspace comparison is a local ceiling check, not a cumulative allocation debit or a GC/RSS bound. No model, score, input, cap, threshold or selected-output policy changes are included.

For future integration research, authenticate the current preimage and apply `correction.diff` in a separate checkout, or copy the four `source/` files to their matching relative paths. Preserve every other file. Build the library and `source/tools/WholeWordPreflightCache.Probe` in that separate checkout; `ReviewedLibraryPath` can bind the integrated probe to the exact candidate DLL. Pass this packet's `fake-streams.json` explicitly. This packet does not authorize production admission.

See [verification.md](verification.md) for completed full-build and diagnostic evidence, including its limits. Historical E: paths there identify existing captures only; they are not prerequisites for the portable checks above. Candidate raw source hashes are unchanged, and repository attributes preserve the packet's exact bytes across Git checkouts.

The public-path follow-up is complete: 34/36 candidate calls completed and 2 reached the unchanged deadline. All 22 common historical completions matched structurally, and all 34 candidate completions retained neural-count reuse. A same-session old-baseline control also completed the original 9/9 inputs with identical structure (one batch: old 7.04 seconds, candidate 7.50 seconds). No latency or acceptance benefit is established; this candidate remains outside production. Details and limits are in `verification.md` and `verification.json`.
