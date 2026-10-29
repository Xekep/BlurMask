# Retained repeat payloads and prepared DOM kernel

Math87 adds a preparation stage between the actual repeat export and its future
original-document receiver. The exported source commands, DOM payload writer
and associated objects are retained through a private one-use lease, issued
under the genuine owning read chain. Payload-ready is set after that chain
returns. No snapshot becomes authority, no original receipt is consumed and no
original registry/DOM mutation occurs at this stage.

Disposing the exporter revokes its reads while deferring payload clearing until
the lease is disposed. Releasing recognition ancestors does not destroy the
retained payload, nor does payload readiness establish original-app readiness.
The future receiver must revalidate the original app, current raw revision and
epoch, consume old read authority before effects and issue a successor only
after completion fences. That receiver, generated journal, actual mutation and
rollback/successor behavior are not implemented by this increment.

The prepared DOM kernel prebinds all parent paths, then uses current child
ordinals for sequential edits. Scalar updates retain existing attribute identity;
mapping sentinel-1 preserves the incoming payload tag value. Attribute removal
detaches without the child deletion/free operation. Only the proved canonical
scalar profile is accepted; nonidentity child-tag mapping and native text setter
remain unsupported. Path/type prebinding is not the complete native scratch
preflight, and prepared operands confer no original authority.

Before compilation, independent review caught the sentinel case and a test
encoding error. The first prepared-test compile also exposed ambiguous
target-typed params constructors; five explicit type qualifications fixed only
test syntax. All original failed/source attempts are preserved.

Validation:

- All46 prepared controls pass in Managed and Native AOT.
- Six genuine repeat exports and six unchanged NN regressions exercise856
  owning preparation controls and48 independent lease checks, including
  precancel, foreign/reentry/oneuse, original C22 identities, deferred clearing
  and six different ancestor disposals. Managed/native captures are byte-equal.
  Post-bind ancestor-return failure injection is not available and is not claimed.
- The original unleased1156-control export probe and all seven independent
  audits remain byte-identical to the Math85 baseline.
- Actual Main Math87 and its two permanent probes were rebuilt/PDB-authenticated
  with Core951. The Main prepared and owning runs pass; the Main owning capture
  equals the independently reviewed isolated Managed/native capture.

The CI retains the original full export checks and adds the prepared/lease
probes. Lease captures intentionally omit unrelated old failure arrays; they use
the five common content audits plus their own lease audit rather than claiming
the old late/export failure coverage again.

Evidence: `E:/BlurMask-build-artifacts-20261014/prior-repeat-original-apply-next-20261029`.
Actual Main receipt `root-main87/FINAL.json` SHA256:
`31f005042e7d6e99f04a56dd6b2f9bf7dbfa8cb87bafc297f07b933c5414e18f`.
Unleased regression receipt `root-regression87/FINAL.json` SHA256:
`6fc01994825cb19fa54f180308fd8960e6ccff34ac95221f8407fa4226fb2127`.
