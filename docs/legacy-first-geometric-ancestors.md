# First primary geometric ancestors

`LegacyFirstPrimaryGeometricAncestors` restores the packed previous-row walk inside `1EE3B0`. It binds the audited fresh first zero-trait policy and an owned/pruned source graph, and snapshots caller-supplied primary records after validating their dimensions, arc spans, predecessor identities and graph compatibility. It does not prove that supplied scores came from the neural network.

The native walk skips blank `E2` and shadda `E6`; when `E0` is false it also skips header symbols `EA/EC/EE`. For the current first resource these are blank 192, absent shadda FFFF and newline/space/NBSP 182/180/181. Each step reads previous-alternative byte 9 and the three-bit span in word E. If the next node is -1, it stops **before** dereferencing byte 9: a skipped root is retained as the geometric ancestor, not converted to a missing parent.

The result keeps the original acoustic parent node, alternative, cost and record separately from the ancestor used for geometry. Native `uStack_bfa` receives the ancestor arc while `uStack_bf8` receives the original arc. `iStack_c04` receives the ancestor node; original-node/alternative storage remains unchanged. The replacement-symbol xor eligibility check uses the original prior symbol, but the later sampler request's prior symbol comes from the ancestor. A single rewritten narrow previous-node request would lose these distinctions.

The probe creates 256 explicit scalar primary scenarios over an actual fresh-source/pruned graph. An independent Python decoder of the 40-byte rows verifies 4,608 walks, 5,801 skipped records and 1,158 skipped-root stops while preserving original costs. The resolved ancestors feed 4,096 actual owned blank-pair callbacks. These are prepared scalar controls rather than a handwriting accuracy corpus or complete native blank primary execution.

The full `F0/F8` compatibility request, blank acoustic fallback and insertion into the primary table remain missing consumers. This resolver therefore does not enable automatic Russian recognition or claim a complete controller winner.

```powershell
dotnet run --project tools/LegacyFirstAncestor.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-separation-profile.json D:/ancestors.json
python tools/LegacyFirstAncestor.Probe/audit_output.py D:/ancestors.json
python tools/handwriting/audit_first_geometric_ancestors.py --output D:/static.json
```
