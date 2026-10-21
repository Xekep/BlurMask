# Direct English resource loading

`NativeEnglishTextResource.Load(resource, token)` loads the committed full
`en_US-ak-cur.res` using C# only. It verifies an owned snapshot of the complete
3,783,049-byte source, SHA256
`355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927`.
No Python process, SDK DLL, on-device certificate or native ML runtime is used.
This is an adapter to the existing portable BLSTM/CTC engine, separate from the
unfinished legacy119 MAIN dictionary restoration.

The parser selects the actual13-channel adapter and its enclosed106 sequential
network. It reads the inline AL104,26 normalization floats, resampling settings,
output labels and three bidirectional layers. All67 ordered tensors, quantization
scales, row alignment and2,007,776 parameter bytes remain unchanged. The temporary
portable image is byte-identical to the independent Python exporter:2,012,905
bytes, SHA256`39418d35708502db6520fa22781614c30c7256efdb9981bf6bbf9b1b0c4f8d5b`.
It is consumed by the existing validated model reader and is not stored on disk
by the application. Only this pinned full English resource is supported; other
profiles do not silently become English.

Source length is checked before copying, the bounded owned snapshot is hashed
before parsing, node discovery is limited to4096 entries, tensor layout consumes
the whole payload, and the intermediate portable body is limited to8MiB. These
are parser/copy limits, not a process-RSS claim. Cancellation is checked before
and after ownership/loading, during resource scanning, between layers and during
label publication. Input and weights are not retained through caller aliases.

The application lazily loads this bundled English resource if no optional
English pack is installed. A separately installed valid pack keeps precedence.
Startup does not parse the resource until recognition is requested. The optional
experimental setting still selects the separate Russian global dictionary path;
automatic Russian/English language arbitration is not implemented by this change.
Neither path is a complete restored MyScript engine.

The new headless probe compares the entire C# portable image against a freshly
generated Python image and checks12 guards: malformed/foreign snapshots, both
cancellation entries, shared immutable inference and caller-buffer mutation.
All100 fixed PluColl rows match the existing English decoder exactly, including
text, score bits and frame counts. Managed and NativeAOT captures are identical.
The complete NativeAOT application passes `--ai-pen-test`, including bundled
English fallback and the separate Russian setting.

On the already opened [recorded Latin-word diagnostic](english-recorded-baseline-20261021.md)
the full-resource CTC path has69 exact words/100,40 character edits/721 characters
(5.55% CER), compared with14 exact words for the different lite Parent/FIRST
pipeline. This compares entire pipelines and resources, not an isolated causal
effect. The sample is limited, includes loanwords/proper names and may overlap
model training. No model, threshold or beam was tuned against its targets.
These results do not establish Russian or automatic bilingual readiness.
