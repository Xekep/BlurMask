# Math2 full cold EE4 output over actual completed owners

`Math2Mode0RawOnlyFinalOrdering.FormatFullCold` consumes the same successful
raw-only completed vector used by the ordinary subset. It processes **every**
retained alternative through all 14 bound EE4 syntax rules and the original
mode-zero/null-eligibility parser, repair, table padding, compaction and grammar
check. It then remaps the record's actual runs, applies the decimal rule and
builds separate interval alternatives through that record's existing Library3E.
It does not join those intervals into a formula string or assert acceptance.

The new named factory and `FormatOrdinary` share one private final-token claim.
The original ordinary factory keeps its complete structural refusal, old API
and capture. A caller cannot run either facade again after consuming success or
failure. No fabricated ordinary winner, alternative filtering, old terminal
coercion, new model evaluation or new string-pool substitution is involved.

## Native structural semantics

The static contract is in `legacy-math2-ee4-formatter.md`: same source AL, 14-rule
table, successful AST-node initialization, 64-byte frames, exact signed steps,
ancestor search, fence delimiters, table counters, recovery and final check.
The portable representation uses managed objects and lists for those owners;
it does not claim their ABI pointer bytes or native allocator order.

Important retained distinctions include:

- Presence is the native AST-kind 2/3 test, separate from EF3 observability.
- A successful rule AST node initially has unit `0000FFFF`; opening a nested
  rule as a fence's first body therefore follows the native invalid-fence path.
- Wrong closers can cause backward ancestor detection and recovery of the top
  frame. Missing-content diagnostics 1/4 discard the current unit as full
  `0000FFFF`; marking recorded rule positions writes only low U16 `FFFF`.
- Recovery may append a missing closer. Table padding inserts complete 32-bit
  separator units and updates the native input-position map in literal order.
- Compaction can replace removed fraction/slanted/vertical bars by the same AL's
  minus or slash while retaining upper original metadata. Its U16 map is not
  reconstructed from final labels or logical table coordinates.
- The final grammar check uses a fresh parser after scratch/AST cleanup. A
  repaired vector that still fails this check cannot become output.

The owning cold branch remains the explicit **library raw-only** source with
no properties/existing expression and no external eligibility map. It does not
infer a general native EEB selected-item policy from tensor shape. Existing
source byte-two units, auxiliary alternatives, other formatter modes and their
owners remain outside this factory. The pure syntax controls may include the
two AL control-rule labels 297/298; those are not actual 297-output decoder
selections and are not manufactured as owning NN records.

## Ownership, budget and failure

Actual Source/Record/Library/AL/score identities stay attached to every record.
The full path builds local formatted units first, maps the actual runs, performs
the decimal rewrite and publishes the formatted unit payload before interval
output. It retains the final vector order and raw score bits. The return type
and `CopyRecords` shape match the ordinary output owner, allowing a later
separate caller-record copy without pretending that a Source-owned string is
already detached.

Before claim, a combined 16 MiB payload envelope covers existing history/EF4/
ordering reservations and the new formatter vectors, frames/positions, worst
table padding, row/alternative payload and UTF8 plus UTF16 interner-key contents.
The formatter's input extent remains at most 1000. Its explicit portable output
map bound is 65535 units; this is a library safeguard, not a native resource or
allocator cap. Table growth checks the bound before insertion. Metadata and
immutable AL byte counts are examined before payload allocation; CLR object and
dictionary-node overhead are not an RSS claim.

Entry cancellation, bad budget and unsupported owner shapes do not consume the
final token. After claim, failure/cancellation poisons the same source; no partial
result or retry is exposed. Local formatter construction is a managed allocation
adaptation, not a capture of every native mid-allocation prefix. Logical actual
record unit/run/interval prefixes are retained for inspection. Every actual
Library remains owned until exact-once DisposeAll, and retained string readers
refuse after disposal. The scoped failure path publishes no vector, as proved
at the outer `047C50` caller. No rollback or native failure-order equivalence is
claimed.

## Actual and independent evidence

The dedicated test uses the official CPU ONNX Runtime 1.23.2 and the exact
three-model bundle. Cold width one and explicit prepared width three/sixteen
produce 20 actual retained records. Five contain actual subscript/fence syntax;
the whole vector now completes with 50 separate alternatives and 100 UTF8
interval bytes. These are post-inference labels, never input/expected-text
feedback. Wider prepared search is not claimed as the cold width-one default.

The independent Python interpreter uses literal native 64-byte frame lanes,
separate rule/position vectors and original source AL bytes. It does not call
C# normalization helpers. All 1614 prepared syntax controls cover every rule,
nested/missing/wrong steps, malformed fences, ragged tables, metadata residues
and mixed tokens: 1343 change their input, 144 extend it, and 18 remain invalid
after normalization. Hand-written discriminators separately check missing sub
closers, invalid fences, default-FFFF nested openers and removed bar substitution.

Eighteen consuming fault/cancel controls stop on a genuine structural record
after an already completed actual prefix, or at whole-vector entry/completion.
They verify full unit/run/alternative prefixes, exact-once cleanup and refusal of
all later output. Ownership, foreign/replay/stale/cancellation, exact/low budget,
fresh Library and detached-array controls total 176 guards. Both claim directions
(ordinary then full, full then ordinary) are checked. Actual raw NN frame counts
and hashes remain unchanged across formatting. Forced collection
before actual reads also preserves the result. The shared ordinary output must
retain its old exact same-platform capture, alongside prior Math regressions.

```powershell
dotnet run --project tools/LegacyMathStructuredOutput.Probe -c Release -- `
  <model.ikrmath> models/math2/math2-unwrapped.res <official-onnxruntime> <capture.json>
python tools/LegacyMathStructuredOutput.Probe/audit_output.py `
  <capture.json> models/math2/math2-unwrapped.res [same-platform-other.json]
python tools/handwriting/audit_math2_owned_ee4_output.py <static-report.json>
```

The static auditor extends the complete parser/output proofs with literal
presence, diagnostic, tombstone, replacement and map-store pins and checks the
owning call order. No vendor executable is loaded. External caller-record deep
copy, complete result vector/tree/layout transport and application recognition
acceptance are distinct later joins.
