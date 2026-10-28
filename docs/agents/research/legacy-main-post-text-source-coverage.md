# Fresh post-TEXT source coverage

`LegacyFreshMainDocumentSourceCoverage.Prepare(actualLineConsistency)`
continues the actual single-line pending document state after025E40. Its
input retains the registered TEXT command, shared WORD/CHAR tree, actual
configured alternatives and updated one-row source-line snapshot. It
preserves the earlier two-row021AB0 snapshot and all pending command
identities. This is an owning postprocessing prefix, not a native source2DA
pointer, an applied selection transaction or accepted OCR text.

## Live source metadata, not constructor defaults

The conditional026090 call cannot be skipped merely because01C010 initially
zeros source2DA.22A.0283B0 overwrites that field from a live source facet.
The actual chain is:

* Frontend222 table2DE610.V250 is1530F0, returning frontend60, the current
  source219.0284F9..02850F calls it and saves the result at RBP38.
* Text initializer002530 installs descriptor2DF150's service table2DF0A0
  in context18.facet38. Its V80 is169740, which returns the supplied
  source219's secondary pointer at120.
* At028720..02873A the caller supplies that saved source, reads secondary14
  and writes source2DA.22A.
* The scoped fresh input has no prior source, user guidelines, external
  property events or caller-set property bundle. NEW2B3's constructor00C860
  zeros word10. Its12D0070 getter00D020 reads byte10;150D00 forwards that
  property through150800 to source218's setter1690D0 at byte80.168271/168278
  then copies the value into secondary14. Configured alternative count18
  is a separate property and does not enable this field.

Thus the actual typed fresh source yields22A=false through the live getter,
making026090's ordered caller guard false even if input-result count or a
retained cache could otherwise qualify. The adapter does not guess that the
secondary68/70 cache is empty. Its ownership and contents remain separate,
unused in this supported branch. A source with prior or enabled metadata
would need a different owner and the actual026090 implementation.

## Selected alternative and exact range arrays

Native0283B0 clears R15 at028771 and writes the complete zero QWORD to B8 at
0287B7, including selected document-alternative indexBC. This establishes
index0 before the postprocessors;01C010's earlier BC=-1 is not reused.
Every alternative in the current sealed shared-WORD result has exactly one
native24-byte WORD link.027670 reads that count, subtracts one, and returns
before any glyph/gap logic when it is one. This closes only that actual
single-WORD-per-alternative path, not generic multi-WORD line extension.

At0248D0 the native traversal unions each WORD source node's original
range key at node38/count40 through C21.VB0. It does not substitute the
accumulated ordinary CHAR ranges at node48. The current actual shared275
result contains one interned WORD source node reused by its two genuine
alternatives, so the producer can retain its exact tree identity and key.

C21.VE8 is081700. It compares counts and both raw QWORDs of every ordered
16-byte range. It is exact equality, not normalized geometric containment.
The existing native union kernel can canonicalize separate complete objects
into the same ordered root TEXT key. The adapter computes that union from
the real WORD keys and requires exact equality with the actual NULL05AFA0
TEXT key. An unequal result refuses before claiming the238B0 fallback,
whose range subtraction and additional source construction are not ported
here. It does not silently accept a geometrically covering but byte-different
range vector.

The probe executes four genuine raw configured sources through NN, WORD
publication, shared source completion, TEXT registration and025E40. All four
actual WORD unions equal their TEXT keys. Managed and NativeAOT are checked
by an independent C21 replay and complete56-byte/command-prefix preservation.
Twelve guards cover cancellation, intervening commands and disposed owners.
An additional256 explicitly prepared range pairs show why containment cannot
replace081700: an FF-fraction alias is contained after search normalization,
but the raw arrays are unequal. They are not described as inferred sources.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet build tools/LegacyMainDocumentSourceCoverage.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/document-source-coverage-build
python tools/LegacyMainDocumentSourceCoverage.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-source-coverage-build/bin/LegacyMainDocumentSourceCoverage.Probe/release/LegacyMainDocumentSourceCoverage.Probe.dll E:/BlurMask-build-artifacts-20261014/document-source-coverage-managed.json
dotnet publish tools/LegacyMainDocumentSourceCoverage.Probe -c Release -r win-x64 -p:PublishAot=true --artifacts-path E:/BlurMask-build-artifacts-20261014/document-source-coverage-native-build -o E:/BlurMask-build-artifacts-20261014/document-source-coverage-aot
python tools/LegacyMainDocumentSourceCoverage.Probe/run.py E:/BlurMask-build-artifacts-20261014/document-source-coverage-aot/LegacyMainDocumentSourceCoverage.Probe.exe E:/BlurMask-build-artifacts-20261014/document-source-coverage-native.json
python tools/handwriting/audit_main_document_source_coverage.py E:/BlurMask-build-artifacts-20261014/document-source-coverage-static.json
```

## Remaining native order and ownership

The following steps are not completed by this producer:

| Native call | Next dependency |
| --- | --- |
|029746 ->01C740|For every WORD choice, copy its preferred ordinary/original source ranges, subtract each actual CHAR's preferred ranges, and run024A90 splitting when a remainder exists with more than one WORD choice. Existing raw-document C21 operations and actual keys are available; the owning scan is next. |
|0297D7 ->01FD40|Traverse the real source tree, inspect original object kinds and update WORD/CHAR flags. Requires an updated document-source state, not mutation of previous immutable snapshots. |
|0297DF ->020000|Apply actual selected alternative0's WORD/CHAR08000000 flags and selected marker44. No accepted-text string may be inferred from this flag alone. |
|0297EA ->020920|Run0207A0/020860 over ordinary CHAR/WORD nodes before setting1F0. The current UTF8 choice vectors are inputs, not proof these callbacks succeeded. |
|029801 ->023260|Process100000-marked choices and possible special inserted-source records. Actual flags and ownership must decide each branch. |
|02985C ->037E80|Construct actual source transforms throughDAF; selected point IDs, source-frame heights and SDK page IDs are different domains. |
|029876 ->026EF0|Consume retained external-span input; absent-input provenance must be proved, not supplied as a null convenience. |
|02959E/029954|Prior-result merging, C4D transaction replacement and final2E7 publication remain separate; pending commands are not application success. |

The immutable result chain retains the actual raw document, selection context,
source tree, selected alternatives and changed source-line storage for those
future consumers. No native addresses, SDK page identities, spaces, UI defaults
or final acceptance confidence are fabricated. Static audit reads pinned Text
and Ink PE bytes only; vendor binaries are never loaded or executed.
