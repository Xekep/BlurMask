# Ordinary dictionary request preparation

`LegacyOrdinaryDictionaryQueryPreparation` ports the bounded request-field and
gate sections of MyScriptText `1fd240` and `201270`. These are producers for the
existing packed source and aggregate query consumers, rather than a replacement
lexical decoder. Source binary SHA256 is
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
Inspection is static; probes execute only independent managed/native AOT code.

Both producers receive prepared policy values, the live frontend `virtual280`
result and explicit owner/span/input flags. They preserve the query's context
container, capacity and internal dictionary cache identity. They do not infer
the complete frontend/controller preparation, graph scheduler or native winner.

Initial `1fd240` takes runtime byte2601 as the limit mode, overridden by
`(inputFlags & 3) != 0` when frontend280 is true. The gate allows mode zero or
signed32 span at most unsigned-byte2602. Accepted preparation writes the full
startup tag `8 << 40` (node0, source0, header8), parent context0, category18=0,
parameter19=limit when enabled, owner at1c, span at20, flag24=4 only when
runtime254c and input flag10 are both set, unchanged input flags at26 and skip0.

Continuation `201270` reads a complete88-byte stored row. It retains that row's
full tag00, uses byte26 for the count and requires signed byte21 to be nonnegative.
The mode gate uses signed32 of wrapping unsigned `parentCount + span`. The
previous-distance gate uses signed32 of wrapping unsigned `owner - row38`, at
most prepared limit54 unless suppress-previous2609 is enabled. Parameter19 is
`limit - parentCount` only for enabled mode and count below limit; otherwise0.
All remaining writes match the initial producer. A rejected gate leaves every
query field unchanged. Trace allocation, downstream alternate-candidate logic,
`1fe740` alternate-parent queries and `204290` auxiliary requests remain distinct.

`Parameter19` is now explicit on the portable query. The packed query-cache key
does not consume it, matching the restored `093b50` key. In this ordinary section
parent context10 is explicitly zero: source queries therefore return their
initial per-symbol contexts. The separate nonzero-parent lookup branch must not
be substituted merely because it also exists in the source implementation.

The independent Python controls cover2048 initial and2048 continuation requests,
1893 accepted mutations, unsigned/signed boundary wrap, count/state/distance
gates, frontend overrides, boundary flags, preserved ownership and canceled or
truncated inputs. Actual RU resource mode connects initial preparation to132
root records/two pages and reuses the same request for the first expanded branch.
The diagnostic graph owner/span and stored row are supplied explicitly; this is
not an automatic handwritten-word recognition or confidence check.

```powershell
python tools/LegacyOrdinaryDictionaryQuery.Probe/reference_controls.py .artifacts/ordinary-query-controls.json
dotnet run --project tools/LegacyOrdinaryDictionaryQuery.Probe -c Release -- .artifacts/ordinary-query-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```
