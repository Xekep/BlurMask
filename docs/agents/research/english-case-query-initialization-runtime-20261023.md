# English case query initialization: bounded runtime component

This component constructs the fresh one-13B English 290/292 prefix. It does not expose Query, First, Continue, a scorer, a D0 session, a terminal, or a recognized word. Its public result contains defensive copies of the initial topology and metadata. These are historical initialization snapshots, not a live query source after any internal diagnostic D0 operation.

The implementation consumes the actual completed post-cut MAIN owner and the exact same Source identity used by the English numeric owner. It retains the ordinary graph identity, constructs actual dictionary/normal292/auxiliary292/aggregate and delimiter290 objects, and creates their native control contexts. Delegates are genuine same-owner readers; none is called during initialization. The numerical tables are copied from the actual source without evaluating a query.

The initial AVL has 86 nodes. The native constructor sequence resolves FFFB, FFF9, FFFB, FFFC, FFFA: the two FFFB references preserve the already-existing node, and only three nodes are added. The completed prefix therefore has 89 nodes under the unchanged library component capacity104. That capacity is not a native lifetime-pool limit.

## Logical memory envelope

`AdditionalLogicalEnvelopeBytes` is 2,907,401 bytes, checked before entry. The default accepted budget is3MiB and the maximum accepted caller budget remains16MiB. This is a conservative logical retained/transient envelope, not a claim about cumulative CLR allocation or process RSS.

The previous source reservation Broot remains1,048,576 bytes. The additional non-graph payload is810,249 bytes: code arrays644,893; cost arrays6,144; case data7,436; context objects20,224; paired retained/export snapshots33,600; maps3,744; label structures8,192; outer objects18,432; case objects2,048; bookkeeping65,536. Input models, completed MAIN owner and caller-owned case/publication tables precede this additional envelope.

The graph adapter is newly allocated, not borrowed. A separate Bgraph1,048,576 reservation covers the whole node-reference array, each node's Arc[] and read-only wrapper, the graph wrapper, and transient GetNode/LINQ selection objects. Preflight checks N<=4096 and A<=4096 before conversion. A conservative portable logical charge160*N+32*A+65,536 is at most851,968, rounded to1MiB. No graph-connection cache is created: GetSuccessorOrdinals is never invoked by this stage. The bounds do not depend on expected handwriting labels.

Each public snapshot copy is separately bounded by the same finite89-node/104-map layout and is created outside the root gate. Repeated caller retention can consume additional caller memory; this API does not claim a cumulative lifetime allocation cap.

## Identity, leases, cancellation

A private issuing plan is one-use: inputs, prefix, root binding, final publication. The exact issued292 binding reference is consumed once. Foreign objects with the same numeric content cannot obtain source authority. Root binding additionally requires the same ContextFactory reference as both root maps and prefix. Before any private reader is exposed, the private dictionary receives the root's exact initial WorkingMap through its O(1) setter; no callbacks or other gates occur.

Snapshot access claims an opaque lease under the existing root gate, then copies outside that gate. This gate is also used by real score and D0 operations. Busy or canceled-before-entry attempts do not poison another active operation. A failure after a successful claim poisons only that exact owner and lease. Stale or forged leases cannot clear an active operation. Cancellation after root binding or publication prevents the factory from returning a reusable partial result.

A subsequent D0 forwarding/query/history bridge is intentionally absent. Internal probes can create genuine score/D0 operations to test the gate, but those handles are never returned by the public initializer. No whole-word, configured-root, default-engine or recognition-quality claim follows from this component.

## Exact old seams

Only three old files change. The source factory accepts an optional private plan; its null path restores every original byte. The tagged case adapter extracts the literal original closure and accepts its original dictionary explicitly (the approved capture correction). The sparse evaluator extracts unchanged numerical arithmetic into an internal pure helper; public query signatures and guards remain unchanged. A Roslyn token/AST comparison verifies both extracted bodies and all original public method signatures.

The NEW4 files contain all new initialization ownership. There is no arbitrary caller category/history/suppress input, no fabricated zero request80, and no numeric Evaluate during initialization.

## Verification

The independent Python oracle rebuilds AVL topology by heights rather than the production balance-state code, checks all nine node fields, roots/pages/capacities/cursors, all initial maps and five control identities,492 classifications,217 form triples,644,893 code bytes,1,536 cost entries,420 policy bytes and224 selector bytes against actual raw models and pinned PE constants. It uses no expected word labels.

The final ownership probe includes58 strict guards and18 cancellation controls, real thread contention against score and D0, foreign-context binding rejection, forged/repeated binding rejection, and stale/forged lease controls. Full134 and100 captures completed Managed/NativeAOT numerical parity before the envelope-only successor. The successor changes only budget/default/property naming and is checked with fresh tiny Managed/NativeAOT ownership controls. An actual final-V3 NativeAOT134 capture also passed the full independent oracle (SHA256091a8a181c37d41b8d3e9ce8bda7e53400c6eac95f071f8dd05d4a29720aefac). Every field of that capture is identical to V2 except Budget1858825->2907401. Full100 remains the genuine prior-V2 Managed/AOT numerical evidence; it was not relabeled as a final-V3 execution. The budget-only source diff plus tiny equality validates unchanged algorithms; no new100 inference run is claimed. Old M/N/D0, sparse query/history and affected Russian regressions are recorded in the frozen evidence manifest. This component changes no model, weight, threshold or recognition algorithm.

## Portable oracle invocation

CI requires no SDK DLL. Run `python -I -B tools/LegacyEnglishCaseInitialization.Probe/audit_output.py --repo REPO --models MODELS --capture CAPTURE.json --output REPORT.json`. The adjacent reference_policy.json contains420+224 original raw table bytes extracted from pinned MyScriptText.dll, with module SHA, virtual addresses and independent span hashes. The oracle pins those hashes and sizes independently; it compares the capture against the snapshot, never against C# constants or another capture.

For an additional local read-only binary check append `--sdk SDK_DIRECTORY`. This verifies the complete PE module SHA and compares both raw spans to the same portable snapshot. The vendor DLL is never executed or loaded. Both modes were checked on the same actual final134 capture without repeating inference. The original134 input is en-main-final-single-blind-raw.json (141records;134admitted/7refused), and the supplement manifest records its exact external path and SHA together with blind100 and tiny inputs.
