# Exact-source Russian first-model zero-extent feature branch

`LegacyFirstPreparedAcousticClassifier.Classify` now handles actual normalized/merged geometry whose short points are all identical for the pinned Russian source6120 only. Previously the generic geometry exception aborted the entire word. This is a native feature-producer repair, not a score, pruning, language-model or confidence adjustment.

## Scope and ownership

One existing runtime seam is changed: after the existing chronological normalization and pen-up merge, the classifier asks the new internal `LegacyRussianFirstDegenerateFeatures` for the actual degenerate feature arrays. Otherwise the existing two generic extractors run unchanged. Context28, actual two boundary signs, prepared NN evaluators, class mask and integer-cost conversion remain unchanged.

Admission requires the private owned model's complete selected-source SHA256 `AA2AF98C8C550603E797F304DB204AEDD1102482CC8A6DDCC0ED8B62CF8113D2`, ordinary offset13082, existing effective normalization target100, and exact network kinds/widths1F/247 and2F/229. Caller-modified resource bytes produce a different model fingerprint and are refused by this helper. A noncollapsed candidate returns false. Generic geometry, other models, normalization defaults and directional313 behavior are not widened.

The helper checks cancellation, actual merged-count1..15/total4096 bounds, and every actual short point before allocating its two fixed arrays. Arrays are independently owned per call, with no shared mutable state or model mutation. This internal API is bounded to the exact admitted native profile; it does not promise a new generic feature format.

## Native proof

The preceding immutable static milestone is `review-russian-first-degenerate-static-frozen-manifest-20261021.json` (SHA256 `9186b4e75d89705dd0e3d2f941777a67050ef784b76acdd560b351568332ebee`). Its complete scoped bodies establish Text007470 zero-extent domain,00AEF0 dispatch,00A9F0 assignments,00AC70 constructor,1B75B0 producer and actual context/boundary append,1B0760 construction and the two-region1C2940 reader. Independent static audit passes7 complete functions/8 regions/3165 pins plus exact source/profile bytes.

Native00A9F0 assigns trajectory217: indices6..35 are1024;210..216 are `[1024,-768,0,0,-768,1024,-1024]`; other entries zero. Raster199 contains those seven statistics,64 entries?1024 and128 entries1024. These are meaningful degenerate assignments, not fabricated zero feature inputs. Resource profile19121 is exactly1011/3F with30/16/8/10 settings and seven zero scalars.

The selected source serializes normalization target0. The existing library reader currently resolves that to100. This batch preserves that behavior and **does not claim native default-resolution provenance**. Constructor/ingress default handling remains a separate next investigation.

## Independent verification

`LegacyRussianFirstDegenerate.Probe` consumes four actual previously failing normalized candidates from the frozen diagnostic capture; it never reads expected words. Its independent Python oracle reconstructs all217/199 basic shorts, appends actual context/signs, separately evaluates both quantized v106 networks from the pinned resource, then independently recomputes EXP probabilities and masked/unmasked costs. Four candidates cover1664 basic shorts,1904 prepared shorts,1440 integer probabilities and1440 costs.95 controls exercise exact/foreign owners, caller-resource mutation, independent vectors, noncollapsed refusal, generic old rejection, cancellation, bounds and concurrent calls.

Managed and NativeAOT vector captures are byte-identical, SHA256 `5d2e1da595598f3c310d4d2249abb55ee43cc1f77daef0c8bd114b3772799b1d`. Both modes build without warnings/errors.

The unchanged blind full-global harness replays all261 recorded inputs (continuous tracks, no inferred pen-up events). Both modes produce byte-identical capture SHA256 `f949995bd9e497bae710b13d8d1da27c72b1983d270a0cc503056f6de3b812fe`. All257 previously successful complete rows remain exact. The only changed rows are0130/0199/0207/0239: all four now reach actual raw WORD/UTF8 publication. They do not encounter the separately unimplemented directional zero-step branch in this corpus.

Only after full independent pinned-symbol/UTF8 emission audit were expected labels read. Exact words improve136?137/261, edits334?325/1276 characters (CER26.18%?25.47%); all261 now supported. This is a small corpus-specific improvement, not readiness for general Russian cursive or a representative/vendor-training-heldout benchmark.124 outputs still differ. Expected labels never drive inference or candidate selection.

Unchanged regressions captured in the preceding frozen revision (before the explicit zero-total guard only) include first-model synthetic/actual source guards and analytic NN controls, six synthetic/six actual prepared groups, four raw owning first fixtures/57NN groups, and English no-initial features124 controls/5degenerate/1merged/2reordered/255guards, in Managed and NativeAOT. Existing EN feature/probability oracle remains unchanged.

## Reproduction

```powershell
dotnet run --project tools/LegacyRussianFirstDegenerate.Probe -c Release -- models ACTUAL-FAILED-CAPTURE.json OUTPUT.json
python tools/LegacyRussianFirstDegenerate.Probe/audit_output.py models ACTUAL-FAILED-CAPTURE.json OUTPUT.json
dotnet publish tools/LegacyRussianFirstDegenerate.Probe -c Release -r win-x64 -p:PublishAot=true
```

Artifacts, full261 harness, build/regression logs, original comparison, post-inference quality report and exact hashes are under `E:/BlurMask-build-artifacts-20261014/review-russian-first-degenerate-v2-corpus-20261021`; the runtime frozen manifest enumerates them. Vendor binaries were only read statically; only independent library code was executed.

## Revision2 review guard

The helper now explicitly refuses zero-total geometry after its complete scan. Public `LegacyInkStroke` construction already requires1..4096 points. The additional negative probe deliberately uses `RuntimeHelpers.GetUninitializedObject` solely to construct malformed empty-stroke state; the helper refuses it instead of allocating features. This does not widen ordinary Classify admission. All95 controls and complete vector/NN/cost oracle were rerun in both Managed and NativeAOT; fresh full261 captures remain identical to revision1 in both modes. Earlier frozen artifacts are preserved without overwriting.
