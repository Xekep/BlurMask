# First ordinary row acoustic model

`LegacyFirstAcousticModel` reads the owned first ordinary model inside an already
selected current row source247/v10d. It follows typed child boundaries rather
than searching for network magic. It evaluates **caller-prepared complete short
vectors**; it does not select glyphs, normalize raw float ink, manufacture prior
context, fill the final class cache or choose a recognized word.

The checked Russian resource SHA256 is
`7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978`.
Current source247 starts at6120. Its postprocessors are24a at11421 and265 at11457.
Reader1d4950 then consumes presence byte11505, graph297 at11506,
graph21b at12727 and ordinary21d at13082. After that ordinary object, the v10d
source tail has zero extra objects and186 inert metadata bytes. The loader checks
every child remains inside that source object. It retains independent immutable
network snapshots; subsequent changes to the caller's resource bytes cannot
alter evaluated model weights.

The first ordinary21d/v110 profile has context selector2 and no dynamic257:

| Slot | Network byte range | Input / hidden / output | Kind | Mode |
| --- | --- | --- | --- | --- |
| Context |13103–19094|30 /100 /28|7001|Separate context producer|
| Trajectory0 |19195–105190|247 /200 /180|1f|2|
| Raster1 |105197–187592|229 /200 /180|2f|2|

Ranges are half open. Only the second network position in slots0/1 is populated;
slots2–6 are disabled with stored weight0. The consumed ordinary prefix ends
at187648. The loader deliberately rejects dynamic, grid, conversion, nonempty
list and other network-position branches rather than assigning defaults. The
ordinary resource tail is left to existing separate cache/profile readers.

These are **not** the second model's256/238 baseline inputs. The earlier restored
trajectory217 and raster199 geometric prefixes plus28 context and2 boundary
channels give247 and229. An isolated short-ink diagnostic can use the existing
`ExtractIsolatedInput` APIs; this does not prove raw source normalization or
automatic first-pass context selection. `ContextNetwork` is exposed for the next
producer composition and is not silently used as an unrelated classifier.

## Fresh no-dynamic weights

Evidence uses the pinned MyScriptText.dll SHA256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`:

* Constructor1b0330: instruction1801b04c4 writes DWORD runtime1b8=0;
  instruction1801b04ca writes QWORD runtime1bc=`0x3f800000`, i.e. float1 at1bc
  and float0 at1c0. These are fresh mode1 and mode2 values, not chosen constants.
* Initializer1b0760 reads resource dynamic flag+10. A zero flag sets runtime1b0
  toNULL; a nonzero flag creates257 and copies separate property1c4.
* Callers1be930 and1bf0a0 invoke dynamic updater1b7440 only if runtime1b0 is not
  NULL. The first resource therefore keeps the constructor values.
* Combiner1b9e20 reads stored mode0 weight, runtime1b8 for mode1 and runtime1bc
  for mode2. Thus both active first-model slots have fresh weight1.

`GetFreshWeights` and `GetFreshIntegerCosts` apply only to this validated fresh
no-dynamic profile. They do not describe cloned/reused runtimes, external overrides
or other profiles. The existing restored integer class-score conversion and
seven-slot acoustic combiner are reused. The result contains probabilities and
integer acoustic costs; it intentionally exposes no invented confidence value.
It is still a pre-cache channel, not a final lexical score.

## Verification

```powershell
dotnet run --project tools/LegacyFirstAcoustic.Probe -c Release
dotnet run --project tools/LegacyFirstAcoustic.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json
```

The vendor-free fixture owns its synthetic source and zero-weight networks. It
checks1265 truncated inputs, typed-child and dynamic rejection,128 independent
analytic equal-class probability/ensemble cases, explicit masks, exact input
widths, cancellation and snapshot ownership. These expected probabilities follow
from equal zero network outputs, rather than copying the implementation's output.
The actual-resource mode verifies first-profile offsets/shapes and exact output
agreement with independently loaded byte slices. Its supplied isolated printed
M stroke's top integer class is22 at4244; this is a narrow model identity check,
not recognition accuracy or a native raw-ink pipeline claim.

Managed and Windows Native AOT pass both modes. The remaining integration gap is
first-pass raw normalization/context and native runtime search/winner, including
representative joined Russian handwriting. Russian AI-pen UI remains disabled
until those producer/consumer and quality checks are complete.
