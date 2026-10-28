# Owned cold Math2 recording loop

`Math2Mode0ColdRecordingLoop` owns the authenticated cold mode0 tensor/prefix
context and its completed-vector owner. Actual official-ORT logits feed the
existing portable F32 cost adapter, raw48 heap/sorter/pruning/EOS handoff,
ordinary record producer, last-position fallback check and selected-parent
tensor/prefix application. This stage stops **before039A96 completed ordering**.
It is not a complete decoder or an application recognizer replacement.

## Actual policy and position producer

`PrepareColdDefault` accepts the existing `Math2Mode0ColdBundle`, whose raw
resource and three exported graphs are authenticated. Its width1 is the
conditional fresh EED/fresh unconfigured EEB result, not an inference from
resource mode0. The model JSON has no `beam_size`; cold temperature is1, and
the decoder metadata contains `delta_cost_max:-1`, `seg_type:sigmoid` and
`need_force_nonobs_positions:true`. Copied, inherited or property-configured
sources do not acquire this default-policy proof.

`PrepareWithPreparedPolicy` separately labels explicit width, temperature and
delta as prepared controller inputs. Width is bounded1..16, temperature is
positive finite and delta is finite. Both factories use the same actual
encoder/initializer/decoder handles and Step ownership.

The maximum comes from the immutable prefix-context fields: original input
`src.Shape[0]` is N, encoder output4 is runtime `max_pred_counts` K, and the
maximum is `min(1000,4*N+1,K)` when K is nonzero, otherwise
`min(1000,4*N+1)`. It is neither encoder memory length nor stroke count.

Native038CCD..038CE5 increments the position before testing the previous
position against the maximum. The loop therefore retains attempted position
maximum+1 but performs exactly maximum decoder calls. It keeps going after
ordinary EOS records exist, and it does not exit merely because the remaining
raw48 costs are all FMAX. The existing handoff retains at least one active row
in that case. No first-EOS or greedy stopping rule is introduced.

## Ordered EOS and distinct fallback ownership

For each actual Step the loop first records all ordinary EOS events through
`RecordOrdinaryEosPhase`. This producer requires consecutive offsets starting
at0 and the exact candidates/Step/context. It binds that candidate identity
even when there are no EOS events, then issues a private recorder-owned phase
token. Skipping an earlier phase, manually omitting an EOS record, or passing
another candidate result cannot establish a later empty-vector proof.

`PrepareLastPositionFallback` accepts only that actual phase and Step. Its
branch order is: not last position; otherwise completed vector already
nonempty; otherwise scan the current post-EOS, re-sorted raw48 pool. It takes
no caller-supplied empty/last-position flags. A repeated preparation refuses.

The prepared eligibility kernel models0393D3..0394D0 only. It scans the first
width rows of the full2*width pool, tests completed-count capacity through its
owning consumer, reads signed parent+28 and requires **any observable token in
the old primary prefix**. The current non-EOS label cannot make an empty or
structural-only old prefix eligible. It does not require the current label to
be EOS. Its IEEE behavior includes the explicit `0393E4 JP 0393EC` before the
equality skip: NaN enters the parent/prefix checks; only ordered equality with
finite FMAX skips the row. The FMAX skip does not read foreign parent bytes.
Actual candidate generation remains in its existing supported finite-cost
domain; prepared NaN controls do not claim an observed network NaN.

An eligible actual row would issue a distinct
`Math2Mode0LastPositionFallbackLease`. The candidate owner registers the batch
before any lease content can be constructed. Each one-use lease retains the
actual post-EOS row, old primary/secondary/ancestry and same-Step common
secondary frontier. It cannot be fabricated from eligibility ranks or scalar
score facts. Capturing another row requires consumption of the previous one;
selection requires the entire fallback scan to be consumed. Foreign vectors,
unissued phases/batches, stale Steps and replays refuse. Ordinary EOS append
and source getters retain their strict source-type behavior.

The fallback append uses the existing constructor/count/content kernel: the
current non-EOS label is **not** appended to either old prefix. The distinct
Library3E owner, logical capacity/count order and failure prefixes remain as
described in [owned completed records](legacy-math2-owned-completed-records.md).
There is no native pointer/padding or physical allocator-order projection.

## Lifetime and failure scope

The loop is single-use. Entry cancellation before its claim leaves it reusable;
any exception after the claim poisons the context and retains the already
written records, prefix generations, position and observation state. Disposal
owns the context and unique record/library cleanup. It does not roll back a
partly consumed record or pretend that managed allocation is transactional.

A friend-only diagnostic observer exercises cancellation/failure immediately
before recording, after recording, and after selected-state application. It
is not the native optional host progress callback. There is no production
callback or public forced-fallback entry. The source registry is checked before
an unregistered batch can invoke a content-construction callback.

## Verification and observed coverage

Eight fixed geometric ink inputs are processed without expected formula text:
the original two-stroke fixture, a vertical, crossed diagonals, sawtooth,
circle, three/eight parallel verticals and joined zigzags. Together with two
explicit prepared-width variants of the original fixture, the probe executes
10 actual official-ORT runs and72 decoder calls. It records24 actual ordinary
EOS events. All eight input geometries already have a completion at their
last position: **zero actual LastPositionEmpty entries and zero owned positive
fallback appends are observed**. The prepared positive eligibility controls
must not be described as genuine positive inference events.

The independent Python oracle checks all9 decoder inputs/4 outputs, raw48
heap/sort/pruning/EOS stages, F32 scores, sidecars, frontier, old-prefix records,
selected-parent ancestry and6,196,680 exact tensor/cache bytes. It verifies the
maximum+1 counter with no additional NN call. There are1024 prepared IEEE and
prefix eligibility controls, six retained failure/cancellation cases and431
guards, including26 named ownership/history/shape guards. Three failures occur
before or after the actual third-step record; post-apply failures retain the
advanced prefix. Prepared controls preserve the whole input raw48 pool.

Fresh Windows NativeAOT and Managed captures are byte-identical locally:
23,614,240 bytes, SHA-256
`e61c3ecd8e92bc30eedcdccd2a6754c2fb001252382b414200540adabccba639`.
This is same-platform evidence, not a cross-platform ORT/MathF golden. The
auditor uses numerical tolerances for the portable transcendental operations
and no universal capture-SHA assertion. The old recorder/prefix captures remain
`f455ac7a4b99e892cb5a827bb955a28d73e3bdf4ed0350f74c122eecaf0a621d`
and `d350e2ca63956bc524936c40c60664c173130d4c1216c1850ed930c3ca5b3d08`;
the event-attention capture remains
`17ee5e4708ad07efda8e4a4be4caca7bed5a01890b984dfd5d9330868ae02905`.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet run --project tools/LegacyMathRecordingLoop.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/math-recording-build -- model.ikrmath models/math2/math2-unwrapped.res path/to/official/onnxruntime.dll capture.json
python tools/LegacyMathRecordingLoop.Probe/audit_output.py capture.json models/math2/math2-unwrapped.res
python tools/handwriting/audit_math2_recording_loop_join.py static-report.json
```

The static join composes four cold-loop source audits and five recorder
prerequisites. Regions overlap, so their counts are not a unique PE-coverage
total. The outer-driver audit now explicitly pins the NaN JP branch. Static
auditors read the vendor PEs but never execute them; actual numerical tests use
the official Microsoft CPU ONNX Runtime1.23.2 package.

## Remaining real joins

This stage does not sort/truncate/destroy completed168 alternatives, capture
attention into the loop, execute036F20/EF4, transport output units or accept a
formula. The actual decoder exports `attn_seg`, so036F20 is a real future
requirement. The EF4 branch byte is caller-owned
`request+4 -> EED.B1 -> packet41 -> EEE51`; no zero/default branch is inferred
from the three graphs. Root's separate attention snapshot API is preserved but
not implicitly called here. Native CPU exp/log dispatch and optional native
host progress binding also remain outside the adaptation.

Imported prefixes, constraint/diagnostic models, configured aliases and warm
inherited source policies remain explicitly outside this cold scope. No whole
search, native SDK execution, confidence, accuracy or application acceptance
claim follows from these recording-stage tests.
