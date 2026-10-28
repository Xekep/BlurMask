# Reusable context to raw global WORD results

`LegacyFreshMainRecognitionContext.PrepareGlobal` shares the context's owned
immutable models/resources and creates a separate raw document and invocation.
`LegacyFreshMainGlobalPlainRecognition.Run` joins actual preprocessing, shared
descriptor/global graph/context/dictionary caches, one bounded LIVE controller,
final compact publication **before** selection and plain WORD callbacks. It
uses the real new published selection throughout, with no old-terminal wrapper.

The caller supplies bounded compact40 storage, WORD120/record40 allocator
residue,256 labels and64/72-byte stack storage. The invocation is single-use
after processing starts. Malformed storage and canceled entry leave it usable;
later failures consume it. Component-level unsupported branches still refuse.
Sources requiring more than240 logical nodes/multiple LIVE cycles are outside
the current implementation.

The result retains the actual selection, session and callback tokens. A genuine
no-winner returns `HasWinner=false`, a null session and no WORDs; it does not
allocate a glyph publisher or invent accepted empty text. Successful callbacks
provide separate raw WORD/UTF8 projections in native head order. There is no
invented separator, acceptance probability or joined replacement string.
Typed failed callback prefixes remain visible through the actual session and
tokens; raw projections are exposed only after all callbacks complete.

This is a library composition, **not accepted document transcription**. Quote
cleanup, scored alternative assembly, imported-result lifecycles, final C20/DAF
document publication and recognition-quality gates are separate. The original
raw-document IDs are not asserted to be native C51 selection-registry IDs.
The application default and experimental setting are unchanged.

`LegacyMainGlobalRawRecognition.Probe` performs32 actual invocations through
one reusable context: eight source inputs with four caller-residue variants.
It checks that all invocations retain the identical model object, validates
96 preflight/single-use guards and compares every raw glyph/WORD byte and real
source range against the independent raw-AK oracle. A separate actual124-node
input verifies the no-winner/null-session path with964 cached main groups.
All final publication/callback stages preserve their invocation's cached NN
count. Repeated variants are integration checks, not additional human samples.

Managed and Windows Native AOT agree completely, capture SHA256
`b83f0dd6911f0ead6130d0f431ea590b4a13423b66a6f9fdf6c129516a4a9551`.
This establishes the stated ownership and byte transport, not improved Russian
cursive accuracy or lower measured application memory use.

The probe takes AK,engine,separation,case,publication,line30,line50,capture,LK.
Run its output oracle with `audit_output.py AK capture.json`. `audit_word.py`
requires four genuinely distinct context invocations per source; the original
32-variant component oracle remains unchanged.
