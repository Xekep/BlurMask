# Detached imported-result source names

`LegacyImportedResultSourceNames.Capture(import)` copies the bounded fresh
Russian source-name table from a **completed successful** control import. It
uses the importer's private old/published source union; it never manufactures
an old result snapshot. `ReadWord` accepts only the exact copied WORD owned by
that import and reads its signed16 source ID at WORD120 offset `0x30`.

The result is detached immutable metadata. Reading a captured name does not
revisit the old glyph pool or run a dictionary/NN query. A new capture still
requires an unchanged completed import. Names are dictionary identifiers,
not decoded text, locale descriptions, or an accepted document result.

## Native producer and owner order

Text `136B40` calls TOP273.V30=`13BE80` import. If successful it calls
TOP273.V38=`13C1D0`, **before** clearing TOP+8/+10 and releasing the live source
at wrapper+18. Later detached glyph reads use V50=`13C920` and its glyph-local
default helper. Thus a future owning wrapper must use:

1. successful `import.Run()`;
2. `LegacyImportedResultSourceNames.Capture(import)`;
3. per-glyph/WORD emission, where requested.

The portable import retains a typed source policy for validation. This is an
explicit lifetime difference from native pointer detachment, not evidence of
another native post-detach live-document route.

`13C1D0` accepts a240 controller, or288 containing240 at+28. It returns a
no-op success if TOP+A0 already exists or TOP+10 is absent. The new API models
one copied table lifecycle, not that in-place cached-pointer operation.

The controller count22D8 comes from `20D8F0` through290.V20=`199BC0` to
28D.V20=`236820`: **selected child count + implicit source0**. V28=`236830`
returns implicit descriptor0 or the selected descriptor at ordinal-1;
290.V28=`1AF6C0` forwards it. `20D8F0` installs these descriptor pointers in
22E0. The registered28D initializer=`235F30` zeroes58; its AddSource service
`236020` increments that count once for each successfully inserted source.
`20C9D0` calls that service for the selected dictionary. For the actual fresh
single Russian dictionary N is **2**, not1.
240's separate service table2E7AF0 slot08=`227D80` resolves a normal descriptor
to its resource+10, and source-1 to controller2300 (associated source).
The proven fresh no-associated policy supplies NULL for that last source.

`13C1D0` visits ordinals1..N in two passes; the final ordinal is resolved as
source-1. Non-NULL names start at resource+14. The inherited133 reader
`1D2680` copies the56-byte wire name and `1D22E0` XOR-decodes it with
14/12/19/77 in the actualFFFFFFFF format. The existing pinned prefix loader
validates termination and the actual ASCII identifier. No glyph alphabet
substitution or guessed display name occurs.

The native allocation is `(N+1)*8 + sum(strlen(nonNullNames)) + N + 1`.
Only its pointer table is initially zeroed. A non-NULL name, including an
empty name, installs a pointer. Every visited ordinal consumes a payload NUL,
including a NULL source. The final extra allocation byte is unwritten. The
portable snapshot exposes semantic offsets (-1 for NULL) and **only initialized
payload bytes**, omitting that final byte and all fabricated native pointers.
Allocation failure occurs before TOP+A0/A8 are installed; cancellation and
portable copy failure similarly publish no table. Prepared kernel tests include
NULL, present-empty, embedded NUL, byte identity and both traversal passes.

## WORD lookup and scope

`1458F0` reads signed WORD+30. -1 selects table[N]; -2 returns NULL immediately
and skips its optional length output. Other IDs index the table. The new API
returns nullable copied bytes, so it does not claim that ABI length side effect.
It rejects indexes outside the owned table instead of performing native's
unchecked memory access. NULL is distinct from `byte[0]`. Prepared N=0 copy
controls test allocation arithmetic only: native1458F0 with A8=0 would instead
take the live-source fallback; actual public captures have N=2.

Portable limits are256 sources and8MiB aggregate initialized name payload.
These prevent unsafe native-sized allocations/32-bit length overflow; they are
not native format limits. The actual bound owner allows one pinned selected
Russian dictionary and no associated, external, or reused source events.

Eight actual global results retain631 acoustic evaluations and their actual
WORD source ID1; each reads `ru_RU-lk-text.lite.lex`, independently decoded
from the raw LK resource. These are source-derived fixtures, not recognition
accuracy measurements. Prepared512 full tables (including N=256), eight
cancellation prefixes, eight source-copy failure prefixes, reserve failures, ownership/copy guards and aggregate
limits are checked separately.

The live2DA.V18=`0283B0` route additionally needs frontend/source218, selected
input40 records, source keys, guide metrics and actual document ownership.
V38 names provide none of those. No accepted document transport, final text
acceptance, or complete Russian/English/math route is claimed here.

## Checks

`LegacyImportedSourceNames.Probe` takes AK, engine JSON, separation JSON, case
JSON, publication JSON, line30, line50, capture path, LK (in that order).
`audit_output.py LK capture.json` independently checks the raw resource name,
all table bytes/offsets/read results and actual source IDs.
`tools/handwriting/audit_imported_source_names.py report.json` checks71 exact
instructions, twelve table bindings, pinned PE hashes and the existing
no-associated/detach-order static dependencies. Vendor DLLs are never executed.
