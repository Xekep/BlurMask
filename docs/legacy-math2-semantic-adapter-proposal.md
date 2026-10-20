# Math structural consumption: native boundary and proposed library adapter

This is a static proposal, not implemented production behavior. The separate
actual corpus proves nontrivial 2D NN/EE4 tokens and a diagnostic existing-library
integral result333. It does not establish a recovered native semantic tree,
calibrated confidence or an application recognizer switch.

## Native typed-node export is a distinct input route

Read-only Ghidra export of `MyScriptMath.dll` identifies these complete bodies.
Their exact contiguous Ghidra extents, hashes, 1,742 instruction pins, 20 literal
strings and seven dispatch-table rows are checked by
`audit_math2_semantic_adapter_proposal.py`. No first-unwind extent is called a
complete function. The seventeen exported bodies include three later layout
owners; those are classified separately below.

`00ADE0` obtains the actual native type through the environment object service,
subtracts138D, bounds-checks0..6 and dispatches through the actual table atAF40:

| Native type | Actual writer | Result |
|---|---|---|
| 138D | 009200 | terminal/symbol candidates and selections |
| 138E | 009CF0 | nonterminal and selected child |
| 138F | 00A040 | rule node |
| 1390 | refusal | low return byte cleared |
| 1391 | 00A2A0 | cell and recursive members |
| 1392 | 00A710 | border and recursive members |
| 1393 | 00AB90 | table and recursive members |

This is not dispatch over token label IDs. `008FC0` constructs a C22 from the
actual typed node's source positions and the passed source owner: type138D uses
service13F0; recognized nonterminal/cell/border types use13F1; other types
refuse. The actual returned selection is used by the writers' CB8 registration.

`009200` uses service13F0, creates `symbolSegment`/`symbolCandidate`, copies
native alternatives and selected state, registers MATH_SYMBOL and emits the
corresponding terminal/tag. `009CF0` obtains the actual native name, source
selection and selected-child index; registers MATH_NODE; creates a
`mathNonTerminalNode` with name/range/selected/negative pending tag; appends it
and recursively calls00ADE0 on the actual selected child. Parent attachment can
precede recursive failure. The rule/cell/border/table writers likewise perform
real recursive construction; a false return does not prove the previously
attached prefix was removed. Cell coordinates, border positions and table row/
column counts come from their native typed objects, not inferred label text.

`00AF60` and `00B450` are owning result/CB8/temp-patch/source-edit publication
callers. They are separate from the previous raw Math2 Document028070 producer.
`0055C0` chooses the mathResult/sequence or terminal/nonterminal representation
and constructs a BCD scoped view; `007C50` dispatches existing DOM node kinds;
`008D80` traverses an existing result. These do not prove that the new raw
Math2 EE4 result has been converted into the native138D..1393 objects.

`0695E0` recursively operates on existing DOM mathRule/nonterminal/table/cell/
border/terminal nodes and floating geometry; `06ADC0` and `06BBC0` are later
existing-field/result/layout consumers. They are exact next analysis entry
points for native layout ownership, not a replacement for a semantic parser.
Their callees/registered owners are not fully closed by this bounded proposal.

The already proved Document028070 path still constructs a fallback `sequence`,
`symbolCandidate.char=?`, and empty `mathTerminalNode.name`. No call-chain or
owning conversion connecting its actual EE4 markup to the native typed-node
export above is claimed here. That missing native bridge remains work.

## Proposed next runtime batch: explicitly named library glue

Implement a NEW owning experimental adapter in `InkRecognition.Math`, using
the actual cold/default width1 path. This deliberately implements a strict
library structural consumer; it does not impersonate138D objects, claim native
tree recovery or mutate the original field. Wider prepared-search policies stay
diagnostic. The selected first record is never replaced using labels, expected
answers or whether a secondary candidate happens to solve.

The public owner can load the existing authenticated pack/raw-resource/official
ORT bundle and own its runtime/bundle. A proposed detached result contains:

- actual immutable raw unit IDs, UTF8 tokens and per-unit raw source ranges;
- first-record score bits, frames and a named cold-default provenance;
- a strict bounded typed syntax tree with token-index spans;
- canonical LaTeX when the supported grammar parses successfully;
- `Confidence = null` and `Admission = Uncalibrated`;
- a calculation-request flag derived from an empty trailing equals only.

The exact public names are implementation choices; the admission/ownership
semantics are requirements. No old `MathRecognitionResult` with its required
float confidence is fabricated. Raw native ranking scores are not probabilities;
neither score normalization nor the old greedy confidence can be reused as the
app's0.85 admission value. No automatic app acceptance is proposed in this batch.

The owner receives coordinates only. It creates its own current view, original
Math field, cold source, actual formatted output, source attachment and produced
selected-field receipt. It validates that exact receipt outside old field gates,
reads only its actual first-copy record and deep-copies its data before releasing
the borrowed handles. This class lives in the same Math assembly and can use
existing internal APIs: **zero old runtime seams are required**. No new source
mutation accessor, receipt factory, supplied field snapshot or caller AL map is
needed. The existing diagnostic friend assembly can host the probe unchanged.

The detached syntax representation retains actual full unit IDs and source
ranges; structural units are not filtered. It preserves letter case and uses
token-index spans without inventing C51 IDs, DOM tags or native source history.
A strict grammar can support the current formatter's fractions, roots,
superscripts/subscripts, under/over limits, fences and tables, with explicit
unsupported results for other constructions. Malformed/unbalanced syntax must
preserve raw evidence and refuse formatted acceptance; it must not turn into a
partial ordinary expression. An independent AST/LaTeX oracle must check every
node, edge, token span, input token and source range, plus malformed/deep inputs.

## Ownership, budgets, cancellation and solver boundary

Use one in-flight request per adapter owner, with cancelable acquisition and
well-defined disposal. The owned runtime/bundle cannot be disposed while a
request uses tensors. Request cleanup disposes receipt, original field and
source/tensor state before its ORT run; disposal is once, never source inverse.
Returned strings/tree/ranges are detached and remain readable after cleanup.
No output retains a decoder, tensor, Step or source-bound output.

Validate finite XY and bounded stroke/point counts before source construction.
Existing NN/source stages retain their own explicit limits. Separately preflight
the complete result/AST byte/node/depth envelope before copying UTF8/ranges or
allocating syntax nodes; a result budget must not be advertised as a bound on
all ONNX/native-runtime memory. Check cancellation before inference and each
copy/parse/mutation boundary. Overflow, unsupported syntax, budget failure or
cancellation yield no accepted partial formula and release all owned resources.
No vendor SDK code is executed. Fresh Managed/Native probes need actual corpus
parity, every adapter allocation-stage failure, cancellation/close/concurrency
controls and retained-result/GC negative controls.

Use existing `MathLatexFormatter` and built-in exact polynomial/numeric solver
only through a clearly named library layer after strict syntax validation.
The source recognition result alone performs no calculation. An explicit
calculation helper, or a later accepted app route, may consume an empty final
equals and call existing local solving. Without equals no result is appended;
with a written RHS no answer is overwritten. Unsupported mathematics remains
unchanged. Existing optional Giac can handle accepted advanced syntax through
its existing bounded process owner; no external service is required by this
proposal. Native solver equivalence is not claimed.

## Deferred application seams

Before an optional app switch, explicitly review `AiMathRecognitionModels`
(currently loads only `StrokeMathRecognizer`), `AiMathLayout` and
`AiAutomaticLayout` (currently require calibrated float confidence), settings
and output types. This proposal changes none of them. A later experimental
mode must communicate uncalibrated admission honestly and retain ink when a
candidate is unsupported/uncertain; changing the whole automatic pipeline's
threshold to admit this adapter would need a separate validated decision.
