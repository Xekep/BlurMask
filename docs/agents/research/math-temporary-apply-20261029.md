# Typed temporary application after the default Math diff

The Math library now connects the actual ready diff to its retained temporary
writer. `Math2Mode0PriorTemporaryApply.Create` takes only the typed diff; it has no
opcode array, copied snapshot, asserted tag or selection input. The candidate and
diff gates must still be active under the existing ancestor chain. The bridge
verifies the exact candidate borrow, claim, live header and next actual kept
action; its private action grant is revoked in `finally`.

This is an internal operational contract, not protection against arbitrary code
inside the Math assembly: existing raw views/nodes/arena kernels remain trusted
internal APIs. Their accessibility was not weakened or claimed as a new public
capability boundary.

Native Engine `0E4B90` completes an empty/exhausted traversal successfully and
stops on an unknown opcode without undoing the previous prefix. `0E2560` frees and
clears the header only after successful traversal; the retained diff/payload pools
still require their own later release. The managed header state represents this
logical lifetime, not native action slab allocation or memory reuse.

Pre-cancel and budget refusal precede the claim. After entry, the claim cannot be
reused, including after failure. Completed-operation and header-consumed facts are
recorded before post-operation cancellation/owner checks. A snapshot carries its
observed action count separately; partial/unobserved mutations are not silently
labeled completed. Readiness follows successful return through all ancestors,
and every owning read revalidates them. No original-document rollback is invented.

## Evidence

- 80 authored Math files are authenticated through the compiled portable PDB;
  all 80 repository source files match those tested staging bytes exactly.
- Six actual model pipelines produce three real tag edits each, all CB8-filtered.
  Empty traversal/header consumption therefore has owning coverage; **nonempty
  kept-action application does not**. The new phase never creates a fake edit.
- 203 pre-frozen controls include eight fault/cancel boundaries, nine closure
  cases, exact budget refusal/success, revoked grants and defensive observations.
  The ActionCompleted boundary is not counted as actual coverage.
- Managed run: 2.411 seconds. Windows Native AOT build: 47.257 seconds; run:
  1.418 seconds. The independent header/ownership, raw-model NN/source-position
  and candidate/range/name audits pass for both. CPU2/affinity3, build600 and
  actual45 watchdogs remain unchanged. Vendor code is not executed.
- Independent static review accepted final source fingerprint `2946f0b8…` after
  correcting an initial action-parameter bridge. Four earlier default Math source
  audits remain applicable; their main80 replay is recorded separately.

Evidence: `E:/BlurMask-build-artifacts-20261014/prior-temporary-apply-root-20261029`.
Permanent [probe](../../../tools/LegacyMathTemporaryApply.Probe/README.md).

The stable-C20/current-INPUT candidate is a separate staged addition and is not
part of this transfer. Current-selected/C23/NEW NN, genuine kept actions, late
registration/resolution, stale cleanup and the outgoing original-document update
remain required before repeat recognition can be declared complete.
