# Shared directional preparation for selected3F/4F/5F

`source.PrepareUnguidedSingleStrokeDirectionalBundle(token)` issues three typed
feature objects from one call-local preparation. Resampling100, actual BC280
landmarks, intersections and17 global statistics each execute once. The existing
temporal16, quantile4x4 and upper5x5 composers then use those same private arrays.
This removes two repeated executions of each common pass when a controller needs
all three families. It does not imply a threefold overall speedup: the family
windows, calibration and networks still have separate work.

The public single-family methods also use the extracted common preparation;
their fixed calibration, output constructors, ownership checks and domain remain
unchanged. The shared frame type and all three composition entry points are
private. There is no caller-prepared sample ingress, global cache, lazy mutable
cache, profile clone, resource rehash or network execution in this operation.
The frame never escapes or becomes retained by the returned bundle. A failure
returns no bundle; cancellation is checked throughout existing passes, at each
core entry and before final publication. A caller can start a fresh independent
feature preparation after failure; this is immutable input computation, not a
claim on a live recognition transaction.

The bundle retains only its source/private issuer and the three existing typed
feature results, whose arrays total `(279+279+223)*2=1562` bytes. Each result
retains the existing source/profile identity, and `OwnsDirectionalFeatureBundle`
checks all three as well as bundle identity. Equal-data foreign sources/profiles
do not acquire ownership. Outputs remain defensive copies. No candidate,
classifier, unified259-label or grammar authority is added.

Admission remains one nondegenerate whole physical stroke,2..4096 original
points and1..255 prepared points, with all inherited numerical guards. Per-family
window memberships remain separate. Conservative aggregate logical array
allocation can still be overestimated by summing the three documented individual
envelopes:136461+144792+154380=435633 bytes at the admitted maxima, under their
stated current-runtime List-growth assumptions. This deliberately counts common
work several times and excludes CLR headers, object/reference metadata and
exceptions. It is neither peak RSS nor a portable allocator/latency guarantee.

## Verification

The managed probe checks seven original geometries, exact bundle-versus-individual
outputs, source identity, defensive copies, foreign/null refusal, cancellation,
domain refusal and immutable profile behavior after clearing caller resource
bytes. It inspects compiled call edges to verify one frame creation, one call per
family core and one call to each common expensive pass; the private cores cannot
invoke those passes again. Reflection reads IL only, without constructing source
owners or invoking private arithmetic.

An independent Python audit derives all5467 feature values from original XY and
authenticated model calibration using the existing three literal references.
All22 guards and11 corrupted captures passed. Root evidence:
`root-directional-bundle-prepared-20261024`, capture SHA256
`7faa10c0a275ad7eddd60c7036b1e30f09ca543852db02fbd34a33e1e63755bf`.
The probe/library build had zero warnings/errors. This verification executes no
network or vendor code and makes no handwriting-quality claim.

The three existing per-family probes were also rebuilt and rerun after the
refactor. Their entire captures remained byte-identical to the prior verified
captures (`ce484a25...`, `7d0f86cc...`, `3b416c2f...`), and all existing numerical,
ownership and corruption audits passed. Evidence:
`root-directional-bundle-regressions-20261024`. The network kernels and weights
were unchanged; no repeated actual network execution was needed for this change.

```
dotnet run --project tools/LegacyMathAkDirectionalBundle.Probe -c Release -- REPO FRESH_CAPTURE
python -I -B tools/LegacyMathAkDirectionalBundle.Probe/audit_output.py REPO CAPTURE FRESH_REPORT
```

This is preparation for the full source-owned classifier; its unified alphabet,
shortlist, grammar matching and tree production are still incomplete. Existing
standalone views remain supported. The change follows RC12.
