# Native post-load cased alphabet ranges

`LegacyNativeCaseAlphabetRanges` restores `058200` over a parsed owned alphabet,
`LegacyNativeCaseData`, and the pinned native script table. It prepares the four
runtime words `D0/D2/D4/D6` consumed by later casing wrappers. Other derived
alphabet metadata and the full wrapper292 query are separate producers.

`LegacyInkAlphabet.IsCaseRangeMetadataEnabled` preserves the actual reader flag:
version103 derives it from nonempty stored form entries; version104 reads its
explicit byte. The previous loader already consumed that byte, but discarded
its value. Lookup tables, related forms, byte count and existing label APIs are
unchanged.

`IsCasedSymbol` implements `059A70`. If any stored relation map exists, it tests
map membership for that symbol across the three native modes. An unmapped
symbol does NOT fall back to Unicode merely because it looks like a letter.
Only when all three maps are absent does the function scan a nonvirtual
symbol's decoded scalars, skip script24 and consult scalar case alternatives.
This differs from the alphabet-wide capability predicate `052BD0`.

`058200` returns unchanged FFFF ranges when metadata is disabled. Otherwise it
finds at most two contiguous runs, preserving the native states at end of input:
a first run reaching the end keeps `FirstEnd=FFFF`; a second run reaching the
end sets `SecondEnd` to the count, one past the last label. A third run is
refused. These states are deliberately not changed into ordinary closed-range
conventions. `LastCasedBoundary` uses the second end when present, otherwise the
first end, matching the later2324E0 selector.

The actual Russian acoustic resource yields `[10,127,FFFF,FFFF]`; the actual
lexical resource's disabled metadata yields fourFFFF words even though its
alphabet-wide Unicode capability is positive. This distinction is required by
the source factory rather than inferred from a chosen script.

An independent byte-level alphabet parser and run-domain oracle supply514
cases, including the two actual resources, explicit104 enable/disable states,
209 third-run refusals and98,496 stored-map predicates. Prepared modified maps
are clearly diagnostic, not actual model results. Managed and Windows Native
AOT checks also cover owner identity and cancellation. The optional static
checker verifies the reader, constructor sentinels, predicate and refusal
branches without executing a vendor DLL.

This post-load component does not implement the remaining alphabet property
arrays, mutable-source refresh, wrapper292 preparation/query or application
recognition. Russian cursive is not production-ready.

```powershell
python tools/LegacyCaseAlphabetRanges.Probe/reference_controls.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res E:/case-ranges.json
dotnet run --project tools/LegacyCaseAlphabetRanges.Probe -c Release -- models/text-unicode/legacy-case-data.json models/text-unicode/legacy-text-publication-data.json E:/case-ranges.json
python tools/handwriting/audit_legacy_case_alphabet_ranges.py --output E:/case-ranges-static.json
```
