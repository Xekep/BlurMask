# Owned global main geometry, case symbols and acoustic cache

`LegacyFreshMainGlobalGlyphGeometry`, `LegacyFreshMainGlobalCaseSymbols` and
`LegacyFreshMainGlobalAcousticScorer` connect the actual shared, cross-merged,
pruned global graph to the main four-family classifier and final195-entry cache.
They consume the owned descriptor cut source and its original chronology. They
preserve global group IDs, primary/secondary descriptor IDs and actual line IDs.
They do not turn descriptor IDs into document-object IDs or create seed memberships.

The caller loads one pinned `LegacyRussianModelBundle`. Each input source owns
fresh geometry, contexts and symbol preparation; the existing read-only network
weights and language resources are reused. The global case-symbol owner creates
one actual2B7→292 symbol/context binding. Its selected lexical alphabet passes the
native20C9D0 case predicate. The acoustic145-label mask comes from actual232C80
callbacks followed by the ordinary native mask producer, rather than an all-one
mask or an expected-language character filter. No second DIRECT dictionary backend
is created. A global query session/controller is a subsequent integration step.

`1B4E40` resolves primary and optional secondary descriptor ownership through
source10, then retrieves their assigned source40 guides. For a cross group it
uses `baseline=max(primaryBaseline, secondaryBaseline)` and
`height=baseline-min(primaryBaseline-primaryHeight, secondaryBaseline-secondaryHeight)`.
This is distinct from `CA5F0` context weighting, which uses the primary guide's
height while selecting contexts by resolved line ID. Primary and secondary
identities remain available after this union calculation.

The actual main21D/v110 profile has FF/FF coordinate modes and target100. Ranges
follow the native original-key ordering; X starts at the group's actual minimum
X and Y at the normalization baseline. Each multiply/divide is binary32 before
truncation, with the existing upper32767 clamp and explicit portable lower-bound
guard. Boundary signs, dynamic four-family weights, source primitive scope and
final-cache masks/corrections use their existing exact producers. Public normalized
diagnostics receive fresh coordinate copies and cannot alter the retained source.

The probe runs eight actual raw inputs through the owned first-result/source720,
guide, descriptor cut, local allocation, cross merge/prune, compact selectors,
global source120 and227 context producers. It evaluates631 groups with2524 actual
networks, including five cross-source groups. On actual single-descriptor domains,
472 arc comparisons match the existing owning single-line classifier/cache.
Forty-eight guards cover cancellation, resource ownership and invalid group IDs.
Python independently reads raw AK/LK symbol tables and related-form callbacks,
builds the145-label mask from1083 callbacks, recomputes float normalization,
evaluates actual networks/dynamic weights/integer costs and final195 cache values.
Existing feature-producer and chronology regression gates cover their kernels.

These are model-execution and ownership checks. They do not measure OCR accuracy,
prove arbitrary mixed/unknown descriptor handling, or claim completed multiline
recognition, winner selection or application integration. The current actual
inputs exercise open-stroke probabilities; they do not establish a new closed-loop
confidence corpus. Nonordinary descriptors and unsupported upstream source branches
retain their owning producers' explicit refusals.

Probe arguments: AK, engine, separation, case, publication, line30, line50,
capture, LK. Run the independent audit with
`python tools/LegacyMainGlobalAcoustic.Probe/audit_output.py AK LK engine capture`.
Run `python tools/handwriting/audit_main_global_acoustic.py --output report`
for the31 exact normalization pins and whole pinned PE block. Existing
`audit_prepared_fresh_main_acoustic.py` and `audit_fresh_main_line_ingress.py`
check the shared metadata/mode/flag and source-lifecycle bindings.
