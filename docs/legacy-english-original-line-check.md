# English original-line frontend check

This is a frontend diagnostic, not a recognition-quality result or a completed
MAIN/TEXT integration. The source was built from
`fa35565d15f4857a15279f0290dca6f1ba796ccc` in an isolated checkout.

## Input and provenance

The fixed input is writer 0, sample 0 from the original variant of the
[BRUSH research dataset](https://github.com/brownvc/decoupled-style-descriptors#brush-dataset).
The sample was chosen by its numeric identifier before running inference.
Other variants and other samples were not searched for a successful result.
The dataset author's original variant already includes temporal interpolation;
this check does not claim to use raw device events.

The dataset is restricted to noncommercial research. Neither the sample nor its
target text is included in this repository, packages, or release artifacts.
Only one ZIP member was downloaded. Its pickle was inspected as opcodes and
literal byte payloads; no pickle deserialization or embedded callable was run.
The original drawing payload was verified byte-for-byte after conversion.

The inference input contains 10 strokes and 689 points. Author-provided pen-up
markers define the stroke boundaries. Coordinates were not flipped, scaled,
resampled, clipped, split, or combined; the existing Float32 consumer performs
one conversion from the stored Float64 values. Device DPI is unknown, so 96 is
an explicit diagnostic convention, not measured physical metadata.

Input SHA256:
`5900b04db0faa5e9c8401a58d94b1f4502704081e9e638b98ab54ad6eed55129`.
The target text was kept separate and was not read by the inference entry.

## Observed result

One root-owned invocation completed in about 1.13 seconds with two logical
processors, affinity mask 3, a 40-second cooperative deadline and a 45-second
outer watchdog. This is an observation, not a latency guarantee. Source,
resources, runtime and own binaries had 1,122 pinned entries checked before
and after the invocation.

All real frontend children were processed. The input produced one nonflat
child with 48 nodes, 315 groups and 279 remaining candidate arcs; 36 arcs were
removed by the existing candidate preparation. FIRST119 completed for that
child. Parent postprocessing reached `post265`, with one descriptor, one guide
and 16 rows. No input strokes or frontend children were omitted to fit an
entry-point restriction.

Capture SHA256:
`b7bd4e015e91f97c13aea51e0cb9726ff85fed3abc5568cb69b7f21590683a48`.

This invocation did not run MAIN, complete a shared TEXT/document, admit a
recognition result, or compare a recognized sentence with its target. The
multiple-word MAIN publication and shared TEXT transaction still require their
own source review and verification. The observation establishes a genuine
original-stroke input for that work; it does not establish English or Russian
handwriting accuracy.
