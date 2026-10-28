# First resource-owned ordinary cache

`LegacyFirstOrdinaryCacheScorer` connects the prepared first row context/classifier
to the restored ordinary final cache. It loads the **current source's own inline
alphabet**, first model and first cache profile from the same private resource
snapshot. It does not substitute the alphabet from the second-model bundle.
Symbol masks, native pair lists, scope/controller flags and pen-up limits remain
explicit caller inputs. Native lexical word search/winner is a later consumer.

## Alphabet and output correspondence

The checked source247@6120 contains its own inline type2b/v103 alphabet at6219,
ending11353. Its header is `(179,140,FFFF,178,182,180,181,192)`. The complete
alphabet bytes are identical to the outer source's99–5233 bytes, but this byte
identity is a diagnostic check, not how the scorer selects its alphabet.

Native ownership is established by the following chain:

* Reader1d4950 delegates inherited134 to reader1d3210. In the inline-present
  branch,1d3210 reads type/version, creates that alphabet and writes resource+60.
* Frontend150d00 copies the selected resource's+60 into frontend+58 and uses its
  label count to allocate the count+3 buffer. Its ordinary initializer descriptor
  starts with that alphabet pointer, `local_a8=frontend+58`.
* Ordinary1b0760 takes descriptor param3[0] into runtime160, the acoustic
  alphabet. It resolves its header labels against that alphabet.
*1b9e20 consumes network probability entries consecutively (native leading bias
  entry skipped), preserving class ordinal. The180 output entries address the
  first180 labels of this owned192-label alphabet.1bb020/1bbf80 then produce the
  final195-entry cache, including virtual/correction and three extra slots.

This explains why raw class numbers must not be mapped through an unrelated
model or treated as final lexical labels. For example, the first model's raw179
is the header's disabled prototype; the final cache removes that raw entry.

## Shared tail reader

The additive `LegacyOrdinaryAcousticCacheProfile.ReadFirstSource` API loads a
validated typed first model internally and returns it with its profile. The
caller cannot supply an arbitrary prefix offset or an unrelated model instance.
The existing dynamic-model `ReadV110Node` API retains its prefix validation and
shares only the private tail reader. Its old behavior remains intact.

The first prefix ends187648; the checked parsed tail ends187725. Its fields are
fallback0, long/CJK dash weights0, negative/positive divisors0.9, closed weight0,
size/smoothing/power disabled, threshold85173, minimum1 and candidate limit10.
The reader retains its unsupported-branch checks. Remaining serialized fields
are not reinterpreted as missing defaults.

Final cache input uses the **merged normalized glyph**, matching the pen-up
reduction stage. Primary symbol masks also address the owned alphabet, and only
their first network-width entries are used for the raw acoustic ensemble.

## Checks and limits

```powershell
python tools/LegacyAcousticCache.Probe/reference_controls.py .artifacts/old-cache-controls.json
dotnet run --project tools/LegacyFirstCache.Probe -c Release -- .artifacts/old-cache-controls.json
dotnet run --project tools/LegacyFirstCache.Probe -c Release -- .artifacts/old-cache-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json
```

Vendor-free own source/networks/alphabet yield two equal5678-cost cache candidates.
Truncated parent objects, encoded ordinary nodes and mismatched masks are rejected.
Actual resource mode verifies the first inline alphabet identity and follows the
float context producer through raw180 to final195 for three own printed glyphs.
With explicit all-enabled masks, empty pairs and explicit pruning/suppression/
knowledge/scope flags, the leading candidates are:

| Own glyph | Leading final cache candidates |
| --- | --- |
|M|LatinM6172, CyrillicМ16456, Cyrillicм19777|
|И|CyrillicИ16233, Latinn31589, CyrillicН32627|
|Р|Latinp14405, o15169, LatinP19020|

These are diagnostic candidates, not a recognized word or a cursive benchmark.
The first cache/controller scope defaults, native language/source permissions,
variable-group word search, full winner/confidence and representative connected
Russian handwriting remain separate. No Russian AI-pen UI enablement occurs.

The targeted shared-reader change passes the existing768 independent cache/
shortlist,512 direction and32 serialized-profile controls plus all old actual
resource cases, in Managed and Windows Native AOT. The new first-cache probe
also passes both runtimes. It does not call a vendor DLL or start a GUI.
