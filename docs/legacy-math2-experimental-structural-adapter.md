# Owning experimental cold Math structural adapter

`ExperimentalColdMathRecognizer` is a new reusable library owner. It loads the
authenticated Math2 pack/resource with the existing official CPU ONNX Runtime
bridge, serializes requests and owns its runtime/model bundle. Recognition uses
the actual cold/default width1 source, original field, full EE4 output, source
attachment and exact produced selected-field receipt. It consumes only the
selected **first** record. There is no alternative search based on expected
labels, syntax success or whether a candidate happens to solve.

The result is detached: exact full unit IDs, copied UTF8 intervals, token text,
raw source range endpoints, score bits, frames, cold provenance and a typed
syntax tree. Tokens, ranges, nodes and child lists have read-only wrappers;
`CopyUtf8` returns defensive bytes. No result retains the decoder, tensors,
source-bound output, field or receipt. A caller can inspect it after all those
owners close. `Confidence` is null and admission is `Uncalibrated`; native
ranking scores are never turned into a probability or the previous greedy
confidence. This API does not fabricate the old `MathRecognitionResult`.

The adapter is new files only. It requires no new internal factory/accessor,
project reference or old runtime seam. The existing trusted first-copy metadata
view permits a complete result budget check before cloning `CopyRecord`, UTF8,
ranges or AST payload. The source receipt is validated outside old field/copy
gates before and after that view. Its exact binding and private request-owned
field establish provenance; a same-ink foreign receipt is refused.

## Strict syntax and bounds

The tree records kind, token span and ordered child IDs. It supports explicit
fractions, roots, superscripts/subscripts, combined scripts, under/over limits,
fences and tables in the existing formatter's token language. Every input token
remains in the result; structural markers are represented by group spans rather
than discarded as ordinary characters. It rejects empty operands, scripts
without a base, mismatched or missing explicit fences, misplaced table
separators, unknown/unbalanced markers, invalid C-string UTF8 and nesting beyond
32. Unsupported syntax preserves raw token/UTF8/range evidence, returns no
LaTeX and exposes no partial syntax tree as an accepted formula. Supported
syntax is formatted with the existing `MathLatexFormatter`.

The source API admits one to256 strokes and at most4096 finite original points.
Those limits are checked before the original input copy; the immutable document
factory independently validates while cloning. Existing numerical/source stages
retain their own bounds. The additional whole detached envelope is at most
32MiB,4096 tokens,1MiB UTF8 and65536 ranges; its conservative allocation plan
includes the existing first-copy reservation plus all token/range/AST copies,
at most `2*tokens+1` syntax nodes and per-token work space. A result budget is
not a bound on all ONNX Runtime process memory.

Cancellation is checked during acquisition, before inference and at copy/parse
boundaries. Requests and disposal share one semaphore. A concurrent Dispose
waits until a live request releases tensors; request cleanup releases receipt,
source state and field before its ORT run. Repeated Dispose is safe. A canceled
waiter never enters inference, and failed requests release their ownership so a
later request can run. Callers must dispose this public model/runtime owner.

## Calculation is explicit

Recognize never invokes a solver. `ExperimentalMathCalculation.Calculate` is a
separate explicit helper and accepts only supported LaTeX ending in an empty
equals. It refuses a written RHS or another equals in the remaining expression.
Numeric arithmetic uses the existing bounded evaluator; an optional delegate
can invoke existing local `PolynomialIntegralSolver` or optional Giac. This
keeps the math recognition project independent of a solver project reference.
Unsupported mathematics returns no result; no external service or SDK evaluator
is implied. The helper does not grant automatic app admission.

## Verification and current limits

The separate probe reuses the existing diagnostic friend assembly name. It
checks31 public cold-owner outputs against93 actual first-record detachments
(cold1 and separately named diagnostic prepared3/16) from the pinned private
corpus. An independent oracle compares exact selected score/unit/AL/frame
evidence with the separately frozen raw NN capture, reconstructs every typed
node/edge/token span and all543 source ranges from original geometry/C79/run
maps. All924 nodes agree. Prepared controls separately cover malformed but
balanced syntax, unknown markers, missing operands/bases/fences, node/depth
bounds, cancellation and foreign/closed receipts. They are not claimed to be
naturally recognized outputs.

The probe covers44 real request-stage throw/cancel prefixes, exact/minus-one
result budgets,72 guards, serialized concurrent calls and an in-flight Dispose
race. Positive GC proves the result releases the source; an actual retained
source-bound output is the negative control. Returned arrays survive closure
and resist mutation through copies. Existing first-record integral-with-equals
produces333 through the explicit local helper; the same integral without equals
produces no requested result. `1+2=` produces3; `1+2` remains uncomputed.

This is explicitly library glue. It does not recover the missing native
raw-Math2-markup to138D..1393 typed-object bridge, original CBB/revision/observer
acceptance or full native semantic/layout engine. Application files, settings,
old greedy recognizer and automatic confidence thresholds remain unchanged.
An app switch requires a separately reviewed admission policy and integration.

CI explicitly runs the17 generated procedural inputs/51 detachments using the
independent oracle's `--procedural-only` scope. It consumes the separately
produced actual raw corpus from that same CI host. The31-input private check
and licensed human coordinates remain a separate local verification.
