# Imported WORD raw pre-canonical bytes

`LegacyFreshMainImportedWordBytes.Project(successfulImport)` follows the actual
post-import alphabet selection. `LegacyImportedControlGlyphBytes.Read` provides
the same operation for one owned glyph reference. Both require a completed
`LegacyFreshMainGlobalControlImport`, validate its original result lease/list/pool
before and after projection, and retain the copied imported owner. No supplied
success flag, replacement alphabet or expected transcription is accepted.

The result is **raw pre-canonical bytes**, not completed V50, accepted text or
Document publication. There is no `Text` getter, normalization or Unicode repair.
Native `0491A0` circled-marker processing and the codec completion after the
`13A0B0` concatenation loop remain outside this result type. Valid circled-marker
bytes are preserved, rather than silently skipped while claiming completed V50.
The storage NUL after the native active byte interval is not part of the result.

## Actual primary/control ownership

`05BAC0` (called by `05CD30`'s charCandidate emission) selects the primary alphabet
using the result-wrapper byte `20`. At `05BAE0`/`05BAE4`, nonzero selects
`wrapper10->TOP18`, the dynamic control `2A`. Zero selects `wrapper18->source60`,
the live original primary alphabet (`05BAEA`/`05BAEE`). The selected pointer is
passed to TOP.V50 at `05BBCE`. This differs from live Document `0213A0`.

`136B40` sets wrapper20 to one before TOP.V30 import, invokes TOP.V38 after a
successful import, then clears TOP8/10 and releases/clears the source wrapper.
Failure retains the prefix and is not accepted output. The portable imported
owner represents a separate copied lifecycle; it does not fabricate native
pointer detach writes or claim V38's source-name metadata is fully implemented.

Existing `LegacyNativeGlyphUtf8Tokens.Expand` supplies exact `139C90` token/run
semantics. The new emitter reads primary bytes directly from the same private
imported2A, and uses that owner's lookup for kind3 LF/space/NBSP runs. It never
re-encodes control bytes through an acoustic alphabet string. The actual fresh
alternate owner is **non-null and empty**: kind4 is refused, matching the native
empty-table failure instead of using the null-alternate primary fallback.

The private interner permits valid UTF8 plus the pinned native unit literal's
one-byte `C2` interval. That genuine token is preserved and reported with
`ContainsNativeUnitLiteral=true`, `IsValidUtf8=false`; it is not repaired to
U+00A7. Other malformed resource/codec stages are refused. Glyph and WORD byte
arrays and records are copied on output. Portable bounds are 1 MiB per glyph,
8 MiB per WORD, **8 MiB aggregate output bytes across the entire projection**,
and 1,048,576 expanded tokens per projection. The checked aggregate is tested
before retaining a copied glyph or writing it into a WORD. These are portable
caps, rather than claims about native allocation capacity.
The public entry checks the aggregate reference count before copying any WORD
reference vectors. The interval core independently rechecks that same cap.
An additional aggregate cap of 65,536 references is checked before any glyph
projection. It bounds retained zero-token glyph objects as well as nonempty
ones; its scale matches the existing pool-record budget, not a native WORD limit.

## Verification and limits

`LegacyImportedWordBytes.Probe` constructs eight actual owning global pipelines,
imports their real completed pool/WORDs, and projects all ordered references.
It checks scalar/WORD agreement, unchanged NN counts and imported storage,
incomplete imports, stale source mutation, invalid references, cancellation and
copied buffers. A separate 320-record prepared set covers every raw acoustic
label, reserved labels, native C2, raw circled markers and run/alternate cases.
Those controls do not assert selected nonempty protocol winners.
A separate prepared boundary control inserts a genuine 4,096-byte valid symbol
into a fresh2A and retains its kind2 record. The shared interval-composition
kernel produces exactly 8 MiB across 64 WORDs with 32 references each, then
refuses the 65th WORD. It checks complete repeated bytes and their hash, rather
than testing only the cap predicate. This is a prepared byte-owner control, not
a claimed fresh recognizer result or native winner.
The prepared zero-count run control also passes 65,536 repeated owned references
through the full composition core with zero output bytes, then verifies that
65,537 references fail before glyph allocation. The capture contains counts,
not tens of thousands of repeated glyph payloads.

The independent Python audit parses the pinned raw AK, independently derives
imported IDs/full40-byte records and run expansion, then compares every glyph
and concatenated WORD byte interval, validity flag and unit provenance. It reads
no expected text. Managed and Native AOT captures must match completely. The
old import probe is rerun separately with its existing goldens unchanged.

Probe arguments are AK, engine profile, separation profile, case data,
publication data, line30, line50, capture JSON, LK. Run
`tools/LegacyImportedWordBytes.Probe/audit_output.py AK capture.json` and
`tools/handwriting/audit_imported_word_bytes.py report.json`. The latter checks
the pinned Text PE, eighteen exact import/caller instructions and three273 table
bindings without loading or executing the DLL. Outputs and TEMP/TMP belong on
the task's E drive. No UI/default recognizer or recognition-quality claim changes.
