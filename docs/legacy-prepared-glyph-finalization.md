# Prepared glyph finalization

`LegacyPreparedGlyphFinalization` ports the complete nontracing post216BC0 part of2173E0. It updates one actual mutable274 pool record and reuses the existing13AF30 symbol update. The output of216BC0 is explicitly prepared input: this API does not determine a handwritten symbol, case variant, confidence, component count or final winner.

The input snapshot contains the216BC0 success flag, signed remaining scratch cursor (initially256),256 ushort AL/synthetic labels, component flag, external-variant byte, raw32-bit score fields0C/10, signed cost14 and publication word06. These are symbol IDs and protocol labels, not Unicode UTF-16 code units. Successful snapshots own their scratch copy. Failed outputs do not consume the glyph reference or row72. The supplied72-byte2498 row contributes only its mismatch byte3D at this stage.

The native order is:

1. If216BC0 failed, returnfalse without touching the pool.
2. Negative cursor with a nonzero component flag keeps scratch0. Otherwise cursor<256 selects scratch[cursor]; cursor>=256 selectsFFFF. Negative cursor with component flag0 would read before native scratch storage and is explicitly refused by the portable endpoint.
3. Compute signed32 `256 - cursor` with native wrap. Call13AF30 with the component flag as argument6 and external variant as argument7. These bytes are different. Its rejected macro count>=8192 returnsfalse before score or flag writes.
4. Preserve raw score0C/10 bits and cost14. Set only glyph flags04.bit3 from row72.byte3D; write publication word06. Preserve descriptor00, unused bytes, payload ownership and next-reference20.

The original pool's40-byte rows and snapshot owners remain distinct from WORD276120-byte linked-reference objects. No pointer-shaped source identity, character box or text is fabricated. A finalizer exception is not a transaction: cancellation after the symbol update may leave that update in the pool. Before-entry cancellation and unsuccessful producer/symbol updates leave existing records unchanged. No native cancellation behavior is asserted.

Trace2368 is unsupported. Native2175D5 uses a source stride of0x288 (648bytes), then copies640bytes in five128-byte iterations into an existing optional payload. This requires its real payload owner and allocation lifecycle; a guessed empty payload is not used.

The independent Python oracle covers512 full40-byte cases, including count zero, cursor256/257, negative cursor+component1, int32 count wrap, macro overflow rejection, external kind4, NaN/infinity/sign-bit score transport, scratch ownership and neighbouring records in the same pool. The probe also verifies immutable owner snapshots and cancellation/reference/extent/trace guards. `audit_glyph_finalization.py` pins the actual instructions and pool dispatch to the archived MyScriptText SHA256 without executing it.

Still missing for the ordinary main-result pipeline:21FFD0's full compact-path-to72-byte row producer;216BC0 (including216860 case/variant and frontend1B8/1C0 score producers);217DB0 residual reassociation;218560 deduplication/forced-duplicate/link repair and final head/tail publication. The predecessor cursor and this finalizer do not make21FFD0 or218560 complete.
