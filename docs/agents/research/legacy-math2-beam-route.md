# Math2 native decoder route: static findings

This is a read-only continuation of engine analysis. The independent app math
recognizer still uses greedy decoding; no native beam or grammar implementation
is claimed by this investigation. The existing14-expression audit remains
10/14 after its stated formatting comparison. No labels enter this analysis.

The Math2 decoder implementation is in **MyScriptDocument.dll** for this SDK
build. Finding generic ONNX beam-search operator strings in MyScriptMLOrt.dll
does not establish the decoder used by the Math2 resource.

The actual EED setup0450C0 selects a width as follows: positive signed request,
otherwise positive signed saved owner90, otherwise1. The selected width must
be at most16; larger values report native error3080003. Owner178 equal to zero
creates EF1 with a64-byte initialization packet; other modes create EF2 with
an88-byte packet. The resource's decoder mode0 does not by itself prove that
every caller uses beam width1. The actual property/request producer still
needs to be connected before choosing an app setting or changing recognition.

EF1's factory table1FD118 and primary table1FD150 are distinct. Constructor
04A2F0 initializes state through04A1F0; initializer04A370 retains network inputs
and resolves model output indices through049630. Its callbacks04A710,
04B9E0 and04C930 manage tensor/state execution and retrieval. Those callbacks
are not interchangeable with a generic top-k sort over app logits.

The EED caller047C50 loads its retained driver at owner220 and invokes V28.
The EEE primary table1ED930 binds that slot to03A260. That approximately
970-line driver and its candidate/scoring helpers remain unported, alongside
the actual constraint producers and tensor-state lifecycle. A new generic
beam search would be a separate algorithm, not evidence that this native
driver had been restored.

`tools/handwriting/audit_math2_decoder_route.py report.json` verifies19 exact
instruction pins,10 descriptor/table bindings and three full PE regions
without executing any SDK DLL. The additional full Document export and focused
function extracts remain in the external build-artifact directory. This audit
changes no model, recognition output, application default or accuracy metric.
