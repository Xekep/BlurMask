# Main-parser word transition geometry

`LegacyMainWordTransitionGeometry` connects the main sampler's prepared
geometry caches to the **full 80-byte** word recombination request. Its
callbacks can be passed directly to `LegacyNativeHigherArcRecombination` via
`CreateOperations()`. They compute resource-backed native costs rather than
supplying zero or scripted corrections.

This is a bounded prepared-source stage. Main source postprocessing, source
guide selection and word-controller scheduling remain separate. The main
frame must not be replaced with the first parser's `6120` frame.

## Native path

Main frontend `118` is `1801525e0`, forwarding through graph
`198/1801ad890` to sampler `A8/180195c40`. For plain symbols and an ordinary
same-descriptor group with zero endpoint nibbles, negative boundary index and
kind0, graph `100/1801a7f20` returns status0. The kind2 path uses
`118/1801a8670`, likewise status0 in this scope. The frontend's kind2 guard
requires every current physical range to start at point0; an internal cut
without the primitive predecessor exception returns false and sets the cost
and accumulator to impossible.

Main frontend `140` is `1801528c0`. Ordinary graph
`1c8/18005a400` returns false, so it calls
`190/1801acfa0`. The actual main sampler at AK offset **188157** stores
runtime field `50=0`, forwarded by sampler `78/180194170` to graph `192=0`.
Consequently the ordinary kind0/2 updater returns true before pair geometry.
This no-op follows the real main profile; the first sampler stores `50=1`
and runs its different pair cost. No first-parser callback is substituted.

`LegacyMainWordTransitionProfile` reads the main version106 category and
scalar fields independently of the first acoustic model loader. Disabled
pair fields are retained as raw data. The main resource contains an inactive
NaN in this disabled storage; rejecting it as an active geometry value would
incorrectly prevent loading the real resource. All active values are finite
and checked.

## Preparation and full request fields

The caller supplies the matching candidate graph, owned prepared point buffer,
actual guide height, source gap and a complete ordinal-indexed snapshot of
compact group/input descriptor facts. Those facts identify the source
descriptor and line, descriptor flags, boundary index, endpoint nibbles,
group kind and group flag byte. They are copied and validated. This API
supports one descriptor, ordinary kind0 descriptor flags, no suppressed arcs,
zero endpoint nibbles, kind0 groups and no opaque/boundary override. It does
not assign those fields by guessing from physical stroke ordinals.

`1941b0` folds future minimumX in reverse through actual successor lists, and
prefix maximumX forward through actual predecessor lists. With no assigned
line, caches retain their impossible sentinel and boxes retain their own
extrema. A nonnegative source gap scales the default gap by stored `110`;
negative source gap uses stored `6c`. Matrix scaling uses stored `114` and
the first matrix entry. Join, triple-gap join and the full 6×6 category cache
preserve native caps, last-row minus1 and uncapped last-column behavior.

The geometry lookup uses current node/arc at request **00/0c**, but previous
**source node/packed arc at 04/0e**. These must not be replaced by previous
ending node/graph arc **08/10**, which instead select the successor list.
Continuation records can keep an earlier source reference while ending at a
later node.

Kind0 chooses the ordinary join cache, or the triple-gap cache for source
kinds7/8. Kind2 selects the category matrix using first IDs at request
14/18, with the native previous-source-kind10 row override. Translation
truncates its float term **before** multiplication by8192. Signed32
arithmetic wraps as in the native instructions, including boundary controls.
The actual cost limit is request34, not a fixed acceptance constant.

Frontend118 additionally updates the cost accumulator at **30**: accepted
calls add the actual cost delta to its existing caller-provided value;
rejected calls store impossible. This field is separate from cost-cell
identity28 and limit34. Unrelated request bytes and callback flags remain
unchanged in the supported ordinary branch. Frontend140 preserves packet
and cost because its main-profile delta is zero.

Special labels, primitive exceptions, Arabic traits, opaque groups,
cross-descriptor transitions, kind1/3/4/5/6/7 and source-kind9 bypass policy
are explicitly unsupported. This is not a complete general SDK transition
dispatcher or Russian acceptance policy.

## Independent verification

The probe exports actual graph topology, source inputs and observations.
Its Python interpreter independently reads the original AK scalar fields
and pinned256-entry separation table, recomputes the cache folds and full
request cost/accumulator output, then writes expected controls. It does not
derive expected costs from the managed output fields.

Managed and Windows Native AOT controls cover 12 prepared-source snapshots,
336 actual graph cache entries, 7,632 full request callbacks, 7,632 canceled
callback invocations and eight ownership/unsupported-scope guards. The snapshots vary
scale, source gap, assigned/unassigned line, transition kind0/2 and source
kind6/7/8. They include internal cuts, impossible caches, int32 wrap,
accumulator residue and complete unchanged-byte checks. In3,600 additional
calls the previous source reference differs from the previous ending node.
The oracle also recomputes each bounding box independently from the original
prepared points and group ranges, so an incorrect managed box cannot become
an echoed expected input.

The integration probe then consumes actual RU NN/cache costs, real native
root and continuation rows and the existing higher-arc controller. Six
higher stored rows cause 30 ordinary successor queries, **72 actual main
callbacks**, 30 persistent-workspace merges, final context projection and
intact materialization. These corrections replace the earlier diagnostic
`+17/-3` callbacks. Mode1, source facts, guide80 and source gap−1 remain
explicit prepared fixture inputs; this does not claim that the unfinished
first-result→main-source frame producer has been restored.

Root's current source trace shows `157c60` produces main data from first
`273` results, updates descriptor34/38 and copies frame150 to childA8.
This lifecycle is nontrivial and remains outside this geometry batch. The
complete engine, production Russian cursive replacement and calibrated
winner/confidence still require that source bridge and controller assembly.

From the repository root:

```powershell
dotnet run --project tools/LegacyMainWordTransition.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json .artifacts/main-word-transition-audit.json
python tools/LegacyMainWordTransition.Probe/reference_controls.py .artifacts/main-word-transition-audit.json .artifacts/main-word-transition-controls.json
dotnet run --project tools/LegacyMainWordTransition.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json .artifacts/main-word-transition-audit.json .artifacts/main-word-transition-controls.json
dotnet publish tools/LegacyMainWordTransition.Probe/LegacyMainWordTransition.Probe.csproj -c Release -r win-x64 -p:PublishAot=true -o .artifacts/main-word-transition-native
.artifacts/main-word-transition-native/LegacyMainWordTransition.Probe.exe models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json .artifacts/main-word-transition-native-audit.json .artifacts/main-word-transition-controls.json
```

Private static references are in `research/myscript`: Text `1525e0`,
`1528c0`, `1ad890`, `1acfa0`, `1941b0`, `195c40`, `1a7f20`, `1a8670`, and
the confirmed descriptor/runtime tables. No vendor DLL is executed.
