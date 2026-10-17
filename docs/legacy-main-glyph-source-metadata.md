# Owned ordinary glyph-source metadata

`LegacyOwnedMainGlyphSourceMetadata` implements the nonnegative ordinary
`21B.V140 -> 1AA470` source-export branch used by `217670`. It binds one actual
compact materialization, source geometry, residual-source overlay, graph and
original chronology. It does not refresh the glyph pool or claim that the final
string wrapper is complete.

`Read` retains two distinct identities: the requested owner reference and its
original base reference. A higher kind-0 source owns merged sampled ranges while
its original-last reference still selects the original acoustic group. It does
not create an acoustic group for the merged ranges. A glyph-alternative ordinal
is refused in either identity.

Base references use the existing exact source-range exporter. Higher references
resolve sampled endpoints through the same original key-record owner, sort and
coalesce the native source rows, and preserve both fractional endpoint bytes.
Original key-record indices are not current reordered physical ordinals. The
portable allocation contains all `24*N` source-reference bytes followed by all
`2*N` residual bytes, matching the allocation in `217670`. Copies and read-only
value records do not expose mutable source storage or substitute native pointers.

The supported scope is a fresh ordinary source with no capture, trace or
collection metadata. Negative/protocol arcs, missing source membership and
foreign numerically identical owners are refused. Native allocation failure and
the mutable `217670` descriptor/pool lifecycle remain separate.

## Verification

`tools/LegacyMainGlyphSourceMetadata.Probe` runs the actual CASE decoder and
materializer, then reads every base arc and diagnostic higher references created
by the real source overlay. These diagnostic merges are not claimed to be
selected-trace residual events. Its independent Python auditor reconstructs
range-cache matching, original-last identity, source-key/remainder conversion,
sorting, coalescing and the complete 26-byte-per-reference storage.

The single synthetic raw trajectory plus nine pinned recorded word tracks
exercise 2,646 queries, 1,417 higher-source reads, 4,063 source records and 2,316
nonzero endpoint bytes per runtime. Eight ownership/input guards run per input.
Managed and Windows NativeAOT captures are compared byte for byte. The recorded
dataset has no pen-up metadata: each track is explicitly treated as one
unsegmented path. Expected transcriptions are never read by this test. These
checks establish source-export behavior, not recognition accuracy.

Run the probe with AK, LK, engine profile, separation profile, case data,
publication data and an output JSON path. Append the two embedded line models,
the private pinned corpus manifest and word index `0..8` for recorded inputs.
Run `audit_output.py CAPTURE.json` on every capture. Private coordinates and
captures are not repository assets.

`tools/handwriting/audit_main_glyph_source_metadata.py` statically verifies the
pinned PE, ordinary V140 vtable binding, base-key forwarding, kind-0 higher
branch, source/remainder output and `217670` 26-byte allocation layout. It reads
vendor bytes without executing them. Pinned function intervals:

* `1AA470..1AAA60`: SHA-256 `622eeeff6e994ec9ccb864f33e15fd66a88008d4920e6f7de3312bf523b7dc5b`.
* `217670..217DB0`: SHA-256 `8321a0ed9770f4e6f4ff036643ff8d1490ad6989dd616084b739dd5a2c0b176b`.
