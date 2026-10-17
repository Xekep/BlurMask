# EF4 request policy and remaining helper graph

The decoder's `attn_seg` output establishes that036F20/EF4 is required. It does
not establish which internal EF4 correction branch is selected.

0450C0 copies byte4 of its actual request configuration to EED.B1.0359F0 copies
packet40..4F to EEE50..5F; since the packet begins at EED70, this preserves the
byte at EEE51.036F20 supplies that byte as EF4's ninth argument.042100 tests it
at042406. A three-graph model, no fourth constraint model, or a forced-attention
flag does not prove this request byte is zero. The separate cold-request audit
now establishes byte1 for the concrete fresh, unmodified EEB caller:027830
stores word0101 at EEB3C, while the request starts at EEB38.027970 passes that
request to045460. With a cold source and no retained decoder,0450C0 therefore
selects the nonzero correction branch. This excludes external request mutations
and warm/copied/raw-initialized prepared decoders; it is not a universal host
default. See `audit_math2_cold_correction_request.py` (three producer regions,
13 pins/eight bindings plus this consumer proof).

With the byte zero, EF4 invokes041020 with its final argument true, then the
shared041A30 and041CF0 tail. With the byte nonzero, the source sequence matters:
an empty sequence invokes041020 with null source/final argument false; a
nonempty sequence clones the segmentation tensor, runs040CD0 and041020 on the
clone. Both then enter041240,041EC0,041660 and041BA0 before the shared tail.
These steps can alter downstream four-byte units, segment association,
auxiliary98/A0 data and final score. Concatenating primary U16 symbols does
not substitute for this work.

The freshly preserved Ghidra bodies are in
`research/myscript/output/Math2EF4HelpersStage4/MyScriptDocument.dll/`, outside
build artifacts, with SHA-256 provenance. The readonly export contains all
eight helper bodies above. They are analysis inputs, not assertions of runtime
integration. Decompiler types and missing floating arguments must be checked
against instructions before any score calculation is ported.

`audit_math2_ef4_control_flow.py` checks four native code regions,22 instruction
pins and the request-byte offset arithmetic. Existing completion and attention
tests do not execute these helpers. Required next work is to connect actual
request policy, preserve both branches and failure prefixes, independently
verify their numeric and structural transformations, then join final ordering
and output. No full-engine or handwriting-quality claim follows here.

The finite prepared041CF0 fresh and masked score kernels are verified
separately. They do not execute the preceding correction helpers or select an
actual request policy. Prepared byte-zero controls cannot stand in for the
fresh EEB byte-one route.
