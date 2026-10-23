# Experimental classified AUTO and Math2

This integration is newer than `v1.2.8-rc.9`. RC9 bundles the new Math2 route
for explicit formula mode; it does not include this experimental AUTO fallback.

This path is selected only when the experimental engine setting is enabled,
mode is `auto`, and no installed user raw-content pack overrides it. Default
AUTO, installed user-pack dispatch, explicit text and manual formula modes retain
their previous routes. An installed invalid pack still follows its previous
failure/fallback policy; bundled assets do not silently override user selection.

The packaged RawContent2 classifier uses the original graph (SHA-256
`788aaed9ed4b4eb2f62f0806695f1353d92826889c2e6ce8f503eba56c9c8741`),
the existing recovered node11/edge16 profile and the existing official bundled
ORT runtime. Its exact exported pack is 5,341,832 bytes, SHA-256
`b3bfe175da6627d8f8281675b5d61aba979478444ccffa0b6eec976fce102d24`.
The existing Math2 packaging command now also packages/verifies this fallback.
The raw-content exporter and feature arithmetic are unchanged, including the
native unusual angle/pi radial feature.

Each category region must have finite softmax >= .85, actual matching per-stroke
predictions, exact minimum score, unique in-range indices and no overlap with
another eligible region. Chronology follows the original stroke indices.
Drawings, uncertain, malformed, unsupported, failed and oversized regions stay
ink. A mixed color/fade formula stays ink rather than being split into separate
formula replacements. The classifier allows 20,000 points per bounded fragment;
Math2 separately admits at most 4,096 original points in one formula region.

Math regions call the existing cold selected-first Math2 recognizer and require
supported structured syntax. The result has `ExperimentalClassifiedMath`
admission, separate `CategoryScore`, and **formula confidence=null** (transient
NaN carrier, as in manual Math2). Category softmax is not formula confidence.
This is a deliberately experimental classification + structural-support policy,
not calibrated recognition probability or a claim of universal OCR accuracy.
Its publication gate rechecks mode, setting and installed-pack override.

Text regions retain the existing scored text decoder and thresholds. Calculation
occurs only after whole-region admission and an empty right side after `=`;
scored numeric text retains the old calculation behavior. Unsupported results
do not trigger a solver. Generation cancellation, quiet-period publication,
original-ink association, replacement undo and peer DTOs use the existing app
mechanisms. Admission and category score are not serialized into peer ink DTOs.

The bundled raw owner is lazy, one-at-a-time, fingerprint checked and failed-load
memoized by asset identity. Settings/mode release never blocks the UI waiting
for inference; an active call releases in its finally block. Math2 shares its
existing bounded owner between explicit formulas and this experimental AUTO.
InvalidDataException is now explicitly recoverable in the manual Math2 owner as
well, so corrupt model loads use the existing failure memoization rather than
retrying on every pen lift. Cancellation remains separate from failed loading.
A reflection runner checked the real built application owner with a corrupted
copy of the bundled pack: repeated calls with unchanged assets did not reload;
changing the pack modification time allowed one retry; release reset the cache;
pre-cancellation did not poison it. This check executed no neural inference.

Verification commands (prepared/injected, no inference):

```
python -I -B tools/test-experimental-auto-assets.py --output <E-output>
dotnet <built BlurMask.dll> --experimental-auto-layout-test <E-output>/models/raw-content2.ikrraw
dotnet <built BlurMask.dll> --experimental-math-layout-test
```

Prepared checks establish dispatch/admission, complete membership, style and
point limits, original snapshot ownership, null confidence, text/math separation,
equals gating, cancellation, undo and finite DTO transport. Static asset controls
independently construct the pack from original graph bytes and reject six corrupt
variants. These tests do not establish classifier or formula accuracy. Actual
classifier -> Math2 inference was also checked on Windows Managed using two
unchanged inputs. The 14-stroke mixed sample classified indices 0..4 as text
and 5..13 as math; Math2 returned `y=3x+2`, associated with exactly 5..13. Its
text region remained ink. The seven-stroke arithmetic sample returned `1+2=`
and rendered `1+2=3`. Both produced valid peer DTOs with null formula confidence.
These two diagnostics are not a handwriting accuracy benchmark or a claim of
finished English/Russian text recognition. No labels or scores were tuned.
The native Windows/Linux/macOS workflow stages run the injected route guards
and verify the bundled raw pack. Actual inference was not repeated on those
platforms by these local checks.
