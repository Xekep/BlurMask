# FIRST276 partial numerical checkpoint

The independent library now includes an explicit internal numerical route from
original strokes through feature initialization, the first hidden layer and the
post-exponential output scores. It retains the same authenticated model and
exclusive input storage. The default input-only route remains separate.

This is **not a complete mathematical recognizer**. Normalization, class mapping,
shortlists, candidate publication and recognition quality remain unfinished.
The numerical route stops with an explicit partial-boundary exception and never
marks the evaluator or candidate cache successful.

## Verification on 2026-10-25

An isolated Release build contained 928 authored library sources and one probe
source; raw Portable PDB checksums and PE CodeView identifiers matched every
source. Static IL inspection confirmed 48- and 352-entry switch tables without
array allocations or static-field access in the lookup methods.

One original 14-case run under .NET 10.0.12, two logical processors and affinity
mask 3 completed in approximately 0.95 seconds. Twelve eligible cases matched
all 201 known hidden values, 257 post-exponential scores and C0 exactly against
an independent calculation using the resource's actual weights. Expected inputs
were independently derived from prior sampler/segment recordings rather than
the new C# output. The two original directional point-domain refusals remained.
The twelve cases exercised eleven distinct hidden and output vectors; C0 was
zero in every actual case, so positive-offset behavior has only synthetic
coverage. INT_MIN hidden activation remains an explicit unsupported domain.

The old Storage14 result remained byte-exact across 14 cases and 93 guards;
its auditor rejected 299 corruptions. The default input-only route separately
passed its original 12-vector/two-refusal gate. The new portable numerical
regression rejects 37 corruptions, including all-refusal captures, and is wired
into the Windows/Linux/macOS source workflow. These local checks are managed
checks, not NativeAOT or native-engine parity claims.

Only input lanes 0–275, hidden lanes 0–200 and output lanes 1–257 are observed.
Zero model weights prove that padding products may be omitted; unknown storage
padding and output slot zero are never read, exported or declared initialized.
Logical source/copy reservations precede numerical work and diagnostic copies;
they are not measurements of allocator overhead, CPU time or resident memory.

The original model/AL source pins and historical data remain unchanged. An exact
additional source restoration step authenticates the private partial model seam
and requires the numerical source and connected route to appear together.

Run the portable regression with:

```sh
python tools/ci/check-math-first276-numerical14.py .artifacts/math-first276-numerical14
```
