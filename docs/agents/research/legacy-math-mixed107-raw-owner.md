# Owned Math raw215 architecture02 storage

`LegacyMathMixed107RawResource` is a new, independent owning loader for the six
FEA0/version107 architecture02 envelopes in the fingerprint-bound
`models/math/math-ak.res` resource. The native evidence and portable wire
reference are in `legacy-math-mixed107.md`. This is raw type **215** storage,
not evaluator type212 or213. Existing architecture12/Source247 network guards
and APIs are unchanged. No application recognition route is changed.

The loader copies signed little-endian int16 first-layer weights and signed
int8 second-layer weights into distinct typed arrays. It retains the complete
258-byte scale vector, padded row values and absolute-origin alignment bytes.
Both complete matrix extents, exact observed dimensions and kinds, enclosing
resource frames, all six model budgets, scales and the complete resource
fingerprint are checked before any matrix payload is copied into typed storage.
The initial bounded resource capture is necessarily made before those checks.
Limits are 3,685,881 source bytes, widths 2..4096, 1,048,576 total matrix elements
and 1,048,576 total envelope bytes; checked arithmetic precedes payload copies.
The pinned six-model domain is stricter than a generic native wire parser:
other resources, architectures, middle layers, quantizers, zero scales and a
leading scale other than 1 are refused. This does not claim native malformed
data equivalence or a complete 13A5 slot parser.

The root owns all typed storage. Model handles retain the root and immutable
metadata; `Capture` checks root liveness and copies the complete payload while
holding its gate. `Dispose` takes the same gate, is idempotent and releases the
root's storage references. An in-flight capture completes or throws before
disposal obtains the gate; no backing array is exposed. Historical snapshots
have no root reference and survive root disposal and collection. Their copy
methods return independent arrays. This is an explicit managed ownership
contract, not a recovered native lease ABI. Arrays are released to GC, not
securely erased. Concurrent caller mutation during initial capture can cause
fingerprint refusal; callers must synchronize their source buffer when a
successful load is required.

Cancellation is checked at entry, each 16KB source/hash chunk, each model's
preflight, each 2048 matrix elements, scales and publication. Snapshot copying
uses 2048-element chunks and checks cancellation again before return. An
internal diagnostic observer receives only phase/index/offset metadata, never
payloads or owners. It runs outside owner locks; throwing or canceling there
prevents publication. The public factory has no observer argument. No tensor
or vendor object is acquired or released.

The NEW probe and independent Python oracle verify all six actual envelopes,
every signed weight and padding byte, scales and metadata; they do not compare
only a digest or a selected sample. They also verify the combined digest,
777 independently reconstructed cancellation/throw boundaries, malformed
headers/dimensions/architecture/scales/fingerprint before payload copies,
returned-array mutation, post-capture caller mutation, diagnostic reentry,
concurrent capture/dispose, historical GC lifetime and 3439 ordered checks.
Managed and NativeAOT captures are identical. The Python reference dependency
is hash-pinned and separately parses the source resource.

Example verification:

```
dotnet run --project tools/LegacyMathMixed107Raw.Probe -c Release -- models/math/math-ak.res capture.json
python tools/LegacyMathMixed107Raw.Probe/audit_output.py models/math/math-ak.res capture.json --reference tools/handwriting/math_legacy_mixed107_reference.py
```

Feature extraction, activation construction, evaluation212/213, masked score
policy, candidate ranking and application integration remain excluded. No
recognition improvement, Russian support or full Math operator closure follows
from these storage tests. Vendor binaries are only inspected statically.
