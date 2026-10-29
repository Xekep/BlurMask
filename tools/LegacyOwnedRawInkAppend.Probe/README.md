# Raw append ownership regression

Six byte-pinned, label-free append geometries and 279 controls exercise the actual production `LegacyOwnedRawInkAppendState`. No implementation copy, model, NN, Math registration or public request is involved. The existing `LegacyRawInkCurrentView.Probe` friend assembly name is reused; no production friend grant is added.

The original 271 controls cover prefix C49/view reference retention, immutable historical documents, exact float32 projection bits and indices, fresh/stale/foreign/forged receipt refusal, borrow revocation/reentrancy/disposal, canceled and over-budget reads before point access, exact mid-copy cancellation prefix, coordinate and object/point caps, prepared-token ownership, abandon/commit lifecycle and successor-pending cleanup. The additional eight controls use two actual threads: Append holds a supplied-point reader while a competitor tries to capture/prepare, then the competitor must see the committed receipt and retain a committable successor token. This checks serialization and cleanup behavior; it is not a deterministic reproduction of every possible historical scheduler interleaving.

Build from repository root with the normal Core937 source present:

```text
dotnet build tools/LegacyOwnedRawInkAppend.Probe/LegacyOwnedRawInkAppend.Probe.csproj -c Release --artifacts-path <output> -m:1 --disable-build-servers -nodeReuse:false -p:UseSharedCompilation=false -p:DebugType=portable -p:DebugSymbols=true
dotnet exec --fx-version 10.0.12 --roll-forward Disable <output>/bin/LegacyOwnedRawInkAppend.Probe/release/LegacyRawInkCurrentView.Probe.dll tools/LegacyOwnedRawInkAppend.Probe/append-fixtures.json
```

Use CPU2, a 600-second build watchdog and a 45-second execution watchdog. The fixture SHA256 is 80b88eb48371bf74e19c048f8b8de61ad8ac0a20873093aba6b90cf8d7262a1e. Its original CRLF bytes are intentionally retained by this folder's `.gitattributes`; source files use ordinary repository LF policy. Do not regenerate fixtures from observed recognition output.

This probe verifies raw-state authority and projection only. It does not prove public incoming addItems/request ownership, native revisions/A8 rollback, C51/INPUT update, changed C23, candidate/diff application or recognition quality. Those have separate owning integration probes and native evidence.
