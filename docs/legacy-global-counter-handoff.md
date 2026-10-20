# Genuine global pending counter handoff

`LegacyFreshMainGlobalCounterHandoff.Apply(actualTransforms)` appends native
operation7 to the same pending C4D after the genuine global037E80/026EF0 route.
It neither executes that command nor replays registry selections or DOM edits.

The initial counter comes from the exact selected INPUT binding's initial Next,
including every retained INPUT ID. The final counter comes from the same
pending transaction that registered actual CHAR, WORD and TEXT selections.
Chosen tag+1, raw object count, line count and caller-provided counters are not
substitutes. Before appending, the owner checks every command identity and the
actual final TEXT selection. It checks that the pending cursor has not changed
while acquiring the C4D gate.

A NEW companion partial calls the unchanged private numeric07AAF0 append kernel.
That kernel owns its existing single-use flag, equality skip and allocation
failure behavior. If counters differ, the retained command stores newCounter18
before oldCounter20 and follows the original operation4 commands. It does not
advance the selection cursor. `CopyPendingSelections` remains the unchanged
operation4-only view; `CopyPendingCommands` exposes original selection objects
and the actual trailing counter object with owner-local links. No native pointer
or padding bytes are fabricated.

All scope/transform validation precedes the C4D gate; the gate holds only private
numeric state and exact command comparisons, avoiding inverse scope locking.
The completed result captures final Next from its immutable appended command,
then validates the entire owner again before returning. `ValidateCurrent`
separately checks the live source, both counters and every command/reference/link.
Copied command bodies remain historical after source disposal.

Cancellation before entry leaves the transaction retryable. An explicit internal
allocation-failure control poisons the existing pending lifetime, preserves all
operation4 objects and the cursor, appends no operation7 and refuses retries.
Public callers cannot choose counters, allocation status or a replacement C4D.
After success, attempts to append more selections refuse without damaging the
completed handoff. Equality and arbitrary signed numeric controls remain covered
by the unchanged existing numeric kernel; no new public equality path is forged.

The actual thirteen-input suite retains eleven complete owners and the two
existing238B0 coverage refusals.55 original selection commands are followed by
eleven operation7 commands,66 total. The69 guards include premature/cancelled
handoff, foreign transaction, duplicate append, closed-source and prepared
allocation failure. Initial IDs0/1 and Next2 remain distinct from chosen tag1.
The independent numerical oracle first validates the full source→WORD/TEXT→
coverage→selection→indices→cleanup→transforms chain, then reconstructs every
operation4/7 link, actual range, counter and assigned56-byte body. Managed and
NativeAOT output matches exactly. No expected transcription is an input.

The static audit composes six pinned regions, three bindings and33 instruction
pins for the native counter append. It reads vendor bytes only. Old runtime
files and APIs remain unchanged. Selected/original registry replay, C20.VF8,
final2E7/BE2 transport, DOM adoption and full original acceptance are separate.

Build `tools/LegacyGlobalCounterHandoff.Probe` and run with
`MODELS RECORDED-MANIFEST CAPTURE`. Its independent auditor accepts
`MODELS CHAR PARENT COVERAGE SELECTED INDEX CLEANUP TRANSFORMS COUNTER [NATIVE]`.
Static reproduction uses
`python tools/handwriting/audit_global_counter_handoff.py REPORT --native-repo REPO`.
