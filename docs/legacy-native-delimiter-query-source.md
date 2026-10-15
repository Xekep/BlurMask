# Native delimiter query source

`LegacyNativeDelimiterQuerySource` restores the ordinary, no-custom-callback
`290.V40` (`2382B0`) query and its recursive `237610` traversal. It sits above
the aggregate source, consuming protocol edges before their labels reach the
acoustic alphabet. It does not filter a query page to hide unsupported labels.

## Placement and initialization

`20C9D0` constructs `28D`, places `290` around that aggregate, and subsequently
places each selected dictionary (with its optional `292` case wrapper) inside
`28D`. The order is **290 → 28D → 292 → 2B7** when case handling is enabled.
The backend delegate therefore supplies the aggregate below `290`; a case
wrapper outside this class would change the native order.

`237380` calls frontend `222.V18` (`150780`) to get the selected `247`, then
`247.V40` (`1D60C0`) to read its signed coordinate mode at runtime `+80`.
`1D4950` produces that field from the serialized parent prefix. The bounded
`ReadV10dCoordinateMode` reader restores this resource path. The checked Russian
resource has mode **2**. This is distinct from source kind **0x135**, stored at
runtime `+78`. Constructor default `290+2E == FFFF` selects the initial protocol
root `FFFA` for modes 3/4 and `FFFB` otherwise. Actual alphabet labels for ZWJ
and ZWNJ are resolved from the supplied alphabet.

## Query behavior

There are eight rotating outer query slots. Each has eight recursion levels,
each reading blocks of 128 complete 32-byte records (`236F30`). The cache uses
the full 64-bit source tag, global accepted skip, parameters `1C`/`20` and flags
`24`/`26`. A slot collision restarts traversal; pagination still skips the
already accepted rows. Ancestor-tag cycles and depth beyond eight are stopped
without inventing a successor order.

`237610` skips ZWJ, follows protocol/ZWNJ edges in backend order, carries native
kind/bit-3 transitions, adds costs with signed-32 wrap and lengths with byte
wrap, and preserves the rest of every record. `FFE3..FFEA` are excluded from
the protocol-range test. Protocol `FFF7`/`FFF9` is skipped when parameters
`1C == 20`. Output capacity and backend block capacity are separate.

The literal native do/while is preserved: the first read of a newly empty
level calls the backend twice at skip zero; another read of that exhausted
query calls it once. The probe checks callback logs, not only emitted output.
Backend failure returns `-1`; exceptions, cancellation and failure invalidate
the preparation. A new instance is required after failure.

After traversal, `2382B0` prefixes the selected control root using literal
`236E90` concatenation. It does **not** apply `1F7FF0` context normalization.
Flag `100` transports the prefix in record `+24` instead of grafting `+08`.
The explicit null-query-context branch writes null for a nonempty suffix.
`LegacyNativeWordStartupRequestFactory.GraftOrdinaryInitialContext` now accepts
the already-owned `FFFA(root) → single ordinary AL leaf` form without allocating
a second prefix. Other initial paths remain unsupported by that startup helper.

## Boundaries

This is the no-extra-cost-side-table, no-custom-`49F58` callback branch. Both
require their actual owning producers before extension. The source coordinate
mode, backend source and shared context factory remain explicit inputs; this
class alone does not establish the complete `20C9D0` factory policy.

Memory owned by query slots is bounded to 8 × 8 × 128 records, plus at most
512 output records per call. The shared context factory has its own ownership
and allocation budget. Literal context paths are bounded to 4096 nodes in this
checked implementation. This is not a concurrency-capable backend: calls are
serialized and reentrant calls are rejected. Finite tree/cycle controls verify
the native state machine; no claim is made that arbitrary DAG traversal equals
a fresh depth-first enumeration when cached repeated tags are reused.

The checked direct ordinary `2B7.V60` is false (`05A400`), `28D.V60` (`236C20`)
ORs its selected children, and `290.V60` is also `05A400`. Consequently this
wrapper's existence does not enable `1FE740` scheduling. That decision belongs
to the outer controller, including its associated-source state.

## Verification

Managed and Windows Native AOT run the same independent Python controls:

```powershell
python tools/LegacyDelimiterQuery.Probe/reference_controls.py E:/BlurMask-build-artifacts-20261014/delimiter-controls.json
dotnet run --project tools/LegacyDelimiterQuery.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/delimiter-managed-artifacts -- E:/BlurMask-build-artifacts-20261014/delimiter-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```

The 256 finite trees independently determine full 32-byte records, literal
context paths and traversal order. They include 128-record paging boundaries,
ancestor cycles, depth-nine chains, all supported mode/flag combinations,
length/cost overflow and output capacities 1/3/17/128. The checked run has 2026
expected rows, 1541 pagination calls and 7203 backend block requests, plus exact
empty-backend logs, nine-query slot eviction, null-context output and failure/
cancellation ownership guards.

The real-resource control finds a parent with an actual mapped `FFFB` edge in
the validated Russian trie, passes its unfiltered `2B7 → 28D` pages through this
source, and independently counts the raw trie descendants. One actual protocol
edge produces two flattened labels. This test uses an explicitly unfiltered
source permission scope; acoustic masks and complete word-controller scheduling
are verified by their separate owning pipelines. No expected text, invented
winner, acceptance confidence or ready Russian AI-pen behavior is asserted.
