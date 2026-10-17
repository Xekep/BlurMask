# Global source geometry and scalar callbacks

`LegacyFreshMainGlobalSourceGeometry` binds the actual global policy, confidence
session and its original transition owner. It keeps actual group allocation,
descriptor, assigned line and physical ranges. Guide lookup reads0/4 of the
actual40-byte guide indexed by line, not the descriptor ordinal or the combined
normalization frame. Win64 caller1ABCDE explicitly passes descriptor+10 inR8D
to source218.V40=168B90; decompiled C omitted that argument.

Bounds come from the SAME live1AACD0 sidecar in the owning word transitions.
They are not recomputed from points or copied from the immutable pre-transition
graph. The added internal getter is serialized and copies one value without
allocating a complete geometry array per callback. Stored ranges and global120
source references retain their actual owner and are copied on public access.
Ordinary kind0, sourceKind135 collectionC1=false, top-mark and alphabet flags use
their existing pinned producers. Higher source overlays remain separate.

`LegacyFreshMainGlobalSourceSymbolCallbacks` restores ordinary1A8 fallbackFFFF
and bounded1A0. Pending metadata is consumed BEFORE previous-source/endpoint
geometry: NEW00C860 WORD8=10h becomes config1 and graph1D9=true. Each actual
kind3 I32-7 control therefore writesFFF9, notFFFB. Its highshortFFFF and zero
prefix5 yield outputflagsC/D=0. Multiple controls preserve order and native
cursor exhaustion: after0, the cursor still decreases without an out-of-bounds
scratch write. A typed result retains only actual written symbols.
Twenty-four separately prepared cursor/count controls cover exhaustion through
4096 controls without constructing a synthetic source-metadata owner.

With no metadata, endpoint returns, kind0 FFFC, assigned kind2/graph159=0 and
ordinary kind1/request2C==2 are supported. General kind1 geometry, custom or
known-input metadata, higher arcs and trailing/unconsumed fallback metadata
explicitly refuse. The factory requires a final ordinary descriptor and real
nonempty local allocations, matching19F850 consumption/reset of pending1F0.
It does not manufacture an empty fallback for an unrepresented trailing state.

The probe uses eight full owning raw preparations and631 actual arcs, exercising
20192 explicitly prepared request64 controls and480 real pendingFFF9 writes.
Independent checks decode raw descriptors/guides/context controls and original
source-reference bounds. Separately prepared actual-key kind6 callbacks cause
102 live bbox updates, proving the adapter observes the same mutable owner;
these packets are not claimed to come from the native scheduler. NN evaluation
count remains unchanged. No glyph sequence, UTF8 or OCR quality is claimed.

Final Managed and Native AOT captures match byte for byte (SHA256
`8c397cf7ef10c74c013b35ac04ed36daf6e3682aeed2051dfdf0ca59d1fc3538`).

The managed/native probe takes the same9 resource/capture arguments as
`LegacyMainGlobalResult.Probe`. Run its independent audit with the capture path:

```powershell
python tools/LegacyMainGlobalSourceCallbacks.Probe/audit_output.py E:/BlurMask-build-artifacts-20261014/main-global-source-managed.json
python tools/handwriting/audit_main_global_source_callbacks.py --output E:/BlurMask-build-artifacts-20261014/main-global-source-static.json
```

All builds/artifacts/TEMP/TMP are on E. Vendor DLLs are only read as static PE
data; no library loading, vendor execution, GUI or default/UI change occurs.
