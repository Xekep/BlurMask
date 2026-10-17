# Owned ordinary raw/current views and identity export

`LegacyFreshRawInkCurrentView.Prepare(actualRawDocument)` constructs the
ordinary kind0 native-class lineage for this library's immutable raw-stroke
root. It retains the exact `LegacyOwnedRawInkDocument`, original source IDs,
float32 XY values, concrete C49 curve owners, and their full current-view
links. This is an additive owner. Existing document selection contexts,
original-ID registration, and counters are unchanged.

`ExportIdentityTransforms(actualRanges)` runs the existing source-range
descriptor producer against those original objects, then checks native
06B7F0 current-view matching for every fragment. Only covered fragments can
resolve identity. Native06F080's identity-exclusion branch consequently emits
zero nonidentity transform records, including for nonempty source ranges.
The returned ranges, fragments, matched view indices and 48-byte matrices are
defensive copies. No caller-supplied no-C25/no-C4C flags are accepted.

## Static factory and channel proof

The pinned `MyScriptInk.dll` SHA256 is
`b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a`.
The auditor reads PE bytes without loading the DLL.

- C32 is an interface. C1C primary table E6FD0 slot18 is 0988F0,
  which returns the C32 method table E7010 for interface C32. The mutable
  builder C1D similarly supplies C32 through 099190.
- C47 default-channel records at E52A0/E52C0 bind the actual strings X/Y
  to channel IDs1/2 and representation10420. 070EE0 selects these records;
  no recognized symbol or manual geometry category participates.
- C1C constructor0978D0 requires the real X/Y channels and compatible units,
  copies them, and calls098080. Its first-two-channel float32 branch writes
  the complete 36-byte raw layout as uint32 values
  `[2,0,0,8,10420,4,0,8,10420]` (10420 is hexadecimal).
  The Y descriptor starts with4, followed by0; those lanes must not be
  swapped. C48 reader096D20 computes address
  `storage+16 + field4 + field8*count + fieldC*point`, then loads10420 with
  MOVSS/CVTPS2PD. Thus the data are interleaved little-endian float32 XY,
  with stride8 and X/Y byte offsets0/4.
- C1E service E6980 slot0 is090CF0. Kind0 allocates C49 and calls08DDB0,
  retaining the actual format and allocating its raw buffer through08DD30.
  Kind4 takes a separate C25 path. This factory models kind0 only.
- Successful full raw append065940 writes both the original raw entry and
  a current-view link, including original source ID and zero view flags.
  Full scoped selection06D2C0 clones the ordinary raw curve via C1E.V18
  (092430/091700); a full self-view retains that cloned C49. Partial views
  or prior/external concrete objects are not silently admitted by Prepare.
- C49 primary table E65B0 slot28 is08E000: a default last endpoint minus-one
  is resolved from the actual raw point count. Slot30 is0222C0, exactly
  `xor eax,eax; ret`, so the concrete C49 has no affine pointer. C44
  tableE3FE0 slot0 is04D580, writing doubles `[1,0,0,0,1,0]`.

The portable owner transports identities as object references and source
indices. It exports every assigned channel-layout and XY byte. Its 8-byte
current-view scalar tail is original source ID plus zero flags. Native
pointer lanes, uninitialized descriptor padding, allocation headers and
unused C1C timing properties are not fabricated or serialized.

## Coverage is required before identity

06B7F0 first performs rounded endpoint equality, then float containment with
the native first/last-point open/closed rules. ASM06B9B7..06B9CB uses
MULSS/ADDSS/CVTTSS2SI: quantization is float32 multiply by200, float32 add.5,
then truncation. A double calculation differs. Controls include256 cases
where request fraction bits3B23D70A and view fraction.005 match natively but
fail the double approximation.

The non-null raw-base branch with no matching current view and null object
output returns at06BC10 without writing the matrix. Prepare therefore
requires complete original-owner full-view coverage, and the export checks
each requested fragment before returning matrices. The shared checked
matching kernel refuses clipped or nonmatching prepared views. The missing
view path is not assigned identity. Nonfinite and out-of-bounds descriptor
inputs are outside this bounded port and are refused.

## Verification and remaining boundaries

`tools/LegacyRawInkCurrentView.Probe` consumes four genuine configured
recognition-owned raw documents: duplicate-point M, two ordinary words,
reversed physical input ordering, and rotated M. It never reads expected
labels. Managed and NativeAOT check14 actual C49 objects and28 actual
selected fragments. The independent oracle checks full36-byte channel
headers, all XY bytes, 8-byte current-view scalar tails and all48-byte
matrices. There are4096 separately labeled prepared coverage controls,
including rounded-equality, clipped, nonmatching and sentinel cases,
256 float32 discriminators, defensive copies and16 guards.

Run from the repository root (all build outputs and TEMP/TMP on E):

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet build tools/LegacyRawInkCurrentView.Probe/LegacyRawInkCurrentView.Probe.csproj -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/line-raw-current-view-build
python tools/LegacyRawInkCurrentView.Probe/run.py E:/BlurMask-build-artifacts-20261014/line-raw-current-view-build/bin/LegacyRawInkCurrentView.Probe/release/LegacyRawInkCurrentView.Probe.dll E:/BlurMask-build-artifacts-20261014/raw-ink-current-view-managed.json
dotnet publish tools/LegacyRawInkCurrentView.Probe/LegacyRawInkCurrentView.Probe.csproj -c Release -r win-x64 -p:PublishAot=true --artifacts-path E:/BlurMask-build-artifacts-20261014/line-raw-current-view-native-build -o E:/BlurMask-build-artifacts-20261014/raw-ink-current-view-aot
python tools/LegacyRawInkCurrentView.Probe/run.py E:/BlurMask-build-artifacts-20261014/raw-ink-current-view-aot/LegacyRawInkCurrentView.Probe.exe E:/BlurMask-build-artifacts-20261014/raw-ink-current-view-native.json
python tools/handwriting/audit_raw_ink_current_view.py E:/BlurMask-build-artifacts-20261014/raw-ink-current-view-static.json
```

This factory adds an executable lineage to the **library-owned XY root**;
it does not infer the same state for an existing SDK page. Page observers,
history, reused C20 views, C25/C4C objects, and arbitrary channel models stay
outside scope. Native source tag/C51 registration and its Next counter are
separate from this library's original-ID policy: Document addItems005380
can register INPUT via Ink06BE30, and layout can register LAYOUT_STROKES.
No empty registry, zero counter, or one-tag-per-physical assumption follows
from this owner.

Actual037E80 caller range/result binding, DAF030FB0 input32 rows,026EF0,
prior/cache replacement and final2E7/BE2 publication remain separate native
joins. This is not full0283B0 or accepted-document completion.
