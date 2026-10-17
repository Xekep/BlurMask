# Imported default glyph emission completion

`LegacyFreshMainImportedWordEmission.Complete(import, normalization, payloads)`
completes each actual ordered imported glyph through the existing pinned default
Unicode/circled helper, then concatenates its bytes into the WORD.
`LegacyImportedGlyphUtf8Completion.Complete` exposes the same bounded operation
for one owned reference. The input must be a successful same-owner control import;
result lease/list/pool validation runs before and after. Raw byte results remain
unchanged and are retained as explicit provenance.

This restores a bounded **default valid-Unicode glyph-emitter component**. It
does not perform whole-WORD normalization, Document string classification,
accepted result publication or handwriting winner selection. No text getter or
framework NFC operation is added to the runtime. `Changed` belongs to each glyph;
`AnyGlyphChanged` is only a WORD diagnostic.

## Exact call boundary

Post-import `05BAC0` supplies dynamic TOP18 as the primary alphabet to TOP.V50.
The previously pinned ownership proof remains applicable. `13C920` saves its
single glyph at `13C94F`, expands that glyph at `13C9B3`, and calls274.V68 at
`13C9E8`. `13A0B0` finishes **all expanded tokens of that glyph** at
`13A5D9`/`13A5DC`, then calls the default helper once at `13A616`.
It reserves/writes the storage-only NUL afterward (`13A629`, `13A63C`).
Neither a per-token helper call nor a whole-WORD helper call follows this route.
TOP.V58 (`13CA40`) collects tokens and does not prove a whole-WORD byte helper.

Default Engine global68 comes from252188 ->1DD740 ->1DD7A0; its E0 method is
0491A0. Bootstrap0324D0 installs it. Default global70 comes from252190 ->1DD990
->1DD968; method0 is04BAF0. Its codec5 entry is20FE10, tag5, initializer094500.
These bindings and the no-custom-service scope justify reusing the existing
validated normalization and circled-payload model owners.

0491A0 builds canonical scratch, recognizes F0000/F0001 circled intervals and
writes the scratch back **only after a successful circled replacement**. Thus
ordinary decomposed text/BOM/noncharacters remain byte-identical when no
replacement succeeds. A replacement can normalize other content within that
same glyph, while content in neighboring glyphs remains unaffected. A marker
pair split across glyphs must not be combined. The runtime calls only
`LegacyNativeCircledUtf8Writeback.Complete`, preserving its exact default policy,
portable budgets and existing native037E/0387 exceptions.

Malformed UTF8, including the genuine one-byte native unit C2, is refused before
helper invocation. Its raw bytes remain available through the separate raw API;
they are never repaired or dropped. Custom/extended codecs, nonempty alternate
ownership and accepted source publication remain outside this scope. The empty
alternate-table refusal is inherited from the raw producer.

## Ownership, bounds and checks

The new WORD result retains its actual raw WORD/import, normalization owner,
payload owner and each raw glyph object. Completed bytes are private owned
copies. No original/imported pool or WORD is mutated, and NN is not repeated.
Projection checks 65,536 aggregate references before helper calls and 8 MiB of
aggregate completed bytes before output writes/retention. Existing per-glyph
scratch byte/scalar/work limits propagate as refusals. The caps are portable
resource bounds, not native model thresholds.

`LegacyImportedGlyphCompletion.Probe` covers eight actual global imports and
320 separate prepared raw-AK/reserved/run records. Actual ordinary results
remain unchanged; no selected positive circled winner is claimed. A further
276 prepared byte-owner groups cover all native payload literals and ASCII
circle arithmetic, successful single-glyph pairs, split-glyph pairs, neighboring
NFD/BOM/noncharacter discriminators, unmatched/unknown markers and embedded NUL.
Prepared positive groups are not recognizer outputs. The independent oracle
parses raw AK and derives import2A bytes, then checks per-glyph completion and
WORD concatenation. It uses official stable Unicode circle decompositions and
the pinned literal payload data. Its selected joining corpus avoids native
037E/0387 and gap-CCC deviations, which remain covered by the unchanged full
canonical/circled kernel probes.

Additional full-composition controls reach exactly 8 MiB across multiple WORDs,
reject the next WORD, allow 65,536 zero-token references and reject 65,537.
They also verify strict C2 refusal, cancellation, stale original source,
incomplete import and copied outputs. These large repeated outputs are captured
as byte counts/hashes rather than megabytes of duplicate glyphs.
A further completion-specific control uses repeated canonical-excluded0958 plus
a circled-A interval in the same glyph. Successful helper writeback expands each
3,081-byte raw glyph to6,147 bytes. Forty-two WORDs of32 references fit; the43rd
crosses the completed-output cap while its total raw input remains below8 MiB.
The independent oracle checks the expansion, complete output hash and both
inequalities, distinguishing the completed-byte guard from the raw-byte guard.

Probe arguments are AK, engine profile, separation profile, case data,
publication data, line30, line50, capture, LK, normalization data, circled data.
Run `audit_output.py AK capture circled-data.json`. Static audit
`tools/handwriting/audit_imported_glyph_completion.py report.json` checks26 exact
instructions,9 descriptor/codec links,5 PE intervals and the existing bounded
circled-search/writeback audit. It only reads pinned binaries. Managed and
Native AOT captures must match; old raw-byte/import goldens remain unchanged.
No vendor DLL execution, GUI, CI or default recognizer change is involved.
