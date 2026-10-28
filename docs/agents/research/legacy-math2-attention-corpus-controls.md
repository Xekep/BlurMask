# Diagnostic attention controls on handwritten formulas

`LegacyMathAttentionCorpus.Probe` compares explicit force-attention off/on
with the existing public greedy decoder on the same research corpus. It runs
real encoder/decoder graphs using official CPU ONNX Runtime, never a vendor
SDK DLL. It is an optional diagnostic, not the restored native beam search or
an application route.

On the existing 14-expression corpus, both controls matched 10/14 expected
LaTeX strings after formatting-only normalization. All 14 raw symbol outputs
were identical with force off/on; force-off also matched the old decoder.
The unchanged old baseline still passes four incorrect results through its
confidence gate. This experiment establishes no quality improvement or
recognition acceptance threshold.

Both controls use the same explicit 128-step argmax cap and stop at first EOS.
They do not implement the native completed-hypothesis continuation, search
termination, maxstep fallback or final hypothesis selection. Each inference
is repeated to check determinism. Expected labels are used only by the
post-inference auditor; it validates symbols against the raw vocabulary and
reports measured accuracy without asserting an accuracy golden.

Windows Managed and fresh NativeAOT captures matched exactly:
SHA256 `802e06c3b15512dd6a33f54994184586228f4001314034f00ca9fb0eaa5db4cf`.
Baseline timing reports are separate and are not byte-equality artifacts.

Reproduce after exporting the model and fetching official runtime as described
in [tensor-state checks](legacy-math2-mode0-tensor-state.md):

```powershell
dotnet run --project tools/LegacyMathAttentionCorpus.Probe -c Release -- model.ikrmath models/math2/math2-unwrapped.res /path/to/onnxruntime.dll /path/to/corpus/manifest.json /path/to/control.json
python tools/LegacyMathAttentionCorpus.Probe/audit_output.py /path/to/control.json models/math2/math2-unwrapped.res /path/to/corpus/manifest.json
```

Use the existing pinned `tools/handwriting/fetch_math_ocr_corpus.py` download for the
optional research dataset. Its license and scope remain those of that tool;
the dataset and generated captures are not bundled with the library or app.
