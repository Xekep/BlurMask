# Owning fresh source-line publication

`LegacyFreshMainSourceLinePublication.PublishFresh(actualWordChoices)` joins
the actual configured shared-WORD publication to native `021AB0`. The input
retains one fresh main source, its sampler descriptor, current guides and the
two real TOP alternatives. It accepts no prepared line heights, baselines,
source IDs, rotation values or appended-line counters. This is a scalar/source
publication stage before document acceptance.

The source is frontend222.V250=`1530F0` (frontend+60), interpreted through
interface218. V08 counts88-byte input descriptors, V10 retrieves a descriptor,
V40 retrieves its40-byte guide, and V100=`1692B0` publishes the line geometry.
The same actual sampler retains the descriptor and its
`Preparation.Input.Input.Input` retains the24B/main24A guide owner after
the owned720 path. Later24D/29B/265 preparation uses that same guide lineage.
The scope requires ordinary descriptor0, assigned line0 and one present guide.
No first-child6120 frame is substituted for the main guide.

The global source rotation is proven through the fresh property lifecycle.
It is unsafe to infer zero from the secondary constructor alone: actual2D0
`081E90` can rotate the points and write secondary+A8. However, fresh `00C860`
zeros the qword at+A4, including2B3 byteA9. Property `12D000C` at274E80 uses
`00E020` to read that byte. `150D00` forwards it through `150800` to the218
property at2DF280; setter `169090` writes source219+B4. In `1680D0`, a zeroB4
skips the source+40.V10 invocation of2D0 entirely. Actual fresh/no-property/
no-prior ingress retains the zero initialized by `16A130` at secondary+A8.
The later configured count18 property does not enable that input-stage angle
property. Configured/external angle overrides are outside this owner.

V100 uses guide8 slope, originalC/10 baseline/height, and corrected14/18 when
corrected height is strictly positive. Current guide0/4 belongs to the
deskewed classifier domain. Those heights can differ on actual raw input.
The existing `LegacyNativeSourceLinePublication` scalar kernel preserves
native float promotion, angle, origins and corrected-guide selection. The new
owner reads all fields from the actual source, including the original
baseline separately stored in each output56-byte row at30. The row's first
eight bytes are zero, and a written row's owner tag at28 is signed minus-one.

Append count and write offset are separate. Fresh2DA constructor `01C010`
setsF8/count100/cap104 to zero. For each outer TOP alternative, `0283B0`
resets wrapper68 to zero, traverses its source families and invokes `021AB0`.
The latter appends `max(active sourceLineId+1)` zero-initialized rows toF8 but
writes a present guide at `wrapper68+i`. Thus this actual one-source/two-
alternative scope appends twice while writing index0 twice; the second
allocated row remains zero. The owner returns both per-call snapshots and
the final two-row storage. It does not duplicate the prepared metric into
that zero tail or collapse the two native counters.

The probe runs four actual raw configured2 owners, including rotated raw
coordinates, through source completion and shared WORD publication. Eight
021AB0 calls and all56-byte records are checked independently from captured
raw88-byte descriptor and40-byte guide bytes. Two actual cases distinguish
current height from original pre-deskew height. The oracle uses no expected
labels or stroke bounding height. Every byte except the platform `atan2`
double is exact; the latter has an explicit narrow floating comparison.
Malformed ownership, cancellation and disposed upstream ownership are
refused. Returned descriptor, guide and storage snapshots are defensive.

The result is neither an accepted Document line nor a live font-size sample.
TEXT selection, document postprocessors, the actual recognized-character to
source-line association and final2DB/SDK publication remain separate joins.
The source guide is not renamed as an independently fitted character box.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet build tools/LegacyMainSourceLines.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/source-line-owned-build
python tools/LegacyMainSourceLines.Probe/run.py E:/BlurMask-build-artifacts-20261014/source-line-owned-build/bin/LegacyMainSourceLines.Probe/release/LegacyMainSourceLines.Probe.dll E:/BlurMask-build-artifacts-20261014/source-line-owned-managed.json
dotnet publish tools/LegacyMainSourceLines.Probe -c Release -r win-x64 -p:PublishAot=true --artifacts-path E:/BlurMask-build-artifacts-20261014/source-line-owned-native-build -o E:/BlurMask-build-artifacts-20261014/source-line-owned-aot
python tools/LegacyMainSourceLines.Probe/run.py E:/BlurMask-build-artifacts-20261014/source-line-owned-aot/LegacyMainSourceLines.Probe.exe E:/BlurMask-build-artifacts-20261014/source-line-owned-native.json
python tools/handwriting/audit_main_owned_source_lines.py E:/BlurMask-build-artifacts-20261014/source-line-owned-static.json
```

The static auditor requires the pinned private PE and performs disassembly
only. Runtime and public tests do not load or execute vendor code.
