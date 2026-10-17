# Existing text-source glyph reuse

`LegacyPreparedMainTextGlyphReuse` restores the bounded ordinary021760 kernel
over actual same-TOP virtual50 glyph emission and an owner-local UTF8 string
pool. Existing source-node records remain explicitly prepared until the
024F30/01B4E0 source-node interner and024D40 metadata caller are connected.
This is not a complete string wrapper or an accepted OCR result.

The public `Apply(snapshot, selectionRows4, sourceKey, selectedInputIndex)`
resolves each selected274 reference and calls the typed
`LegacyFreshMainGlyphUtf8Emission`, which checks the identical current TOP
reference and all40 glyph bytes. Arbitrary emitted text callbacks exist only
in the friend probe's explicitly prepared numeric controls. No transcription,
corrected symbol aliases or caller-supplied recognition labels enter production.

## Native algorithm

1. Capture the existing record count from source payload58 into a fixed signed
   local (`R12`,0217AC). The search bound does not grow as rows are appended.
2. Resolve the next selected glyph and run actual273.virtual50. There is no
   HeaderE filter, empty removal or2639/263A smiley replacement in021760.
3. Intern the complete emitted byte interval in this same wrapper string pool.
   Empty strings are valid interned strings.
4. If the intern operation found an existing string, scan only the initial
   record prefix for the same interned-string identity. A hit reuses the old
   record unchanged, including all flags and other scalar metadata.
5. Otherwise, append a fully zeroed40-byte row and increment the count before
   calling021000. Record an input-selected marker as its old or new row index.
6.021000 writes byte count32 and low-ten glyph flags8, interns pointer16,
   then performs its existing character/script/literal classification. The
   marker-token flag100000 and aggregate source flags are added only after
   successful publication. No earlier row is rewritten.
7. On a failed prefix, keep appended rows and earlier scalar/string writes.
   Cleanup writes the current marker remapping, including the newly reserved
   row index when its subsequent publication failed. An input marker never
   visited maps to minus one.

Two equal new strings in the same invocation can therefore produce two rows:
the second lookup still scans only the original prefix. On a later invocation,
both can reuse the first old matching row. This is native behavior, not an
opportunity to deduplicate more aggressively.

## The apparent fifth-argument trap

The decompiler calls a stack byte `param_5`. It must not become a caller
force-append option. ASM218F4 takes the address of that stack slot and passes it
as the fifth argument to the string interner. Engine0315E0 forwards it to
030E60, which overwrites the byte with zero for an existing entry or one for a
new entry. The branch at21910 reads this **OUT-new** byte. The implementation
derives the same decision from this serialized owner's actual string-pool
growth. There is no public force flag.

## Portable storage and failures

`LegacyPreparedTextGlyphReuseRecord` retains every scalar byte of a prepared
40-byte record. Native pointer16 is represented separately by an
`LegacyNativeInternedUtf8` owned by this exact string pool; its serialized slot
is zero. Stable managed identities are never serialized as native addresses.
Foreign string owners are rejected. Copies do not expose mutable record bytes.

The implementation preserves the native append/publication write prefix,
including a row with only byte count and low flags, or a row with an already
interned string but no classification. Cancellation retains this same prefix.
Portable retry after a failed or interrupted invocation is an explicit refusal;
native allocator state, pointer capacity growth, malformed legacy codecs and
allocator-failure replay are not simulated. Explicit budgets bound selection,
record count and bytes. Existing rows and input-source ownership must remain
stable during an invocation; concurrent mutation of the supplied string pool
is not supported.

## Verification

The independent Python generator produces1024 controls covering old-only
identity lookup, preinterned strings without old records, unchanged old40-byte
metadata, repeated new strings, empty and smiley strings, marker mapping,
aggregate flags and five failure boundaries. Managed and Windows NativeAOT
agree with every expected scalar row and identity:4286 appended rows,847 reused
rows and292 failed prefixes. Additional guards verify foreign string ownership,
failed-prefix retry, cancellation before work and interruption after an actual
append/string write.

The production entry point is also run12 times over four genuine pool records
from an actual raw source. A separate raw-AK oracle reconstructs emission,
string identity, all40 scalar bytes, aggregate flags and markers. Those calls
still start from an explicitly prepared source-node state; they are not a
claim that024F30 already selected or interned that node.

The static auditor reads pinned Text and Engine PE bytes only. It checks the
fixed-prefix bound, actual OUT-new parameter overwrite, the complete row-zero
and count-before021000 sequence,021000 scalar/intern/classification order,
aggregate success gate and marker failure cleanup. No vendor binary is loaded
or executed, and no GUI is launched.
