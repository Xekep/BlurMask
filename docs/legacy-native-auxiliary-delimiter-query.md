# Auxiliary delimiter traversal

`LegacyNativeAuxiliaryDelimiterQuerySource` ports the no-extra-cost branch of
`290.V48`: the tiny `2392E0` entry selects the single cache at `290+41BE8` and
tailcalls `239100`, which invokes `2388E0`. It is a different traversal from
normal `V40/237610`; it never performs `2382B0`'s outer context prefix.

`239100` refreshes the last-source-node test from frontend `222.V88`.
The actual getter `151FC0` forwards to graph `V20/1A73C0`, so its argument is
the owned candidate graph's node count, not a glyph/physical-stroke count.
The public constructor keeps this producer explicit. Source tag, accepted
skip, parameters `1C/20` and flags `24/26` own one cache with eight levels of
128 complete 32-byte records. Ancestor cycles/depth are bounded as in native
code. New empty levels preserve the native duplicate initial backend read.
The parameter-change block explicitly sets the **outer** cache tag to-1
(`2391DB: mov qword ptr [r8], -1`). Consequently the next same-query page
replays the backend from zero once, then later pages resume the live cursor.
Literal first/second/third-page backend logs test this extra replay separately
from output equality; it must not be optimized away.

The backend must be the aggregate's auxiliary reader. With the full factory,
case wrappers belong inside the aggregate below `290`. No source-ID stamping
is added after this wrapper. The additive delegate constructor on
`LegacyNativeAuxiliaryWordQueryPages` allows the actual outer `V48` source to
feed the existing request/pagination/current-terminal-injection consumer. Its
old aggregate constructor forwards to the same reader and retains its old
behavior.

## Record and context transformations

`2388E0` first overwrites low kind bits with the inherited kind, unlike the
normal traversal's initial kind-combination operation. It recurses through
protocol/ZWNJ/ZWJ records. `FFF7` and the allowed `FFF8/FFF9` branch drop the
current context leaf and select kind1; `FFFB/FFFC` also drop it and update kind.
`FFF8/FFF9` are skipped on the actual last-node condition. ZWJ drops its leaf;
ZWNJ keeps it. An ordinary record needs flag `10` to enter terminal handling,
and drops its leaf unless request flag `20` is set.

After recursive children, handled records are emitted only when their tag's
terminal bit is set or request flag `20` permits them. The output recomputes
tag-byte5 bits1/2/7, carries cost with signed32 wrap and length with byte wrap,
and preserves the rest of the full record. ZWJ output receives bit2 and loses
the terminal bit. Terminal output order is postorder, not a flat list of the
backend's ordinary symbols.

Protocol branches that natively dereference a null context leaf are refused
instead of dereferencing a pointer. Repeated-tag graph/cache semantics are
preserved by mutable per-level cached records; this is not an assertion that
an arbitrary DAG is equivalent to fresh depth-first enumeration. The tests
cover finite trees and a concrete recursive resume branch, not every unsafe
native pointer configuration. Side-cost buffers and the custom source
extension remain unsupported. Failure/cancellation invalidate the instance;
queries are serialized and a failed instance cannot be reused.

## Checks

```powershell
python tools/LegacyAuxiliaryDelimiter.Probe/reference_controls.py E:/BlurMask-build-artifacts-20261014/auxiliary-delimiter-controls.json
dotnet run --project tools/LegacyAuxiliaryDelimiter.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/auxiliary-delimiter-managed -- E:/BlurMask-build-artifacts-20261014/auxiliary-delimiter-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```

The independent Python oracle covers 256 finite trees, producing 3359 full
32-byte records and exact stripped context paths. Separate literal controls
check 777 flat paginated records across the 128-record boundary and six
recursive resume rows at capacities1/2/128. Backend failure, cancellation and
side-cost guards are also checked in Managed and Windows Native AOT.

The real-resource test inspects all 895571 packed trie nodes and the actual
auxiliary symbol map. Its mapped auxiliary edges have no terminal edges in
this direct resource. A real parent with a mapped auxiliary edge therefore
produces zero records and exactly two initial backend reads. That zero result
is also consumed through the new outer-reader constructor. A nonempty native
auxiliary result is **not** fabricated to make this resource test positive.
Current-terminal injection is a separate, nonempty producer in204290.

The unchanged auxiliary-page regression contains 512 independent streams,
1606 exact requests, 1266 full output-buffer pages, 18 lifecycle/extent guards,
and32 real terminal-injection pages. The complete factory, case routing,
whole-word winner and acceptance confidence are separate work.
