# Math2 owning ordinary completed-record output

This internal library stage consumes the exact successful
`Math2Mode0RawOnlyFinalOrdering` owner. It restores the ordinary **no-rule-hit**
subset of `046AE0 → EE4.025980/0251E0 → 045C60`, with real Source, Record,
Library3E, alphabet, unit and run ownership. It does not produce an accepted
formula, concatenate labels into a string, publish a layout tree or replace the
application recognizer.

## Supported route and explicit refusal

`FormatOrdinary` validates the complete retained vector before its one-use claim.
It binds the immutable source alphabet already owned by each recorded Source;
the formatter never loads a parallel alphabet or accepts caller grammar flags.
Every record must share that same alphabet, authenticated cold bundle and raw-only
factory lifetime and retain its fresh, not already populated Library. Optional
alternatives are absent and the actual cold detection
channel is absent. This is the library pen-only source contract, not a claim that
all native EEB selections exclude existing glyphs.

All 14 actual EE4 rule tables are bound through their opening, continuation and
closing AL labels. **Any** retained rule token, nonzero unit byte two, zero label,
control/synthetic label or unsupported lane refuses the whole vector before
mutation. It does not drop alternatives or select a more convenient winner.
An empty completed vector also refuses this bounded entry because it has no
record-owned AL lease; it is not converted into an accepted empty formula.
The actual width-sixteen retained vector contains structural tokens and is a
required refusal control. Cold width one and explicit prepared width three are
positive actual-model routes; width three is not claimed as the fresh default.

For the accepted no-rule domain, `0251E0` copies complete 32-bit units and returns
the identity U16 map. Every nonnegative run index is remapped through that map;
negative absent indices remain absent and segment ends remain unchanged.
`046D20..046D7D` then replaces only the low U16 middle-dot label with period when
it is between the actual contiguous digit labels and all three unit byte-two
flags are zero. Eight explicitly prepared controls distinguish every flag
combination and preserve the upper 16 metadata bits. They are not fabricated NN
observations.

The source AL contains 299 labels, but the separately serialized ordinary output
limit EED+1E2 is **297**. The stage uses this limit. It does not turn the two extra
controls or synthetic replacement 299 into ordinary text.

## Same record libraries and semantic rows

The existing actual `Math2Mode0EmptyLibrary` owner gains a lazy interval-interning
capability. It wraps the already restored Engine3E `0315E0/030E60` portable
interner **inside that same record library**. A caller cannot install a pool.
Identical intervals reuse identity within a library; different record libraries
retain different identities even when bytes match. Library Release disposes its
lazy pool exactly once, so retained interned-string readers refuse after source
disposal.

Each actual ordinary unit receives one semantic 40-byte row and one 16-byte
alternative: typed interned identity, UTF8 byte count, raw scalar bits zero,
flags zero and absent detection metadata `FFFFFFFF`. The scalar is not calibrated
confidence. The context/table lane at row +00..0F, native pointer bytes and ABI
padding remain opaque rather than being filled with invented zeros. Unit rows
are returned separately in native record/unit order; no joining spaces or
formula acceptance threshold is inferred.

The associated actual EF4 cell receives the formatted unit and mapped run
payloads. Record identity, Source, Library and score are retained. Copies exposed
for inspection are detached; typed interval identity remains library-owned.

## Claim, budget and failure lifetime

Shape, actual Source/Record/Library/AL validation and the complete ordinary domain
check happen before the output claim. Preflight reads metadata and immutable AL
strings without copying unit, attention or string payloads. The explicit
16 MiB default is a **payload envelope**, not a native allocation cap or an RSS
claim. It conservatively combines the existing history/EF4/final-order
reservation with formatted unit/map storage, unit rows, alternatives, run and
per-record workspace copies, distinct per-library UTF8 bytes plus terminators,
UTF16 hexadecimal interner-key contents, and peak temporary UTF8/key conversion.
Shared payloads are counted once; CLR object headers and dictionary-node overhead
are outside this numerical payload bound.
Named low-budget controls are diagnostic entrypoints, not a native property.

Entry cancellation, invalid budget, insufficient payload envelope, or an
unsupported vector preserve the unclaimed final token for inspection/reuse.
After claim, the final token's former row/permutation getters refuse. A consuming
callback failure or cancellation preserves the units/runs/intern prefix, poisons
the entire loop and prevents all result reads or retries. Detached phase, unit,
run and pool-count traces are logical writes, not zero replacements for native
buffers that have not yet been visited. They are failure inspection only. Disposal
releases every
original Library and its intern pool exactly once; it does not roll back prior
managed stores. This follows the scoped outer no-publication/DisposeAll
adaptation proved in `legacy-math2-completed-output-route.md`.

## Verification and reproduction

The dedicated probe uses official Microsoft CPU ONNX Runtime 1.23.2 and the
fingerprinted three-model bundle. It captures two actual positive vectors with
four completed records/eight separate unit alternatives, actual structured
whole-vector refusal, eight prepared decimal cases, eighteen consuming
fault/cancellation prefixes and 117 ownership/disposal/budget guards. Actual
output reads also pass after forced collection. All eight earlier Math probes
retain byte-identical Managed/Native full captures and sidecars against the
same-platform pre-change baselines, with their independent numerical auditors.

`audit_output.py` independently reads the original raw AL envelope and decoder
count, reconstructs interval bytes, identity maps, decimal changes, run mapping,
per-library identity numbering and failure prefixes. It neither executes a model
nor calls implementation helpers. Same-platform Managed/Native exact comparison
is optional; no universal Windows full-capture hash is required on other OSes.

```powershell
dotnet run --project tools/LegacyMathOrdinaryOutput.Probe -c Release -- `
  <model.ikrmath> models/math2/math2-unwrapped.res <official-onnxruntime> <capture.json>
python tools/LegacyMathOrdinaryOutput.Probe/audit_output.py `
  <capture.json> models/math2/math2-unwrapped.res [same-platform-other.json]
python tools/handwriting/audit_math2_ordinary_output.py <static-report.json>
```

The static auditor depends on the existing vendor-binary location and
`inspect_text_features` research helper, as the complete route auditor does.
It executes no vendor code and adds 26 instruction pins for no-rule map/full-unit
stores, semantic row initialization and SAME-library alternatives to the complete
15-region/83-pin/10-binding/14-rule output proof.

The complete structured EE4 grammar engine, existing-string lane, optional
auxiliary alternatives, caller first-record deep copy, external result vector
transport, source/context/layout tree and application acceptance remain outside
this stage. A later `043AC0` copy failure is not claimed transactional.
