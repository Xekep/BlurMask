# Prepared fixed-sequence integer word endpoint

`LegacyPreparedSequenceWordSearch` connects caller-supplied sampler297 atom
markers, consecutive scored glyph groups, seven resolved acoustic weights and
explicit word flags to `LegacyIntegerWordDecoder`. It retains composed point
range ownership and returns acoustic, pre-lexical, language and total costs.
Terminal language scoring happens before the final bounded beam is pruned.

The groups must partition the supplied atoms. Marker bounds refer to the
**prepared** stroke lengths; same-stroke atoms retain the shared endpoint rule
from native1935e0 and193690/193720. The existing integer decoder uses an original
bounded acoustic prefix beam and original minimum-cost related-form mapping.
This endpoint does not reproduce native candidate generation or search.

Every glyph score is already supplied by the caller. There is deliberately no
conversion from `LegacyCursivePreparedInk` float coordinates to normalized
`LegacyInkStroke` short coordinates. Inserted points can be fractional; silently
rounding them would assert a normalization rule that has not been established.
The endpoint validates ownership and score width, but cannot establish that an
externally supplied score was computed from its corresponding point range.

Whole-word category, resolved per-glyph seven-slot weights and multiplier,
history, category/boundary flags, additional prior/cut/correction path cost,
heap fullness, cost limit and beam width are all explicit. It does not derive a
word category from a last glyph, treat the seven NN slots as seven categories,
choose default dynamic weights or claim raw-ink/cursive accuracy.

Run the vendor-free check:

```powershell
dotnet run --project tools/LegacyPreparedSequence.Probe -c Release
dotnet publish tools/LegacyPreparedSequence.Probe -c Release -r win-x64 -p:PublishAot=true -o .artifacts/legacy-prepared-sequence-native
.artifacts/legacy-prepared-sequence-native/LegacyPreparedSequence.Probe.exe
```

Its own zero-weight NN creates uniform three-class probabilities. Independently
computed integer acoustic cost is8995. Reversed acoustic/lexical alphabets and
an own nonuniform language model select the terminal with lexical subtotal150,
even with final beam width1. The test verifies cost components, explicit heap
tie rejection, shared endpoint composition, preserved fractional point(.25,.75),
partition/bounds failures, inactive weights and cancellation. These are transport
and arithmetic controls; the normalized glyph is explicitly supplied separately
from prepared float geometry and is not a handwriting accuracy fixture.
