# Managed owning-first diagnostics

Run from the repository root. Requires .NET10, Python3 and NumPy. This reflection-based observer tool is deliberately separate from the original NativeAOT probe. Its assembly name retains the existing friend identity; it is never published or loaded with that probe.

```powershell
dotnet run --project tools/LegacyFirstOwnedRecognition.Diagnostics/LegacyFirstOwnedRecognition.Diagnostics.csproj -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json .artifacts/legacy-first-owned-managed-audit.json
python tools/LegacyFirstOwnedRecognition.Diagnostics/audit_model_replay.py . tools/LegacyFirstOwnedRecognition.Diagnostics/input-pins.json .artifacts/legacy-first-owned-managed-audit.json.owning-replay.json
python tools/LegacyFirstOwnedRecognition.Diagnostics/audit_primary_replay.py .artifacts/legacy-first-owned-managed-audit.json.owning-replay.json
python tools/LegacyFirstOwnedRecognition.Diagnostics/audit_context_replay.py .artifacts/legacy-first-owned-managed-audit.json.owning-replay.json
python tools/LegacyFirstOwnedRecognition.Diagnostics/audit_terminal_replay.py .artifacts/legacy-first-owned-managed-audit.json.owning-replay.json
```

The original four fixtures require42/3/6/6 actual groups with unique ordered ordinals (57groups/114networks). Model replay independently recomputes trajectory/raster features, int8 networks, authenticated Russian-first mixedQ15 and integer ensembles. Primary and B0/B8 recurrence, stable insertion, ancestry, terminal minimum, predecessor trace and WORD scalar offsets20/24/28 are independently checked. Separate observed allocation identities are validated by complete parent/symbol ancestry; every other storage byte is compared, including reserved and inactive cells.

Normalized/merged geometry, context28, boundary signs and geometry callback values are captured actual-source inputs. Additional context evaluation is diagnostic and never supplies production scores. Cache195 policy, normalization, callback arithmetic and WORD source-kind/logmass are not independently rederived here. These checks establish the narrow owning path, not Russian recognition quality or full public routing.

The corrected first fixture cost40843 follows the authenticated first-source mixed converter. Earlier41268 used stale NN conversion; labels remain[22,70,25], literal mixed-script M\u0418P. All57groups (including seven not covered by the older35group fixture) were replayed before the golden changed. Other three costs12111/14219/29007 remain unchanged.
