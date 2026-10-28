# Retained source range UTF8

`LegacyFreshMainSourceRangeUtf8.Prepare(counterHandoff)` formats the actual
retained CHAR insertion list, WORD key and TEXT key from the same selected-input
seed, raw document and pending transaction. It retains their typed payload
owners. It neither builds DOM nodes nor executes the pending transaction.

Ink C21 descriptor `1800E5C90` points to service table `1800E5AC0`;
service `+180` is `083DF0`. Its `080430` check compares raw unsigned positions,
including the FF fraction, and returns true immediately when a previous end
has mode4. An overlap or whole-consecutive-object join makes a copy necessary.
The copy keeps the first range, skips individual later mode4 ranges, and
invokes literal `07FAF0` for each other row. The port reuses the existing
document-bound native range operations one row at a time. It does not replace
this behavior with sorting or a generic interval union.

`07F690` writes `source:point`, then `$` for FF, or the three decimal digits
of `fraction*5` with trailing zeros removed. Range delimiters are `[` for
first mode3, otherwise `]`; `]` for last mode5, otherwise `[`. Disjoint ranges
are separated by uppercase `U`. Source is a nonnegative signed32 field and
point is unsigned16. Source coordinates retain original raw-object IDs;
the C51/C4D registry, INPUT tag and selection IDs remain separate owners.

The actual C79 writer `00DB00` computes F32 remainder times200 plus0.5,
then carries a rounded200 into the next point and writes zero or terminalFF.
The actual source projection uses that writer. The owned formatter also
checks the retained document's object and point counts. Prepared fractions
200..254 and negative signed source fields are refused: native `07F690`
would index outside its decimal digit table, so no portable replacement
number or clamping is invented. Prepared quantization controls cover every
fraction0..199 and the terminal carry; retained-source tests exercise the
actual inference-produced keys without supplied labels.

The C21 UTF8 bytes and the Text caller bytes are separate snapshots.
`0309C0`/`030EF0` test source flag400000 and change only the first byte to
`{` if it was `[`, otherwise `}`. They leave all later delimiters intact.
TEXT has no such CHAR/WORD caller rewrite. No .NET culture-specific decimal
or GUID formatting is used.
The caller rewrite is reached only after formatting succeeds. An empty
prepared string with bit400000 is refused rather than inventing an owned
first byte for the native unchecked write; no retained real source in these
fixtures requires that branch.

The internal prepared formatter models the copy/string allocation boundaries,
1.5 growth and retained output prefix. A failure can retain a trailing `U`
after separator success and subsequent range-growth failure. Final virtual
string-service failure is a separate explicit prepared control; its concrete
allocator/environment cause is not claimed as an actual inference fixture.
Actual portable formatting owns its output storage and succeeds or throws.
Unsupported numeric-domain inputs are checked before output publication.
Prepared allocation controls use fresh output capacity0. Native031880
reuses a scratch vector across source, origin and choice calls; that full
DOM scratch lifetime/order is not part of this source-string snapshot stage.

Probe: `tools/LegacyMainSourceRangeUtf8.Probe`; static audit:
`tools/handwriting/audit_main_source_range_utf8.py`. The independent Python
oracle replays raw16-byte keys with linear range normalization and verifies
all UTF8 bytes and failure prefixes. Four real inference paths include
reversed physical ordering and rotation, retain all source payloads and
leave the pending command sequence and selected registry bytes unchanged.
DOM031880, BE2 transport and document acceptance remain subsequent stages.
