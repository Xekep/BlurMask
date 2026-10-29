# Existing fresh versus unchanged-prior attention histories

The current-ingress probe's opt-in schema2 sidecar now observes the fresh
inference that `Pipeline.Create` already performed, before the old source/DOM
application. It reads the real source's guarded history and verifies completion,
decoder-call counts, dimensions and each retained float32 SHA. It neither
executes another neural pass nor supplies hypotheses or tensors to an owner.
The original twelve unchanged/current histories remain captured and audited.

The auditor requires all six fresh histories to match the corresponding actual
unchanged-prior history exactly: input-frame count, search width, reversed flag,
decoder calls, frame shapes, SHA and every float bit. Same-platform repetition
and Managed/Native parity also include the fresh histories. Seven corruption
controls reject a coherent changed float/self-hash, missing/duplicate case,
wrong route/width/input frames and omitted fresh evidence.

Root reauthenticated actual Main Core951 and Math87 through their PDBs, built
the three-source probe with zero warnings/errors and tested opt-in versus
disabled observation. All original473 controls pass, and the original capture
is byte-identical with observation disabled, enabled, repeated and Native AOT.
All six Windows fresh/prior histories agree exactly; all eighteen histories
agree between Managed, repeat and Native AOT. The historical unchanged-reference
audit remains enabled and passes on this Windows run.

Evidence: `E:/BlurMask-build-artifacts-20261014/math-fresh-twin-20261029`.
`root-validation/FINAL.json` SHA256:
`7103f9d8bb218c000c235dcc797b805a8550a668f2f1725c74762e6695e208ae`.

This is an exact cross-route consistency check sharing the feature/NN kernels,
not an independent neural mathematical oracle. It does not directly observe
the submitted encoder input tensor or prove why macOS differs from the frozen
Windows fingerprints. No tolerance, platform-specific expected hash or reference
waiver is added. The existing frozen-reference audit remains a release gate;
the new Linux/macOS CI observations are required before changing its contract.
No full repeat application, handwriting quality or release readiness follows.
