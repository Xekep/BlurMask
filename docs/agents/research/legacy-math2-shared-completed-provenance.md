# Sharing immutable completed-event provenance

Completed EOS and fallback leases now retain the exact immutable
`Step.Generation` and a validated parent index instead of retaining a fresh
primary row, secondary row and deep ancestry matrix for every event. This is
a managed memory adaptation to out-of-band source provenance. It changes no
native168 fields, numerical algorithm, record content or attention policy.

The lease already retains its Step, which already retains that generation.
Consequently this sharing does not introduce a newly retained generation.
The actual completed Record still owns its separate primary/secondary lists,
Library3E and source lease as required by037430. Those record fields are not
deduplicated or aliased with generation arrays.

## Immutable ownership and active-Step guards

`Math2Mode0PrefixGeneration` owns private primary, secondary and ancestry arrays
and has a private constructor. `Cold` allocates its initial rows. `Append`
allocates new rows for every rank and returns a new generation; it never writes
the old arrays. `Copy` constructs a detached copy. All available array readers
return copies, including the new selected-parent readers. No live array or
extractable ReadOnlyMemory view is exposed.

Both canonical event-data producers carry the actual generation object:
ordinary EOS registration and last-position fallback capture. The common
private lease constructor invokes `BindCompletedGeneration`. That method runs
under the active Step gate, requires `ReferenceEquals` with that Step's retained
generation, then validates the active parent and cold prefix lengths. A foreign
same-shape generation, equal detached `CopyGeneration`, wrong parent or stale
Step cannot substitute for this owner.

After capture, the shared generation remains immutable and usable for detached
inspection even after selected-parent application or tensor-context disposal.
It confers no active-Step capability: attention capture, candidate consumption
and tensor application retain their existing owner/generation gates.

`CopyPrimary` and `CopySecondary` copy only the selected old parent row.
`CopyAncestry` deep-copies the complete retained rank/time matrix only when a
caller asks to inspect it. Thus repeated inspection can allocate, as before;
ordinary event capture no longer deep-copies the matrix. The root attention
adapter obtains the prefix length through the immutable selected-parent
metadata rather than a removed duplicated array. It still captures no tensor
unless explicitly requested through its existing API.

## Actual retained-data measurements

The official Microsoft CPU ONNX Runtime1.23.2 evaluates the same two-stroke
geometric input under the known cold width1 policy and explicitly prepared
width3/16 variants. The probe records67 real EOS events. Fifteen events at
width3 share five generations;51 events at width16 share six; the one width1
event has one generation. Within those totals,65 events share10 generations
that each contain multiple EOS events.

For one lease the old extra retained **data-element** footprint was

`4*ancestryIntCount + 2*selectedPrimaryLength + 2*selectedSecondaryLength`.

The new generation reference uses the already-retained Step generation and
does not retain those duplicated arrays. On these short actual runs the
avoided repeated element bytes are20,1140 and16320, totaling17480. These are
not CLR object headers, GC heap sizes or allocation benchmarks. Temporary
inspection arrays and required completed-record list storage are excluded.

The independent auditor verifies every integer/label, source raw48, parent,
generation offset/width, ancestry hash and the byte-count arithmetic. A
same-platform optional baseline compares every event against the previous
initial-order capture before this optimization. The probe also uses actual
reference equality to verify sharing, mutates returned inner/outer arrays,
advances selected parents and repeats detached-read mutation after disposal.
There are72 wrong-parent, foreign/copied-generation and stale-Step guards.

The existing fallback behavior remains covered by the recording-loop
regressions, including canonical-source rejection and prepared IEEE/eligibility
controls. The same common constructor and generation binder are used by both
source types. No positive owned fallback event has been observed on the
current actual geometric catalogue; this optimization does not manufacture
one or relabel an EOS event as fallback.

## Attention budget: measurement and design only

The production loop does not begin storing per-EOS attention matrices. For
measurement, the new probe reads the current output1 shape once per Step and
retains no tensor values. Its actual shape is `[segments,beams,time]`; the
potential completed matrix is `[oldPrefixLength,segments]`.

For the67 observed events, separate F32 matrices would contain13208 data bytes.
Saving one last-frame raw slice per Step across the same three trajectories
would contain6396 data bytes before any tensor/array overhead. These are
different layouts and lifecycles, not an implementation-equivalence claim.
Neither is automatically stored by this change.

The scaling difference matters beyond these short examples. With width W,
segments S and maximum M, storing up to W full old-prefix matrices per position
can require `4*S*W*M*(M-1)/2` data bytes; one last-frame history per Step scales
as `4*S*W*M`. At the supported prepared upper domains W16/S6000/M1000 these expressions
are191808000000 and384000000 bytes. Neither can be treated as a free default.

A future owning join must compare the actual native per-time history lifecycle
with the carried-attention shortcut, preserve source ancestry and observable
masking, and reserve an explicit total byte budget before allocation. A budget
failure must keep its native/managed failure scope clear and poison an already
consuming context. The existing per-snapshot1M-value guard is not a total-loop
budget. No numerical default budget or automatic hook is inferred here.

## Verification commands and scope

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet run --project tools/LegacyMathCompletedProvenance.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/math-provenance-build -- model.ikrmath models/math2/math2-unwrapped.res path/to/official/onnxruntime.dll capture.json
python tools/LegacyMathCompletedProvenance.Probe/audit_output.py capture.json models/math2/math2-unwrapped.res
# Optional same-platform old-owner baseline, not a universal ORT hash:
python tools/LegacyMathCompletedProvenance.Probe/audit_output.py capture.json models/math2/math2-unwrapped.res old-initial-sort.json
```

Fresh Managed/NativeAOT and independent old recording, recorder, attention and
initial-order regressions must remain byte-identical on the same platform.
No vendor SDK/MLOrt execution is used. This change adds no complete search,
EF4, output transport, accepted formula, confidence or application behavior.
