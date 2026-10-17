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
