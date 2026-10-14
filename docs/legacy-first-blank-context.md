# Full first blank and ancestor context passes

`LegacyFirstBlankContextPasses` composes the owning first-network/cache primary producer with its actual group-cost query and first-source sampler geometry. It restores the B0/B8 blank/header ancestor branches without deleting unsupported rows. Public entries accept neither a supplied primary nor a cost/success override.

Context ancestry differs from primary ancestry. B0 walks prior B0 tables and B8 walks prior B8 tables. Each context record stores the previous primary-row index in byte 9 and the previous context-alternative index in byte A. The packed span identifies the earlier node. The original context parent retains its cost, identity and F0/F8 graph endpoint; the separately resolved ancestor supplies sampler geometry. A skipped root is retained before the walk dereferences its predecessor sentinel.

A current blank E2 restricts the original primary-row choice to the current primary record's byte-9 predecessor. Selected-source B8 controls additional consecutive-blank exclusion. B0 kind 0 shares the original identity for E2 only; B8 kind 0 shares it for E2 and E4. B0 kind 2 starts a new symbolic root; B8 kind 2 with E0 false inserts FFFB before the current symbol. Blank/shadda does not receive the alternate kind-2 branch. The context sampler order remains 140 then 118, reverse of the primary order.

The authorized internal `FromOwnedTables` bridge validates B0/B8 dimensions, active primary rows, current symbol/arc/span/category, finite costs, symbolic identity membership and predecessor primary-row/context-column tuples, then deep-copies all table storage. It returns the existing context result type for downstream materialization compatibility. Malformed tuples, identities, dimensions and subsequent caller table mutation are covered.

Eight real model captures score the same 57 own-fixture groups as the prior producing-kernel phase. Their complete contexts preserve 340 records and observed literal traces: printed MIR still produces mixed-script `MИP` at 41,268. These real beams retain no blanks, so their ancestor walks are not presented as demonstrated real handwriting recognition.

Separate **internal friend-probe controls** provide 128 explicit scalar primary tables over the actual owned/pruned graph. The public recognition path never sees those overrides. An independent Python tuple oracle verifies 1,569 synthetic context records, 9,010 callbacks, 2,409 successful skipped-ancestor walks, 2,200 blank-parent restrictions and 81 blank identity shares. This separates branch correctness from model accuracy. The actual eight captures independently verify 340 records and 2,160 callbacks.

The source scope remains the audited fresh first alphabet with no Arabic traits, E0 false and alias folding disabled. The full native protocol-owner 194 binding, general/reused source branches, main linguistic winner and a representative Russian cursive corpus remain unfinished. This phase does not enable production Russian recognition.

```powershell
dotnet run --project tools/LegacyFirstBlankContext.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json D:/contexts.json
python tools/LegacyFirstBlankContext.Probe/audit_output.py D:/contexts.json
python tools/handwriting/audit_first_blank_context.py --output D:/static.json
```
