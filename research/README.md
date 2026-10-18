# Recognition engine research

Local MyScript static-analysis materials are stored in `myscript/`, outside
build artifacts. The entire directory remains excluded from Git and releases.

- `myscript/output/MyScriptText.dll/decompiled.c`: text engine export.
- `myscript/output/MyScriptEngine.dll/decompiled.c`: common engine export.
- `myscript/output/MyScriptMLOrt.dll/decompiled.c`: inference integration export.
- `myscript/output/Math2FeaturesStage4/MyScriptInk.dll/decompiled.c`: ink export.
- `myscript/output/Math2PostprocessingStage4/MyScriptDocument.dll/focused.c`:
  EF4 and segmentation activation exports with references and hash provenance.
- `myscript/sdk/`: original SDK files used for static inspection.
- `myscript/ghidra-projects/`: retained analysis projects.
- `myscript/scripts/`: local inspection and reference scripts.
- `myscript/output/`: focused exports, reports and comparison fixtures.

Repository-owned portable code is in `../src/InkRecognition/` and related
libraries. Checked algorithms and remaining integration boundaries are
documented under `../docs/`. Retaining a decompiled file does not establish
that the entire engine or Russian cursive recognizer has been implemented.

The relocation preserved all7993 files and their sizes. SHA256 was checked
before and after for all four complete `decompiled.c` exports. Local scripts
and repository documentation now use `research/myscript` paths. Existing
historical reports may contain the original location as provenance.

The readonly EF4 helper export is preserved locally at `myscript/output/Math2EF4HelpersStage4/MyScriptDocument.dll/` with function lists and SHA-256 provenance. It is analysis evidence outside build artifacts; these helpers are not yet a complete runtime EF4 implementation.

The readonly completed-hypothesis caller036F20 export is preserved at
`myscript/output/Math2CompletedPostprocessCaller/MyScriptDocument.dll/` with
references and byte-hash provenance. It supplies analysis evidence for the
attention reconstruction, activation and EF4 request/source argument chain.

The full038610 driver export and references are retained at
`myscript/output/Math2DecoderDriverComplete/MyScriptDocument.dll/`, with
SHA-256 provenance from the readonly headless export. Its decompiled types
remain analysis hypotheses that must be checked against native instructions.

The byte-vector reserve/append-zero helpers040130/0401B0 are preserved at
`myscript/output/Math2MaskVectorHelpers/MyScriptDocument.dll/` with references
and SHA-256 provenance for the correction-mask initialization analysis.

The readonly nonempty-source correction and matching helpers040CD0/0409F0,
0403E0/040670 and040270 are preserved under
`myscript/output/math2-existing-source-correction-20261018/`,
`myscript/output/math2-existing-source-matching-complete-20261018/` and
`myscript/output/math2-existing-source-column-20261018/`. Each contains the
complete focused export, references and root SHA-256 provenance. These local
analysis files remain excluded from Git and releases.

The readonly027E20/028070 symbol/result publisher export and references are
preserved at `myscript/output/math2-result-transport-20261018/`. The same folder
retains the02AF50 caller extracted from the complete local Document export,
with source/native-region SHA provenance. Symbol40-byte input rows and the
separate32-byte freeze rows do not establish the missing decoder-to-source
transport join or native document acceptance.

The complete first-result copy, row initializer, scheduled worker and outer
aggregate functions are preserved at
`myscript/output/math2-first-result-copy-20261018/`. The folder includes eight
functions extracted from the full Ghidra export, native disassembly and SHA-256
provenance. Its evidence requires both new-pool string interning and copied
source ranges; the isolated metadata/string copy is not a complete transport.

The CB8 pending-tag allocator, constructor, late tag resolution, old/new tag
collection and stale-selection removal helpers are preserved under
`myscript/output/math2-cb8-tag-owner-20261018/`, with ten complete focused
functions, disassembly and SHA provenance. Temporary IDs start2 and are
separate from registered Ink IDs. This evidence does not create a native
math-field/C23 owner or original-page acceptance. The checked source-aware
first-copy runtime now separately owns both new-pool text and actual C79 ranges.
