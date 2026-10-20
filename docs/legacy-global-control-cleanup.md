# Genuine global control cleanup

`LegacyFreshMainGlobalControlCleanup.Prepare(actualIndices).Run()` joins the
actual global020920 index owner to the bounded no-control023260 branch. It
does not create the older configured-two-choice owner or change existing APIs.

The preceding global publication owns exactly one ALT/WORD48. Cleanup reads
that complete actual row's flags at byte8 before clearing100000 in its own WORD
aggregate overlay. The row flags and aggregate flags remain distinct. It then
visits every CHAR node in the actual insertion list, including unindexed nodes;
AVL in-order and the list of indexed sources are not substitutions. Every
visited payload must retain the same original interner identity. Historical
WORD48/CHAR40 bytes, strings, indices and pending source commands stay unchanged.

The static audit composes the pinned five-region native proof: WORD row scan,
WORD aggregate clear, CHAR insertion walk, active control predicates and the
following037E80/026EF0 boundaries. Active WORD100000 refuses before unavailable
022960. Any active CHAR100000 branch is conservatively excluded, even if a
particular native reverse-record scan might find no control record and return
without deletion. Native control deletion and034D90 index updates are not
simulated. The admitted no-control path does not need a guessed string filter.

A weak-table lease belongs to the exact completed global index owner. Its own
upstream publication lease prevents recreation to bypass single use. Pre-entry
cancellation leaves the prepared cleanup retryable. Entered failure preserves
row counts, the WORD-clear overlay and CHAR visit prefix, and poisons retry.
Completion revalidates the upstream owner after all work and diagnostic hooks:
disposed sources and counter-only failed selection registration cannot produce
a successful cleanup. `ValidateCurrent` requires successful completion and a
still-current upstream index; copied diagnostic prefixes remain historical.

The actual thirteen-input suite retains eleven completed owners and the two
existing238B0 coverage refusals. It checks33 CHAR owners and72 full string/40-byte
records. An independent Python oracle first replays the original source,
WORD/TEXT, coverage, selection and AVL-index captures, then checks the exact
WORD48 flags, aggregate clear, complete insertion order and unchanged rows.
Managed and NativeAOT output must match byte for byte.

Separately prepared tests cover five cancellation prefixes, late source disposal
and counter-only invalidation,96 flag-predicate controls and an exclusive claim
race. These controls are not natural active native control-service invocations.
There are178 guards. No expected transcription enters the production path or
numerical oracle. This stage does not improve the measured Russian accuracy,
apply a document patch, or implement037E80,026EF0 and final acceptance.

Build the probe `tools/LegacyGlobalControlCleanup.Probe` and run it with
`MODELS RECORDED-MANIFEST CAPTURE`. Run its independent auditor with
`MODELS CHAR PARENT COVERAGE SELECTED INDEX CLEANUP [NATIVE]`. Static reproduction
uses `tools/handwriting/audit_global_control_cleanup.py REPORT --native-repo REPO`;
vendor binaries are read only, never executed.
