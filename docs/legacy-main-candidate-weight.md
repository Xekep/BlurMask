# Main candidate field0

`LegacyOwnedRussianMainGlyphConfidenceGroup.ReadNormalizedCandidateWeight`
restores frontend150 (`1529A0` → ordinary21D.V78 `1C1260`). It reads the
first retained candidate with the requested label, or returns zero when absent.
This field is separate from `ReadSymbolScore` (frontend1B8 → V80 `1C1300`,
candidate.vector10[channel88]).

Native `1BE530` assigns a singleton weight of one regardless of its integer
cost. Otherwise it computes exp(-float(cost)/(scale*8192f)) for the first twenty
entries, adds in float32 order, normalizes in float32 and zeros later entries.
There is no exp clamp, fallback on zero sum, or replacement for NaN/Infinity.
The public owning factory obtains scale from the actual main scorer; the
supported raw100=0 resource branch proves scale one. No caller-supplied costs,
labels, confidence vectors or substitute scale enter that owning API.

The new probe accepts AK, LK, engine, separation, case-data, publication-data,
line30, line50 and capture paths. It checks 320 explicit numerical cases
separately from 92 real owned DIRECT/case graph groups (908 retained rows),
including singleton overflow costs, top20 tails, integer-to-float rounding,
unclamped exp(-80), underflow and overflow. The Python audit independently
rounds every float32 operation and compares exceptional values by class.
Existing confidence captures remain unchanged. Static auditing pins the PE
producer and both getter/thunk chains without loading the vendor library.

This supplies one scalar to the compact-word materializer. It does not establish
a complete word winner, source geometry publication or Russian handwriting accuracy.
