# Owned ordinary Russian glyph production

`LegacyOwnedRussianOrdinaryGlyphProducer` restores the complete bounded
ordinary subtype0 branch of Text `216BC0`. It binds a
`LegacyFreshCaseMainSourcePolicy`, the same case dictionary's
`LegacyOwnedRussianMainGlyphConfidence`, its actual sampled graph and the
checked native separation table. Both the single-physical main source and
the single-descriptor, multi-physical main line use their own classifier,
normalization and context producers. No first-child confidence or frame is
substituted for the main source.

The 72-byte backtrack row remains an **explicit prepared input**. Its node0
and arc4 must identify the owned graph. Its subtype3C must be zero. If its
signed field20 is not minus-one, the caller must also supply complete
prepared compact40 storage containing that indexed row. Native21709C reads
its symbol1C before calling216860; this read is checked even though the
no-wide Russian resolver does not consume the neighbor value. The API does
not construct these72/40 rows, infer their decoder winner, recover source
references or implement full218560/21FFD0.

The symbol resolver uses this dictionary's exact acoustic alphabet and
same-owned case preparation. That preparation proves no Arabic labels;
the actual alphabet also has no wide pairs. Therefore width-mode BC and
the unused neighbor/caller scalar arguments do not change this branch.
Synthetic quote conversion uses only the low byte of the supplied conversion
argument. Primary21FE30 passes zero; alternate219580 passes one. Independently
of conversion, scoring always maps193 to apostrophe134 and194 to quote129.
The emitted label stays193/194 when conversion is zero.

Fresh00C860 clears Knowledge88 and99. Selected1D60D0 preparation does not
overwrite them, and the typed factory excludes property/reuse overrides.
The owned confidence adapter supplies channel0 from real final candidates,
with the native top20 cutoff, original-symbol keyed192x4 calibration and
actual primitive probability service. The external2458B0 wrapper temporarily
sets99 and is outside this fresh direct invocation scope. Ordinary21B's
frontend280 is false, so subtype0 publishes flags06=0 without a caller flag.

The geometry request has node0/arc4, candidate ordinal6=FFFF, remapped symbol8,
signed geometry indexA, byteB=0 and dwordC=0. Mismatch3D selects the end
composite when present, otherwise the start composite. F0001 uses indexFF;
the native bypass prevents dereferencing it. Actual source raw100=0 assigns
the same193860/195520 separation cost to every geometry prototype. The eight
native bypass labels are191,164,FFFF,187,188,182,180,181. The last three come
from stored alphabet headers10/12/14, not guessed Unicode categories.

`AL+E` is the unit-score label178, **not** ALCount192. That label and
FFF7..FFF9 return score0C=1, score10=1 and cost14=0. FFFA..FFFC require the
separate delimiter/multi-output callback and are refused. FFFD requires
frontend1A8 and remains unsupported. Other unchecked synthetic labels are
outside this portable scope, although216860 itself accepts arbitrary
ushort values.

`LegacyOrdinaryGlyphProductionScratch` copies the256-label scratch and exposes
scalar residue for precise partial-state checks. Structural portable scope
guards precede entry. Once entered, the producer clears component/external
flags, decrements the signed cursor, resolves/appends one label, writes0C,
then10,14 and finally06. Division of byte3E by255 uses float arithmetic.
No rollback is promised. For example, unsupported FFFD retains the decremented
cursor and cleared flags, while preserving labels and score residue. Scratch
underflow/overflow is a checked refusal after the decrement. Cancellation is
a portable interruption, not a reconstructed native cancellation callback.
No native false-return branch is reachable in this supported no-wide,
no-Arabic resolver; unsupported services are never replaced with dummy success.

The returned `LegacyPreparedGlyphFinalizationInput` feeds the existing2173E0
nontracing finalizer. The tests cover3104 prepared72 rows over46 actual NN/cache
groups from one physical stroke and two physical strokes in one assigned line.
There are368 synthetic-quote cases and736 unit-score cases,16 groups with a
nonzero independently computed separation cost, complete256-label residue
checks,3104 complete40-byte finalizations,22 guards and four retained-prefix
refusals. These are scalar/ownership controls, not handwriting accuracy or
actual selected-word materialization. In these raw fixtures no closed primitive
is retained; its retained-confidence branch is tested separately by the
confidence component and is not claimed as an actual observation here.

The Python oracle independently reads the pinned resource's alphabet,192x4
calibration and main category maps and computes float32 grouping costs from
actual sampled points/ranges. It verifies every emitted label, scalar channel,
cost, scratch byte and final glyph40 byte. Confidence allows at most2ULP for
cross-platform `exp`; other writes are exact. The static audit pins the complete
216BC0/216860 bodies plus confidence/cost getters, exact instructions and actual
frontend/graph vtable bindings. Vendor binaries are read only as data.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet run --project tools/LegacyOrdinaryGlyphProduction.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/ordinary-glyph-build -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/text-unicode/legacy-case-data.json models/text-unicode/legacy-text-publication-data.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res E:/BlurMask-build-artifacts-20261014/ordinary-glyph-managed.json
python tools/LegacyOrdinaryGlyphProduction.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-separation-profile.json E:/BlurMask-build-artifacts-20261014/ordinary-glyph-managed.json
python tools/handwriting/audit_ordinary_glyph_production.py E:/BlurMask-build-artifacts-20261014/ordinary-glyph-static.json
```
