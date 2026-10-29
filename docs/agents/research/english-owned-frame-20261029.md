# Private English LAST frame admission

The five-file `owned-last-frame26-v3` revision is integrated into Core951.
It removes three 944-byte clones and a discarded wrapper from the private
ordinary LAST-frame producer. Public preparation still clones its inputs;
both 88-byte source-row copies and context/callback behavior are retained.

The real store checks thread, active lease, continuation identity, current LAST
phase and ring identity before touching the non-thread-safe work ledger.
Structural validation is separately prepaid and still executes. Normalization
reserves shifts from the actual frame's removable-symbol count, bounded by
`12*r*(2*n-r-1)` bytes; dense removal can cost more than the old reservation.
The fifty-symbol ancestry bound, cancellation checks and ledger ceilings remain.

Source admission preserves the original 14 additions, 67 overrides, 13 protected
files and historical metadata. A separate five-body revision authenticates
three checkpoint preimages and two baseline935 preimages. The baseline content
aggregate verifies the latter without accepting their old bodies as current.
The source gate now includes 26 checks, including mutation, wrong ancestry,
unknown scope and a genuine old-body rollback. CI freezes the rollback fixture.

## Verification scope

The frozen candidate packet is
`E:/BlurMask-build-artifacts-20261014/english-frame26-source951-root-v3-20261029`;
its manifest SHA256 is
`040c4d2c4546dddee0d0eec3c6d1b49a52f60739455dccfc621a17dadbec30c8`.
Its Managed/diagnostic Native AOT captures agree on all logical data and ledger
values, apart from independently validated fresh UUIDs. Five actual-store fault
controls cover wrong thread, precancellation, revoked ownership and cancellation
or revocation immediately before normalization. These are diagnostic probes,
not a production AOT distribution-size or broad accuracy claim.

On the unchanged one-stroke/three-point input, visits fall from 16,776,858 to
14,744,762: 2,032,096 fewer, approximately 12.1%. Allocation/copy ledger values
also fall, but they are accounting envelopes rather than measured RSS. No
application latency or real-word quality improvement is established by this
small input.

Actual Main sources were rebuilt and PDB-authenticated: normal951 and test-only953,
with every normal source body checked against the isolated candidate. Independent
public controls pass: 1536 normalizations, 512 context differences and 768 complete
frame/context/callback cases. The genuine English owning capture matches the
candidate including its ledger. Three actual Main publication calls and the
independent paired audit pass. Math87 retains its byte-identical capture across
1156 controls/seven independent audits; all nine original Russian public calls
retain complete data except measured wall time.

Main evidence root:
`E:/BlurMask-build-artifacts-20261014/frame26-main-admission-20261029`.

| Receipt | SHA256 |
| --- | --- |
| `verified-v3/MAIN-FINAL.json` | `e505936400a40ad5737b5818edbb4cbe90c9fc797791f8eea94caa326428cde5` |
| `main-regressions/FINAL.json` | `b24c08b96d94797bde51efe496c22070e249161fb71bb5e61eace13de0e804ae` |
| `paired-publication/FINAL.json` | `bd957d119f447a9befb2d34cd67d1f928141b6553675998cc117e25f0f5e28f5` |

Failed orchestration attempts are retained: the first exposed a CRLF-sensitive
fixture assertion (corrected by the same canonical-LF rule as admission), and
the second hit a log-name suffix collision after a successful Main build.
The final run reauthenticated the completed build and used fresh logs; no
recognizer code, inputs, budget or expected recognition result was changed to
clear those failures.

Public positioned English admission, Russian/English cursive quality, automatic
classification and complete application integration remain incomplete. This
revision does not complete the engine or authorize a release.
