# Initial ordering of actual completed Math2 owners

`Math2Mode0ColdRecordingLoop.OrderInitialCompleted` joins the first native
completed-vector sort at039A96..039AB3 to the actual owned records. It returns
`Math2Mode0InitiallyOrderedCompleted`, explicitly **not postprocessed, not a
final order and not truncated**. The existing recording entry points keep
their behavior unless the new endpoint is called.

## Exact native boundary

Document039A96 reads the actual vector count+8. Count0/1 bypasses036810;
otherwise039AA6 computes `(count-1)*168` and039AB3 calls036810 over the full
inclusive range. This initial block neither filters by beam width nor releases
an owner. It is followed by success-dependent038170,038340 and036F20 work.

At039BF2 the vector is sorted again, including native failure paths. Only after
that later sort does039C15 compare count with width and039C26 call035000 to
destroy the tail. Therefore initial ordering cannot claim final ranking or
perform truncation. The current API accepts only a successfully finished cold
recording loop; native failure-path outer orchestration is not implemented.

The actual decoder exposes attention, making036F20/EF4 a real prerequisite to
final output. Its branch policy is separately caller-owned. This endpoint
neither calls nor skips it under a claimed default. Formula transport and
acceptance remain separate work.

## Moving actual owners through the existing numeric kernel

The source remains the same private `Math2Mode0CompletedRecords` vector from
the same cold loop. Each private Record retains its actual one-use source
lease, arrays, parent/frontier, Library3E and empty nested lanes. The new code
does not reconstruct a record from text, score facts or copied source arrays.

The established `LegacyPreparedMathCandidateOrdering.Sort` kernel is reused.
An internal168-byte **portable identity projection** stores the actual F32
score at0 and a unique original ordinal at8, with the other projection bytes
zero. These bytes are not ABI pointer fields or native constructor padding.
No additional sort algorithm or generic .NET sort is substituted.

The kernel preserves the native short-range insertion behavior for at most20
records and midpoint partitioning for larger ranges. Equal/NaN comparisons
retain their existing native partial-order behavior; no stable or total order
is imposed. In particular equal21-record and42-record partitions can move
identities unlike the stable20-record insertion case.

The complete original Record-reference array is retained throughout the
operation. The returned projection must be a full unique permutation whose
every row exactly matches its original score/ordinal projection. This check
finishes **before any actual owner mutation**. Only then does the code apply
the validated permutation to the original Record references, keeping each
Source and Library attached.

Managed commits use swaps so the vector always contains every original owner
exactly once between observable boundaries. This is a memory-safety adaptation,
not a claim about native168 intermediate stores or native allocator/callback
failure order. The full original-reference array stays retained even during an
injected failure. No Library is released or replaced by initial ordering.

The result checks the exact successful loop/result identity before returning
record snapshots or source associations. Copied permutations and prefix arrays
are isolated from mutation. Source getters return actual retained leases;
they are not fake old terminal or numerical-only owners. Initial ordering also
seals the vector against further recording mutation. The loop owns disposal,
which releases each original Library3E and its pool once in either the success
or failure case.

## Cancellation and retained prefixes

Ordering is one-use, including failures. Entry cancellation occurs before its
claim and leaves the valid finished recording reusable. Missing/incomplete
recording, foreign/unregistered result objects, replays and disposed owners
refuse.

After the claim, an exception poisons the loop, recorder and prefix context.
Before owner moves, the original owner order remains unchanged. An injected
failure after a swap keeps that committed permutation prefix. A failure just
before returning keeps the completely sorted vector but produces no successful
result. None of these failures truncates or releases an alternative; later
context disposal still releases every original owner once. There is no rollback
or native intermediate-failure-order claim.

The diagnostic observer is friend-only and observes logical numeric/move
boundaries. It is not a native036810 callback. Production initial ordering has
no callback or caller-provided permutation.

## Verification

The official Microsoft CPU ONNX Runtime1.23.2 evaluates the original fixed
two-stroke geometric fixture. The known cold policy yields one actual EOS
record. Two explicitly prepared widths give15 actual records at width3 and51
at width16. Thus67 genuine records exercise the owning join, including the
larger-than20 partition route and41 actual owner swaps in the width16 case.
These width3/16 policies are not presented as fresh native defaults.

The width3 source was already numerically ordered and needed no owner moves;
failure tests therefore use the genuine51-record vector. Ten prepared
throw/cancel boundaries cover before/after numerical ordering, before/after
the first owner swap and before completion. The original Source/Library
references remain associated, and all child release counts are one only after
disposal. Count15 remains greater than width3 and count51 greater than width16,
proving that no premature truncation was introduced.

The independent Python auditor replays the literal036810 comparisons and
partitioning; verifies the actual record score from the retained raw48 cost
and old prefix, source planes/frontier/library fields; checks the exact
permutation and each committed owner-swap prefix; and verifies512 prepared
full168 byte controls totaling3,774,120 bytes. Those controls include0/1,
20/21/42 ties, signed zeros, infinities and NaN payloads. There are57 lifecycle
and ownership guards. No expected transcription is read.

Fresh Managed and NativeAOT are compared byte-for-byte on the same platform.
The auditor has no universal ORT/MathF capture hash assertion. The old recording,
ordinary recorder and event-attention probes are rerun independently as
regressions; old source APIs are not widened into this new phase.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet run --project tools/LegacyMathInitialCompletedOrdering.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/math-initial-sort-build -- model.ikrmath models/math2/math2-unwrapped.res path/to/official/onnxruntime.dll capture.json
python tools/LegacyMathInitialCompletedOrdering.Probe/audit_output.py capture.json models/math2/math2-unwrapped.res
python tools/handwriting/audit_math2_initial_completed_ordering.py static-report.json
```

The static auditor pins two exact caller regions and33 instructions plus the
existing complete036810 numerical body and23 comparison/stride pins. All
vendor accesses are static reads. No vendor SDK/MLOrt execution, complete search,
native pointer-image equivalence, accepted formula or application change is
claimed.
