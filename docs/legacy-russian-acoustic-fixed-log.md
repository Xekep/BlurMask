# Legacy integer acoustic log costs

`LegacyAcousticFixedLog` restores native `180249e90` for the bounded quantized
probability domain zero through 32,768. The positive integer cost used by
`1801b9e20` is a separate channel from mode1 calibrated class confidence.

For probabilities at least 16, the native function divides by 16 and chooses
the next table index only when the remainder is greater than eight. Ties use
the lower index. For probabilities one through 15 it returns `table[p] -
0x58b9`. Zero returns the negative impossible sentinel `-0x4000000`. Native
acoustic scoring first clamps zero to one, so `ProbabilityCost(0)` differs
from `-LogProbability(0)`; the API exposes both operations explicitly.

Every positive entry of the 2,049-entry static table at `180261290` matches
`truncate(8192 * ln(index / 2048))`. The reusable component constructs the
table mathematically and contains no copied DLL data. It rejects probabilities
outside the model's bounded domain. The implementation's `Math.Log` table
construction is checked against independent Python logarithms for all 32,769
input probabilities in Managed .NET and Windows Native AOT. A separate
read-only Python mode validates every reconstructed table entry against an
explicit local inspected PE and produces ignored, private static-byte controls.
No DLL is loaded or executed.

```powershell
python tools/LegacyFixedLog.Probe/reference_controls.py .artifacts/own-fixed-log-controls.json
dotnet run --project tools/LegacyFixedLog.Probe -c Release -- .artifacts/own-fixed-log-controls.json
```

The probe also checks analytic powers of two, the small-probability branch,
rounding ties, zero semantics, bounds and monotonicity over the entire domain.
Native weighting, related-form penalties and complete acoustic/lexical cost
addition remain separate work. This component is not wired to the UI and
does not establish complete Russian word or cursive recognition.
