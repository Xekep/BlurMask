# Recognition engine research

Local MyScript static-analysis materials are stored in `myscript/`, outside
build artifacts. The entire directory remains excluded from Git and releases.

- `myscript/output/MyScriptText.dll/decompiled.c`: text engine export.
- `myscript/output/MyScriptEngine.dll/decompiled.c`: common engine export.
- `myscript/output/MyScriptMLOrt.dll/decompiled.c`: inference integration export.
- `myscript/output/Math2FeaturesStage4/MyScriptInk.dll/decompiled.c`: ink export.
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
