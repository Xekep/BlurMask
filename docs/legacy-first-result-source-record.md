# First result into ordinary source rows

`LegacyFirstResultSourceRecord` restores the bounded numeric/range writes of
Text `156950`, `012bc0`, `157030` and `157340`. This is the missing downstream
consumer of an actual `273` result's nested glyph alternatives; it does not manufacture that result
from a word string or substitute a first-model frame for the main model.

## Proven producer and callback contract

`158970` prepares a new child249 and invokes `157c60` for unresolved ordinary
input descriptors. `157c60` initializes a real273 result, submits its selected
physical strokes and then calls `156950` for surviving glyphs. For kind0 it
may omit very flat strokes: width > `3 * sourceScale` and height/width < .02,
except the last stroke when it is the sole submitted stroke. Thus child
physical ordinals require an explicit retained-child-to-parent mapping.
Kind6 bypasses that filter. Source ingestion and glyph emission are distinct.

`156950` walks linked120-byte **word276** records under result273. The result
service+20 virtual48 returns a packed key with a **signed24** graph end node,
**signed16** graph arc in bits24..39, and **signed16** selected273 alternative
ordinal in bits40..55. A negative arc is skipped. These are not Unicode,
physical stroke IDs or modern document object IDs.

For a valid glyph the required callbacks are:

| Call | Native binding | Output |
|---|---|---|
| frontend168 | `152a10` → graph21b.B8 `1a7bf0` | Packed first boundary of that end-node/arc |
| frontend170 | `152a30` → graph21b.C0 `1a7c70` | Packed last boundary |
| frontend188 | `152b20` → graph21b.120 `1a9ae0`, mode0 | Array of inclusive24-byte source-key pairs |
| nested glyph alternatives | TOP273+48 pool, canonical graph key → result service+8.08 `13ac80` |64-byte AVL owner+18 packed count and owner+20 pointer to40-byte glyph alternatives; selected ordinal from the packed key |
| categories | frontend48 `151bd0` → graph170 `1acf30` → graph188 | Start/end geometry maps and prototype table |

`1a9ae0` has graph path/source-ref merging cases. The source row API requires
its actual emitted ranges; it does not replace this callback with a guessed
bounding box, all physical strokes, or a Unicode context path. Each endpoint
key is `(sourceGroup, childPhysicalStroke, retainedPointOrdinal)`.

The nested alternatives are **not the TOP273 alternatives**. `156950` starts
at TOP273+68, follows its first40-byte alternative's linked word pointer+18,
and resolves the120-byte word as `(node.owner+18)+node.index*78`. The word
service enumerates its packed graph references. For each reference `156950`
passes **TOP273+48**, plus a canonical key retaining node24/arc16/bit56, to
result service+8 virtual08 `13ac80`. This returns a64-byte AVL owner, whose18
packed count/20 array contain labelU16+8 and glyph scores atC/10/14. The
selected alternative ordinal is read separately from reference bits40..55.
Service28 `13ae90` resolves the individual40-byte record using the same pool
and that full reference; it returns a record, whereas08 returns its owner.

The machine-code pins are explicit: entry156955 saves TOP273 argumentR8;
156985 retains it inR13;156bf5 reloads that same saved argument;156c15 loads
`RDX=[R13+48]`;156c2f selects service08 and156c33 calls it with the normalized
key inR8. **Word120+48 is not this pool source.** The initial description
confused Ghidra's unchanged `param_3` (TOP273) with the enumerated word object;
that evidence error is corrected here without changing the prepared-row API.

TOP273 alternatives have float mass at8; feeding those rows to this source
producer would corrupt labels. The current raw B8 terminal scanner alone has
neither this nested owner nor the source callbacks.

## Owned row representation

Native `155c70` zeroes a720-byte row. The pointer at00 is represented by the
owned `Ranges` collection; `CopyStorage()` contains a zero pointer placeholder
and the correct U64 range count at08. No process address is serialized.
All remaining restored fields keep their native byte offsets:

| Row offset | Producer meaning |
|---|---|
| 10 | Signed−1 existing source item;14 remains0 |
| 18/1c/20/24 | MinX/MaxX/MinY/MaxY across inclusive retained source ranges |
| 28..4f | Twenty U16 labels; selected label first, then exact-label dedup in nested glyph encounter order |
| 50/54 +8*i | Selected row uses `pow(alt.C, explicitChildExponent)` and alt10; later labels copy altC/10 directly |
| f0 +4*i | Alt14 score channel; unused labels get04000000 |
| 140 +4*i | Geometry channel count0/1/2 |
| 190 +8*i | Upper geometry channels |
| 230 +8*i | Lower geometry channels |

`157340` uses the **start** geometry map's prototype for both geometry
channels when the end map is available: `lower = -1 - 2*First`,
`upper = 2*Second + lower`. Replacing the second lookup with the end-map
prototype changes native behavior. Exponent, categories, alternatives and
source ranges remain supplied from their actual owners.

`012bc0` remaps filtered child physical ordinals into the parent buffer,
splitting gaps and coalescing adjacent mapped strokes. Cropped first/last
point ordinals remain intact. `157030` scans these inclusive spans in the
parent retained float coordinates. Raw-before-dedup point IDs cannot be
substituted for these indices.

`PublishDescriptorInterval` ports the ordinary unresolved descriptor's34/38
writes around source append. It preserves other88-byte descriptor fields.
Reused-source cloning157970, rollback/allocation, downstream shifts158970,
special result flags, ownership of selected273 glyphs and the main247 frame
factory remain separate. This phase is not full Russian recognition.

The portable implementation limits projected ranges to4096 **before adding**
another fragment. Its separate geometry work budget permits at most1,048,576
point visits per record, including repeated visits from overlapping ranges.
A coordinate-free preflight counts these visits before bounding-box traversal;
the actual point loop also checks cancellation on every visit. Input-buffer
validation and submission filtering check cancellation per point. These are
portable resource limits, not parameters or behavior recovered from native
157030. Work exceeding the budget is rejected rather than silently changing
source membership. The native geometry arithmetic is unchanged within bounds.

## Verification

```powershell
python tools/LegacyFirstSourceRecord.Probe/reference_controls.py D:/first-source-record-controls.json
dotnet run --project tools/LegacyFirstSourceRecord.Probe -c Release -- D:/first-source-record-controls.json
python tools/handwriting/audit_first_result_source_records.py --output D:/first-source-evidence.json
```

An independent Python membership/float/label oracle covers512 source rows,
cropped endpoints, contiguous and discontinuous child maps, repeated labels,
selected alternatives, missing one/two geometry channels and negative costs.
The720-byte pointer-free numeric representation is compared bytewise;
ownership/cancellation/malformed inputs and signed packed keys are checked.
Submission filtering is checked for all256 flat/nonflat eight-stroke patterns,
kind6 bypass, the last-stroke fallback, width equality and the native **double**
.02 comparison after float32 height/width division. Descriptor34/38 publication
is checked for128 existing-source offsets with untouched opaque fields.
Small-buffer overlapping fixtures accept exactly1,048,576 visits and reject
257 repetitions of a4096-point stroke. Fragmented mappings accept exactly4096
projected ranges and reject the next addition without constructing excess
fragments. These guards exercise valid source membership, not malformed input.
An instrumented point list cancels at the first bounding-box coordinate read;
the probe verifies that no second point is read after cancellation.
These inputs are synthetic prepared273 fixtures, not vendor execution or an
accuracy claim. Actual raw B8 currently has mixed-script `MИP`; its terminal
selection alone does not supply the required273 glyph/source callbacks.
