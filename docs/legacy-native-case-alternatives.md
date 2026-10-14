# Portable native scalar case alternatives

`LegacyNativeCaseData` reads the fingerprinted numeric tables in
`models/text-unicode/legacy-case-data.json`. The independent library needs no
vendor DLL, decompiler output, or public Unicode files at runtime.

## Recovered behavior

Engine `05F4C0` first returns zero for U+00B5. Otherwise it counts/emits every
special row for the scalar in stored order. Each row retains lower, upper and
title sequences and the two original opaque language/context flag words. A
context consumer must select these alternatives later; the scalar function does
not filter them by the current language or surrounding characters.

The final generic variant follows the native alternating-range precedence,
then the separate lower/upper ranges and singleton maps. An alternating-range
hit suppresses fallback even when its parity supplies no mapping. Missing lower
or upper mapping keeps the original scalar; the eight title overrides precede
the upper fallback. Output capacity truncates writes but leaves the total count
unchanged. The portable result owns copied scalar sequences instead of native
pointers into72-byte records. No canonical host `ToUpper`/`ToLower` substitution
is used.

`LegacyInkAlphabet.HasStoredRelationMaps` exposes the three actual stored-map
pointers represented by the parsed resource. `HasCaseAlternatives` restores
`052BD0`: any map activates the predicate immediately; otherwise `04FFE0` scans
nonvirtual alphabet symbols, skips script24, and tests `05F4C0(cp,NULL,0)`.
Missing text is an explicit unsupported native error, never a false result.
The actual Russian acoustic alphabet takes the stored-map branch; its lexical
alphabet has no stored maps but still takes the positive Unicode fallback.

## Source identity and public reference differences

The exporter reads tables from the pinned engine SHA
`d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c`.
The numeric table hash is
`5e99b64e4e7c2893a50e60552eb5c6a11894e466e18b8c4945071b5699916c04`.
There are58 alternating ranges,34 lower ranges,89 lower singletons,35 upper
ranges,110 upper singletons,8 title overrides,130 special rows and348 sequence
scalars. The loader checks counts, widths, provenance, work budget and numeric
checksum before accepting the data.

Unicode9 is a **reference**, not a claimed version of these native case tables.
The public reference comparison retains46 presence differences and54 generic
triple differences, including legacy Georgian and titlecase digraph behavior.
Neither difference list changes the exported native tables. The public files
are optional exporter inputs and are unnecessary for recognition runtime.

## Verification and remaining consumers

A separate set/dictionary Python oracle builds counts and ordered variants.
Managed and Windows NativeAOT probes check all1,112,064 valid Unicode scalars,
2,845 nonzero counts and14,541 writes across capacities0 through5. Both actual
Russian alphabet branches, cancellation, U+00B5, invalid scalars, truncated
JSON and oversized input are checked. Optional static evidence checks the
stored-map assembly, scalar table dispatch and factory wiring without executing
vendor code.

`20C9D0` consults this predicate, then selects wrapper292 when the default
resource flags `68 & 7` equal1. That frontier is identified, but wrapper292
preparation, stateful queries, language/context filtering, normalization and
refresh are not completed by this scalar component. This does not enable the
new engine in the application or establish Russian cursive accuracy.

```powershell
python tools/LegacyNativeCase.Probe/reference_controls.py models/text-unicode/legacy-case-data.json D:/case-controls.json
dotnet run --project tools/LegacyNativeCase.Probe -c Release -- models/text-unicode/legacy-case-data.json D:/case-controls.json models/text-unicode/legacy-text-publication-data.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res
python tools/handwriting/audit_legacy_case_binding.py --output D:/case-static.json
```
