# Prepared stroke-boundary filtering and unused-unit pruning

`LegacyPreparedMathUnusedUnitPruning` restores the prepared041660 structural
stage with the actual fingerprinted Math2 EF3 observable-token gate. It accepts
four-byte output units, complete assignments in original coordinates and ordered
exclusive stroke-end positions. It preserves high payload bytes and returns
detached use counts, suppressed positions, deletion mask, old-to-new map, units
and assignments.

Only the first segment of a new assigned-unit run can be suppressed: it must
end a stroke without also beginning a stroke. The predecessor remains the
original unit even when that segment is suppressed. Unused units are deleted
when byte2 is nonzero or their low U16 symbol is observable. Unused unflagged
nonobservable structural units survive. Assignments are remapped after removal;
suppressed assignments stay−1. The native absent-gate branch is not implemented
by this non-null-gate adapter.

The bounded entry accepts1..1000 units and1..6000 fully assigned segments.
Malformed assignments, invalid/duplicate/unordered stroke ends and cancellation
are refused. Native allocator failure prefixes and in-place completed-record
mutation are not represented; managed preparation returns an immutable result
or throws. No auxiliary98/A0 data or invented native padding is written.

Verification on2026-10-18:

- 4096 prepared controls and13 refusal guards.
- Independent run/stroke-boundary oracle:6921 suppressed segments,15,511 removed
  units,1427 unused unflagged nonobservables retained and5615 flagged
  nonobservables removed. All byte2 values0/1/2/255 are covered, including
  single-segment strokes and repeated original units after suppression.
- Caller arrays and every returned inspection array can be mutated without
  changing the retained result.
- Fresh Managed/Windows NativeAOT capture equality, SHA-256
  `12015c21b1ad7e8a0b6421c08f2e6dbfb170b84726174818b8a65f42b028f3ba`.
- Static full041660 region hash/32 pins, composed with the existing raw-resource
  EF3 proof (seven regions/65 pins/seven bindings). Vendor code is read as data.

Probe arguments: `rawMath2.res capture.json`; auditor arguments are identical.
The static audit requires local preserved research binaries. This helper still
needs the earlier correction stages and provenance-bound output owner before
it can participate in an actual EF4 transaction or accepted formula.
