# Math2 restriction-map and special-cost mask provenance

The native decoder has two distinct expansion routes selected by EEF's `C8` pointer. A cold EEF starts with a null pointer; setting a symbol-knowledge source builds and transfers a separate byte map. Ordinary EEE reset preserves that map. These facts do not implement the beam decoder or the CPU exp/log routines used by its scores.

## C8 lifecycle

Addresses refer to the fingerprinted `MyScriptDocument.dll`, image base `0x180000000`.

| Operation | Native route | Ownership effect |
| --- | --- | --- |
| Cold EEF construction | `03B300`, clear at `03B37D..03B3B3` | Sets `C8 = NULL`. |
| Ordinary EEE reset | EEE `V18 = 035E60` -> EEF `V18 = 03B920` | Releases/reset score and auxiliary state; preserves `C8`. |
| EED knowledge setter | EED service `+0x18 = 045BB0` | Releases previous EED `230`, retains the supplied SK object, and forwards the supplied object to EEE `V20`. |
| Clear map | EEE `V20 = 035EB0`, null argument | Tail-calls EEF `V20` with null. |
| Build map | EEE `V20 = 035EB0`, nonnull argument | Resets EEE fallback label `78` to `FFFF`, calls EED service `V30 = 0463E0` using the retained actual source packet and EF3 table, `param5 = 1`, and fallback-label output at EEE `78`; passes the returned allocation to EEF `V20`. |
| Adopt allocation | EEF `V20 = 03BA20` | If pointer identity changes, frees old `C8` and adopts the supplied pointer. Returns true, including for null. |

There is no intermediate map clone in the adopt operation. Reusing the same pointer does not free it. `035EB0` does not reject a null `0463E0` result before adoption: it can clear the previous map and report the success returned by `03BA20`. This audit does not introduce a different managed allocation-failure policy.

Fresh EEB constructor `0277B0` initializes its SK field `60` to null. During `027970`, the EED SK setter is called only when that EEB field is nonnull. Thus a cold source, fresh EEF, and fresh unconfigured EEB retain `C8 = NULL`. EEB clones its EED source; copied or already prepared decoders can retain a configured map. A new EEB wrapper alone does not establish the cold condition. `027C20` is the separate attachment path accepting a type `13E` source and retaining it at EEB `60`.

## Configured map producer

`0463E0` allocates one byte per decoder vocabulary entry. Its count comes from the retained source packet's unsigned word `+0xA`, EED `1E2`, which is **297** for this resource. The full source alphabet has 299 entries, as described in [the EF3 audit](legacy-math2-observable-token-gate.md); those counts are not interchangeable.

In the EEE caller's `param5 = 1` branch, non-observable EF3 classes get byte `1`. Observable classes get membership results from the supplied SK object through service `13E` and helper `043FA0`. Fraction/slanted-fraction/vertical-operator special labels use the native fixed category table and its membership route. The first enabled ordinary observable label can populate EEE `78` as fallback. The separate `param5 = 0` producer branch starts with byte `1` and clears category-dependent entries; EEE's setter does not select that branch.

The audit fingerprints the fourteen 72-byte native category rows. It does not implement arbitrary SK membership or calculate general configured maps from a caller's category policy. A nonnull map is an owned allocation with its own lifecycle, not an implicit all-ones default.

## Expansion dispatch

Driver `038610` checks actual `EEE80->C8` at `038FE6`:

- Null selects the direct path, which offers all vocabulary classes from the EEF cost array to `0351E0` and fills their 48-byte candidate records.
- Nonnull calls `036360` at `03900E`. That function builds both an observable permitted/alias-alternative heap and a raw all-class heap, sorts them, and transports alternate semantic-label IDs onto disabled observable candidates before merging them into the output heap.

`036360` does not simply discard every class that EF3 classifies false. Structural classes such as `<frac>` and `<sup>` remain legitimate candidate classes. Its special-label fallback includes native `-`, `/`, and `V` lookups, then EEE `78` when a direct alias is absent. Heap retention, tie order, score pruning, and complete-hypothesis ranking remain separate decoder algorithms. The two later prefix scans in `038610` test whether **any** prefix token is observable before calling `037430`; they do not require all prefix tokens to pass an observable-token filter.

## B0/B2/B4 cost-mask source

Resource reader `0486B0` performs actual alphabet lookups for `[bos]`, `[eos]`, and literal ASCII space, storing their unsigned 16-bit IDs at EED `160`, `162`, and `164` (`048C85..048CC8`). EED's EEE construction packet begins at `70`; EEE's raw EEF packet is at its input `+0x50`, so that packet begins at EED `C0`. EEF raw initializer `03B300` transfers input `+0xA0` to EEF `B0`, carrying the same packed IDs. This is the packet lineage established by the [score-channel audit](legacy-math2-score-channels.md), rather than a second alphabet or guessed control labels.

For `math2-unwrapped.res` (SHA-256 `2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a`), the IDs are:

| EEF field | Alphabet lookup | Actual ID |
| --- | --- | --- |
| `B0` | `[bos]` | 0 |
| `B2` | `[eos]` | 1 |
| `B4` | ASCII space | `FFFF` (absent) |

After score preparation, `03C690` writes the exact F32 maximum finite bit pattern `0x7F7FFFFF` at the `B0` class for each active beam row in **E8 costs**. It does not apply that write to the separately retained D8 log-probability array. Its additional `B2/B4` masking block is guarded by `B4 != FFFF`. That whole optional block is skipped for this actual resource, including its first-step and retained-space-history subbranches. This establishes the mask producer and branch selection; it is not evidence that EOS is generally disabled or that search should stop at the first EOS candidate.

## Reproduction and remaining scope

```powershell
python tools/handwriting/audit_math2_restriction_map.py E:/BlurMask-build-artifacts-20261014/joined-math2-restriction-map-static.json
```

The audit checks fifteen whole native regions, 55 instruction pins, six service bindings, the fixed category-table fingerprint, literal lookup strings, the complete raw resource fingerprint, and actual model vocabulary IDs. It performs static PE/resource reads only. There is no vendor DLL execution, ONNX Runtime session, GUI, inference, or runtime change.

Remaining work includes arbitrary SK membership, actual configured-map caller policy outside the concrete EEB attachment route, native candidate heap/ordering and semantic-label transport, CPU exp/log dispatch, and completed-hypothesis collection. Cold null-map provenance closes one ownership condition; it does not make the existing greedy decoder equivalent to native beam width one.
