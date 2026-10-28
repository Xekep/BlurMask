# Library DOM target operation lifetime

The explicit library target serializes target-bound Build, V50 Export and
Apply with one target-owned reentrant monitor. This is a portability lifetime
rule, not a recovered native SDK concurrency contract or C8F history lock.

Build previously captured a live prior child under the target monitor, then
released that monitor before cloning the child into its writer. A concurrent
Apply could remove and release that exact child. Export likewise validated
the epoch before its clone phase without retaining the target operation
scope. The new scope retains the monitor across the entire target-bound
operation, including captured-node reads and writer copies. Apply already
held this monitor across its full mutation prefix.

The acquisition order is target monitor, then builder-local monitor. Apply
and Export do not acquire a builder monitor. Root snapshots use the target
monitor and return deep immutable snapshots. The shared BEE name pool already
protects its intern dictionary with its own monitor; no operation acquires a
target monitor while holding that name-pool monitor. There is no new general
thread-safety promise for recognition owners, sources or internal diagnostics.
Blocking monitor acquisition does not promise cancellation before acquisition;
the token and epoch are checked once the scope is acquired, before single-use
consumption, arena creation or mutation.

The old detached Prepare route has no target and keeps its existing builder
monitor and output. The new scope does not change sequential native algorithms,
arena traces, failure prefixes, copies, scalar bytes or public signatures.
Export/Apply retain their existing single-use behavior. A stale builder/export
is refused before consuming its invocation. A stale Apply is refused before
poisoning the target or consuming its edit.

The target probe adds two deterministic scheduling controls on actual inferred
CHAR/WORD/TEXT sources. One pauses Build immediately after capturing a real
prior child; the other pauses Export after validation. Each starts a competing
Apply of a real same-target edit. A nonblocking monitor check from that thread
must fail before Apply is allowed to wait: sleeps are not used to infer mutual
exclusion. Both operations finish after the writer releases its scope, and
the resulting stale invocations are refused. A reentrant immutable Root read
is also checked. These controls are outside the JSON capture, whose independent
raw-record oracle and stable bytes remain unchanged. The tests do not infer
native observer/history semantics or an accepted C20 document.
