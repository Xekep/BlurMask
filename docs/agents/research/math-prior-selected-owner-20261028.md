# Current applied Math result: a separate selected owner

The library can now create a bounded selected copy of an actually applied Math
result. This closes the source-authority seam after the
[fresh retained Ink](math-retained-ink-owner-20261028.md) increment. It does not
run a second inference, produce a nonfresh diff, update an existing result or
enable Russian/English cursive recognition in the application.

## Responsibility and authority

[Math2Mode0OriginalApplication](../../../src/InkRecognition.Math/Math2Mode0OriginalApplication.cs)
owns the current applied DOM arena. The original field's historical scaffold
is not the current result. Its new `WithCompletedOriginalInput` enters the
existing live receipt/field/publisher/outgoing/application chain before issuing
a private-constructor `PriorBorrow`. The token is usable only while it is the
application's exact active borrow under those gates. A `finally` block revokes
it on success, exception or cancellation; reentrant issuance refuses.

The application must have completed source execution, journal and DOM mutation.
It validates the current registry, Next, canonical names, typed tags and exactly
one actual mathResult child. `priorReady` is set only after the completion
observer returns: the observer cannot create a successor from provisional
`complete=true` state.

[Math2Mode0PriorSelectedScope](../../../src/InkRecognition.Math/Math2Mode0PriorSelectedScope.cs)
owns a new selected arena and a distinct current-Ink clone. Its field factory
reads the private live original registry under the actual borrowed authority.
It preserves current IDs, category, names, Next, range bytes, empty-storage state
and shared C22 aliases using a reference-identity map. It retains the original
name owner, rather than the old fresh selected name owner. Neither snapshots nor
caller-provided arrays can restore this authority.

Every scope access reenters the ancestor chain before taking its own gate and
checks the applied epoch and actual arena/Ink/name identities. Disposing the
scope or a required ancestor invalidates access. Inspection arrays are detached.
The scope exposes no inference, registration, diff or document mutation API.

## Bounds and cancellation

The preflight bounds the selected field subtree to65536 nodes/depth240,
the registry to65536 entries, distinct C22 ranges to65536 and UTF8 values to2MiB.
The conservative clone-payload reservation is
`8192 + nodes*1024 + entries*512 + ranges*128 + utf8Bytes*8`, with a16MiB ceiling.
One-short and exact reservations are checked before DOM/C22 cloning. Cancellation
is checked during preflight, registry cloning and arena cloning.

This reservation does not bound total managed allocation or process RSS:
preflight itself uses inspection rows and a set of distinct source C22s.
No source registry, old claim, model, score or acceptance ceiling is relaxed.

## Verification and remaining work

[LegacyMathPriorSelectedScope.Probe](../../../tools/LegacyMathPriorSelectedScope.Probe/Program.cs)
first creates the real model/official-ORT result through fresh publication,
outgoing transfer, source execution and actual original DOM application. Its61
controls cover the applied owner, current Next/name domain, alias isomorphism,
detached snapshots, exact/one-short budgets, cancellation and refusal of
prepared/source-only/failed/rolled-back/closed/foreign/reentrant/escaped access.
The foreign source uses the same raw document, so document equality cannot
substitute for owner identity. Callback exceptions revoke the borrow as well.

The [independent oracle](../../../tools/LegacyMathPriorSelectedScope.Probe/audit_output.py)
replays the26-node selected subtree directly from the actual applied capture,
including attribute-before-child order, reindexed parent IDs, current registry
and alias groups. It also compares managed/native captures when supplied.
This fixture is a source-ownership test; it is not a recognition-quality corpus.

Local root verification is under
`E:/BlurMask-build-artifacts-20261014/math-prior-owner-20261028/`.
All72 authored Math files are authenticated against their compiled portable PDB.
The previous three owning pipelines remain separate regression checks. The
dedicated ownership workflow includes this fourth probe on all three OSes.
All four local managed and Windows Native AOT paths passed their independent
oracles and produced byte-identical paired captures. The three previous managed
captures also remain byte-identical to the preceding retained-Ink increment.
The reviewed Math DLL SHA256 is
`9f49cf5d65da31a014d635dd46b3c21868f03d6536bf5f3d781f44c19cf0441c`.
The four-probe managed/Native AOT matrix also passed on Windows, Linux and
macOS for commit `4436c4f`:
[run40753321233](https://github.com/Xekep/BlurMask/actions/runs/40753321233).

Remaining: current-result freeze preparation, inference bound to this new scope,
the second selected-to-retained clone, genuine old/new DOM diff and opcode7
matching, and a receiver that updates the same active document/registry with an
epoch check. There is no current mutation API for a second epoch, so this test
does not fake an epoch change through reflection. The existing fresh singleton
guard and opcode2-only receiver remain intact.
