# Native case-path source292

The new normal source is an executable233090 producer over the checked2B7
dictionary reader. It generates ordered record32 variants, context paths and
integer corrections; it does not replace strings after recognition.

`LegacyNativeCasePathPolicy` carries the exact14/28 state policies selected by
232090 for TCM0/200, their15-byte rows and8-byte classification/form selectors.
The200 metadata lookup indexes32 signed bytes at2E8998, including the zero
tail for emitted terminal state31. This is distinct from the28 policy rows;
terminal31 is valid for metadata, not a normal source-query table index.
`LegacyNativeCaseRecordExpansion` applies the prepared stored-form tables,
including native+1 and initial-state+8192 corrections, terminal duplicates,
Dutch i/j transitions and oldest-first suffix rewriting through the same
`LegacySymbolContextFactory`. Record bytes outside the assigned fields survive.

`LegacyNativeCaseNormalQuerySource` owns eight128-row backend buffers. Its cache
key, reset, backend offset and emitted-count fields follow233090. The caller's
output capacity leaves four rows of slack before expanding the next record.
Pre-skip counting skips complete source records, even when a requested offset
falls inside that record's variants. It must not be implemented by slicing a
fully expanded output. Calls require serialized access; failed processing
invalidates the session, cancellation is checked in both loops and around the
backend invocation. A zero backend page replaces the previous block count so
the subsequent read does not advance past the same exhausted block twice.

## Producer ownership

2324E0 stores its frontend in source+10. Thus233090's+280 is **not an alphabet
predicate**. Class222.V280 is155710, forwarding graph21B.V1C8 to05A400=false.
The typed ordinary graph closes that branch. The case preparation still proves
the separately owned non-Arabic alphabet/form tables.

20C9D0 source selection consults raw resource13B.V40=1D6DD0, which reads its
Field68. This is distinct from the prepared2B7 descriptor flags such asC0A.
The actual Russian dictionary at606964 has Field68=1. For container setting90
equalFFFFFFFF, the native selector chooses Field68&7; otherwise it uses the
explicit setting. Setting1 selects292, setting2 selects291. The helper refuses
291 and unwrapped modes rather than substituting a case path.

00C860 initializes container90/94 toFFFFFFFF, but that constructor alone does
not prove that later properties left them unchanged. Therefore
`LegacyNativeCaseDictionaryFactory.Bind292` requires the actual prepared
container90 value. Its language version comes from the owned model; its Dutch
flag comes from the decoded resource identifier's byte-equivalent `nl_`
substring. Actual `ru_RU-lk-text.lite.lex` does not enable that flag.

The wrapper-selection case predicate belongs to the word resource's **lexical**
alphabet, whereas2324E0 table preparation uses the frontend's acoustic alphabet.
These are distinct inputs. The actual Russian lexical AL has no stored case
maps: its native Unicode fallback must establish the predicate. An acoustic
stored-map pointer alone cannot justify selecting292 for a lexical source.

The actual2B7.V58 is05A3F0, returning0. Consequently the bound dictionary's
233FC0 auxiliary24-byte continuation query emits no records. The exposed empty
branch is proved by that exact binding; nonempty28C/provider24-byte sources are
not implemented or silently treated as empty.

Restart233F70 transports only case-state bits58..62 when incoming header05.bit2
is set. `LegacyNativeCaseWordMetadata` implements234970 over the same owned
terminal source: it writes language category8, preserves word identity4 and
all other terminal fields, and keeps the actual containerC4 ignore-case input
explicit. TCM0 leaves the inner metadata unchanged. TCM200 applies the category
table or the native start-comma/sentence-punctuation categories7/8.

## Independent verification

Portable controls use four checked stored alphabets and10,248 independent
state/category/source-flag records. They validate11,483 ordered emissions,
complete preserved bytes, costs and1,024 restart bit transports. They are
record-level controls, not handwriting accuracy evidence.

The actual Russian dictionary test obtains132 real root source records and
runs all28 TCM200 states. It emits5,742 variants and exercises3,719 paged calls;
capacity5 and512 produce identical streams. An independent Python consumer
checks every variant's label, flags, correction and same-owner context symbol,
plus512 actual terminal-category records. Additional owned-context checks cover
an anchored two-symbol suffix and Dutch special states. Managed and Native AOT
execute the same pipeline without loading a vendor DLL.

```powershell
python tools/LegacyCaseQuery.Probe/reference_controls.py preparation-controls.json query-controls.json
LegacyCaseQuery.Probe case-data.json publication-data.json query-controls.json AK.res LK.res engine-profile.json captures.json
python tools/LegacyCaseQuery.Probe/audit_actual.py preparation-controls.json captures.json
python tools/handwriting/audit_legacy_case_query.py static.json
```

The diagnostic backend uses already prepared dictionary maps. Their current
controller permissions, actual container90/C4/side-table settings and aggregate
request forwarding still need the full main-source owner. This does not claim
that the application now recognizes Russian, that mixed-script first results
have been corrected, or that main240's final word winner is complete.
