# Immutable document source selection state

`LegacyFreshMainDocumentSelectedSources.Prepare(actualCharacterCoverage)`
continues the genuine pending document pipeline through the supported
01FD40/020000 branches. It retains the same raw document, full shared WORD
array, actual selected alternative0, CHAR tree, pending transaction and
updated source-line view. It creates an immutable state overlay for flags18
and selected choice index44. Earlier source payloads, UTF8 records, keys,
WORD48/CHAR24 vectors, commands and line snapshots remain unchanged.

The adapter gathers actual typed CHAR payloads from all WORD choices and
validates that their source-node objects account for every entry in the
actual interner's insertion list and AVL snapshot. Equal node numbers from
another owner are not accepted as identities. Missing/unreferenced payloads
refuse instead of being reconstructed. The native in-order01FD40 traversal
uses the actual left/right tree, a64-entry stack limit and checks that the
root/shape have not changed during preparation. The result includes every
node, including sources absent from the selected WORD.

01FD40's CHAR path is deliberately bounded. Native flags10600000 skip both
prior and raw-object reads. An unmasked bit29 source with selection ID-1
also skips; an actually used V128 prior-owner read refuses. Otherwise only
a one-range original key can reach the raw-object predicate. That predicate
literally tests U32[4]==U32[C], U16[2]==0, byte[1]==0, U16[A]==3 and
byte[9]==FF. It is not a generic whole-object or bounding-box test. If that
predicate actually reaches the C25 kind lookup, the producer refuses because
its owning raw-object kind service is not yet available. It does not assume
a false C25 result from a constructor or source label. All supported CHAR
branches preserve their previous metadata flags.

For the actual selected WORD,01FD40 recomputes bit01000000 from whether
every CHAR metadata flag intersects11004000, preserving all other WORD bits.
020000 then walks that selected WORD's CHAR links in their actual forward
order. Each write ORs08000000 into its source flags and copies the link's
raw signed marker to44. Repeated source nodes retain the last marker;
markers are not sorted, maximized or clamped. After the CHAR loop the WORD
source receives the same flag OR and its actual24-byte WORD-link ordinal
at44. Fresh payload40 remains the independently proven minus-one preference,
so the actual279 link supplies the selected WORD used for CHAR traversal.

The four real configured sources produce12 complete CHAR nodes and10
selected link writes. Two unselected source nodes remain in the complete
result. All four avoid the unsupported prior/C25 reads. The independent
oracle traverses the actual AVL snapshot, checks unsigned lexical key order,
replays the lazy guards and WORD metadata flag, and applies actual ordered
link writes to a separate copy of the original flags/indices. It also runs
the preceding independent raw-key/subtraction oracle over the retained
WORD48/line56/transaction inputs.256 explicitly prepared alias controls
exercise repeated-source last-write behavior, nonzero flag bits and raw
signed markers; they are not described as recognized sources. Twenty-three
guards include cancellation, intervening commands, controlled insertion of
a node with no payload, disposed owners and separate numeric output/input
storage. The controlled malformed trees are negative tests after inference.
An internal current-owner check rejects later tree/context mutations and
disposed owners before future consumers use the overlay; historical immutable
snapshots remain readable.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet build tools/LegacyMainDocumentSelectedSources.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/document-selected-sources-build
python tools/LegacyMainDocumentSelectedSources.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-selected-sources-build/bin/LegacyMainDocumentSelectedSources.Probe/release/LegacyMainDocumentSelectedSources.Probe.dll E:/BlurMask-build-artifacts-20261014/document-selected-sources-managed.json
dotnet publish tools/LegacyMainDocumentSelectedSources.Probe -c Release -r win-x64 -p:PublishAot=true --artifacts-path E:/BlurMask-build-artifacts-20261014/document-selected-sources-native-build -o E:/BlurMask-build-artifacts-20261014/document-selected-sources-aot
python tools/LegacyMainDocumentSelectedSources.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-selected-sources-aot/LegacyMainDocumentSelectedSources.Probe.exe E:/BlurMask-build-artifacts-20261014/document-selected-sources-native.json
python tools/handwriting/audit_main_document_selected_sources.py E:/BlurMask-build-artifacts-20261014/document-selected-sources-static.json
```

The native caller next invokes020920. Its0207A0/020860 callbacks build
0184E0 interval membership and01A520 exact-key indices before setting1F0.
Those index producers,023260, source transforms, external-span processing
and final C4D/2E7 application are separate unfinished stages. A selected flag
does not mean an accepted document, calibrated confidence or a live font-size
sample. No expected text is read and no vendor binary is executed.
