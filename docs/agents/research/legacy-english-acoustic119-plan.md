# English acoustic119 and first-source24C: static boundary

This is a raw-resource and pinned-PE inventory, not an English recognition endpoint. No vendor library is loaded or executed. The existing Russian21D/110 reader, its guards, features and captures remain unchanged.

Run `python tools/handwriting/audit_english_acoustic119_plan.py report.json` from this checkout. This research-only auditor uses the existing local decompilation helpers. It checks the manifest hashes, nine native byte ranges, thirteen instruction pins, both24C dispatch bindings, six network alignment dispatch bindings, and all five main plus five nested first acoustic nodes through their exact ends. Its JSON retains offsets, dimensions, network hashes and matrix locations, profile/slot flags, cache fields and ordered child ownership.

## Actual selected owners

| Resource | First249 / nested247 / acoustic21D | Active first networks | Main acoustic21D |
| --- | --- | --- | --- |
| en_US-ak-cur.lite | 8827 / 8843 / 18695 | slots0,1: 219,201 inputs | 184155 |
| en_US-ak-cur | 8827 / 8843 / 18695 | slots0..3: 219,201,276,276 | 1054648 |
| en_US-ak-superimposed.lite | 8134 / 8150 / 27890 | slots0,3: 219,276 | 219250 |
| en_US-ak-superimposed | 8134 / 8150 / 27890 | slots0,3: 219,276 | 219250 |
| ru_RU-ak-cur.lite | 6104 / 6120 / 13082 | slots0,1: 247,229 inputs | 189734 |

Every English first source is an actual child of249, ends four bytes before that249 end, and has source exponent0.75 in those final bytes. The English first21D is119, has initial-network=false, dynamic=false, one non-grid profile, and active slots with first=false, second=true, mode2. These are serialized facts, not caller-selected modes. All English first output widths are182; Russian first output width is180 and it additionally owns the30-input initial network producing28 context channels.

English main profiles also lack the initial network. Cursive main has a dynamic block; superimposed main does not. Full cursive main has two profiles, including a grid-enabled second profile. These branches are not interchangeable with the bounded first-source profile. The main cursive input widths begin228,210,285,285; they are not the Russian256,238,313,313 widths.

## Version-aware21D wire differences

Native reader `1801C2940` and parent reader `1801C25B0` establish the following order and gates:

- Version118 and later serialize the float cost multiplier at native0C. Older versions initialize it to1.
- A missing initial network also omits the initial context parameters. A zero dynamic flag does not carry a dynamic payload.
- NativeA0 is serialized only below116. The obsolete byte afterC0 exists only for versions10C through113 inclusive.
- PositiveC8 groups retain count+1 ushort boundaries, a label vector selected by the final boundary, and one float per group from115 onward. Earlier versions broadcast one float.
- The obsolete byte following100 is absent from119. The108 calibration mode remains a separate payload selector; it is0 in the English resources, whereas Russian main has a192-by-four-family calibration table.
- Versions above110 carry the148/150 optional-child flag. The optional final resource flag present in110..116 is absent in119.

English first has eight positive groups,17 labels, noB8 size penalty, and raw100=0. English main has seven positive groups,25 labels, B8 enabled, and raw100=.1 for cursive or.2 for superimposed. These payloads cannot be silently accepted by the old strict cache reader. Exact values and raw network hashes are retained in the auditor output.

## Network107 is a separate dependency

All inspected English first/main networks use FEA0/version107. Their architecture byte is12, the two unused middle widths are zero, and their final parameter is10. The31/47/63/79 network kinds belong to the stored EXP family. This inventory does not equate that family with a newly implemented evaluator.

Header reader `1800EC270`, matrix reader `1800EC490` and dispatch factory `1800EEFD0` establish16-byte alignment: all six selected backend tables have slot0=`180083980`, which returns16. Version107 aligns each matrix start to the absolute resource stream position and pads its input stride to a multiple of16. Matrix row-scale vectors include the leading sentinel scale. Both aligned matrices and both scale vectors must end exactly at the FEA0 envelope end. The existing106 `LegacyInkNetwork` uses packed rows and cannot load these bytes by merely widening its version check. Copying an isolated107 envelope and forgetting its original stream offset also loses the alignment origin.

The next bounded reader should retain the selected `LegacySource247WireLayout`, the exact owned140 child, all prefix/cache scalar fields and arrays, and network107 matrix metadata/weights with their original absolute offsets. First cur.lite's actual no-initial/no-dynamic profile is the initial supported executable-model scope. Positive cache groups must be typed, not skipped. Main/grid/dynamic profiles remain explicit unsupported branches until separately joined. Feature evaluation must wait for proven EXP activation and feature-slot semantics.

## Additional first-source24C

English first processors are ordered24A,24C,265; Russian first processors are24A,265. Cursive24C occupies16878..16902, superimposed24C16185..16209. The24C runtime table `1802DFE20` slot18 targets `180176A10`; its reader interface `1802DFE78` slot18 targets `180176FB0`. The version100 payload is exactly two serialized float values at native8/C. Setter `180176980` converts external degree arguments to radians; the wire reader reads the stored floats directly, so applying the setter conversion again would be wrong.

`176A10` scans the real unassigned physical ranges and accumulates the signed Y-direction transition/run counter. The comparison at176B0B/176B0F returns unchanged for a counter below20. The other branch transforms real points using trigonometric geometry. Its existence prevents treating24C as an unconditional no-op or substituting the Russian first frame. A future bounded short-frame join must either port that geometry or establish the below20 branch using the same actual owned source and preprocessing. No English recognition claim follows from this wire inventory.
