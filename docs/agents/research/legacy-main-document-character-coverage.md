# Actual WORD-minus-CHAR source scan

`LegacyFreshMainDocumentCharacterCoverage.Prepare(actualSourceCoverage)`
ports the ordinary01C740 calls between0248D0 and01FD40 in0283B0. It consumes
the completed typed source-coverage prefix, exact shared WORD/CHAR owners,
same raw document, original point counts and pending CHAR/WORD/TEXT context.
It accepts no labels, prepared source IDs, supplied success booleans or
invented gaps. It is not document acceptance.

Native0296F9..029788 visits each ordinary WORD source's48-byte choices in
reverse index order. Before every call it clears a temporary C21 vector
through V78/07F820.01C740 chooses the WORD's ordinary source vector at30/38
when nonempty, otherwise its original interned key20/28. It appends those
exact16-byte ranges through V88/07FAD0 ->07F830. This is a raw append into
the cleared temporary vector, not geometric union or normalization.

The helper then visits that WORD choice's24-byte CHAR links in forward
order. Each CHAR uses prior ranges30/38 when present, otherwise its original
interned key20/28. The current fresh raw producer initializes30/38 through
the native C21 constructor and excludes the cached known-text producer;
`PriorRangeCount` is zero. The owning scan still checks that count and
refuses an unavailable prior-range owner. It reads the actual interned
CHAR payload's original key, not a later glyph's equal-looking current
projection. Shared keys retain their first real producer and source node.

Every subtraction uses the existing C21.VD0/0812D0 port with the identical
original document's point-count table. Raw source object IDs and original
point endpoints are retained; sampled/current physical ordinals do not
index this metadata. Each result owns the initial ranges, exact CHAR input
sequence and final remainder together with the actual WORD/CHAR identities.

The0283B0 caller tests the remainder count after01C740. With multiple WORD
choices a nonempty remainder requires024A90 splitting, which this producer
explicitly refuses. A sole WORD choice follows the native no-split branch
even if ranges remain; the remainder stays visible in its result. Current
genuine tests cover two choices with empty remainders, not an inferred
positive test of the sole-choice branch. There is no source removal,
guessed separator or post-hoc concatenation to make coverage pass.

The probe runs four actual configured raw inputs: one duplicate-point
stroke, two ordered three-stroke groups, reversed group order and a rotated
stroke. Real recognition, source completion, shared WORD/CHAR publication,
TEXT registration and preceding line/range stages supply all inputs.
Eight reverse-order WORD calls perform20 forward CHAR subtractions and
produce eight empty remainders. The independent oracle reconstructs the
ordinary WORD union from actual positive CHAR keys, selects the same
preferred/original vectors, decodes each full16-byte interned key and runs
the independently tested linear C21 subtraction using original counts.
It also checks actual WORD48 bytes, one-row line storage and command prefix
remain unchanged. Twelve guards cover cancellation, intervening registration
and disposed owners; returned range arrays are defensive snapshots.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet build tools/LegacyMainDocumentCharacterCoverage.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/document-character-coverage-build
python tools/LegacyMainDocumentCharacterCoverage.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-character-coverage-build/bin/LegacyMainDocumentCharacterCoverage.Probe/release/LegacyMainDocumentCharacterCoverage.Probe.dll E:/BlurMask-build-artifacts-20261014/document-character-coverage-managed.json
dotnet publish tools/LegacyMainDocumentCharacterCoverage.Probe -c Release -r win-x64 -p:PublishAot=true --artifacts-path E:/BlurMask-build-artifacts-20261014/document-character-coverage-native-build -o E:/BlurMask-build-artifacts-20261014/document-character-coverage-aot
python tools/LegacyMainDocumentCharacterCoverage.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-character-coverage-aot/LegacyMainDocumentCharacterCoverage.Probe.exe E:/BlurMask-build-artifacts-20261014/document-character-coverage-native.json
python tools/handwriting/audit_main_document_character_coverage.py E:/BlurMask-build-artifacts-20261014/document-character-coverage-static.json
```

The optional static audit pins the reverse WORD loop, forward CHAR loop,
preferred-vector selections, C21 clear/append/subtract bindings and exact
nonempty-remainder split guard. Vendor binaries are not loaded or executed.
The new result retains the actual document/source state for01FD40,020000,
020920 and later native postprocessors; those stages and final2E7/C4D
publication are not claimed complete.
