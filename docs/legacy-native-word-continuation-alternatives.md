# Native continuation alternatives and ordinal production

This batch connects `2144f0`'s ordinal writers and the unanchored branch of `211eb0` to the existing root alternative producer, duplicate merges, and stored-row materializer. It preserves real parent ownership and symbol-context identities. It does not implement `20e6a0` stored-parent alternatives, anchored `211eb0` reconstruction, the rejection prepass internals, or a complete winner/confidence controller.

## Ordinal writers in `2144f0`

`LegacyNativeWordAlternativeOrdinals` owns a complete copy of supplied 128-byte heap storage and changes only the active slots' dword at `58`. Adjacent `5c..5f`, the rest of each active slot, and inactive capacity bytes are retained. Physical heap/slot order is supplied explicitly; the writer does not sort, select, or allocate candidate/pool memory.

Before each `20e6a0` call, the initial loop writes its shared unsigned32 cursor into every active slot and increments the cursor as unsigned32. Before each `211eb0` call, the continuation loop stores the current cursor as a dword, but advances it as `ushort((short)cursor + 1)`. This produces a zero ordinal after 65535, including across heap boundaries. `AssignInitial` and `AssignContinuation` expose their next cursor so the caller can reproduce those per-heap calls. The native loops initialize their cursor to zero; a supplied nonzero cursor represents the exact state carried from an earlier heap call. No row index is inferred from a record's source/category or cost.

The checked per-heap storage limit is 4096 complete slots, matching the bounded heap API. Initial arithmetic can produce a dword with its sign bit set; this writer preserves it, while the owned pool producers subsequently reject a negative signed ordinal instead of emulating an invalid native pointer index.

## Rejection prepass ownership

At the start of `211eb0`, the native rejection path (`210870` → `211210` → `2113f0`) runs when:

- heap index is zero and controller `22e8` bit4 is set; or
- heap index is nonzero, descriptor signed source index `30 >= 0`, and that source descriptor's header bit `20` is set.

`RequiresRejectionPrepass` reproduces this exact predicate from explicit scalar inputs. It does not invent a source header or bind an unknown source descriptor. `CreateUnanchored` requires a caller-owned callback when the predicate applies. Cancellation is checked immediately after the callback, including a false result
and the disabled-secondary-count1 branch. A noncancelled false callback returns
`Succeeded=false` before parent lookup/composition. A true callback precedes subsequent live parent lookups, so it may replace/invalidate stored parent alternatives in their actual owning container. When the predicate does not apply, the callback is not called.

The rejection algorithms themselves are not ported by this API. Passing no callback for a required prepass refuses the branch; silently skipping it would change the controller. Trace cloning is also rejected. Unsupported anchored slots and out-of-range ordinals are checked before external mutation, a bounded refusal policy rather than a claim to replay the native prepass for an unsupported branch.

## Unanchored `211eb0`: slot `3e == FFFF`

The parent node is the native function's explicit signed argument (`param3`); the parent arc is the current slot's ushort `1e`. This is not computed from slot `20`. `LegacyNativeWordParentRowLookup` returns the actual owned `LegacyNativeStoredWordRecord`, with an exact 88-byte record and the configured `secondaryCount * 40` bytes. The callback is invoked in physical active-slot order. Its record and alternatives are copied after the owning prepass and before use. No missing parent record, context, or cost is synthesized.

For each available parent alternative (`signed cost00 <= 03ffffff`):

- New cost is unchecked signed32 `current.slot10 - parent.record10 + parent.alt00`.
- New increment is unchecked signed32 `parent.alt04 - parent.record14 + current.slot14`.
- New mapped/raw IDs are the parent's alternative `08/10`, composed with the actual current slot08 suffix through `1f7e30/1f7ff0` when the suffix is nonzero. A zero suffix copies them directly.
- `18` receives the current parent arc; `1a = FF`; `1b = alternativeIndex & 7F`.
- `1c/1d = FF`, `1e = 0`.
- `1f` preserves its prior high three bits and takes the current slot21 low five bits.
- `20/21 = FF`, `22 = FF`; remaining `23..27` bytes retain their previous pool contents.

The first unavailable parent alternative stops copying. Every remaining destination alternative receives only `cost00 = 04000000`; its old increment, identities, metadata, and reserved bytes remain untouched. Negative costs and signed32 overflow are preserved. New sums are not clamped or re-filtered by a guessed threshold.

This branch only writes the secondary candidate pool (`2430`). It does not create or merge primary alternatives. The configuration retains actual default counts2/2. The corrected `LegacyNativeWordAlternativeOrdering` continuation phase therefore accepts an absent unused primary pool, applies only `1f9830` on duplicate secondary records, and leaves an explicitly supplied primary snapshot unchanged. Continuation materialization starts at destination `230a`, so `213410`'s `destination < 230a` predicate omits primary stored blocks and transports the secondary blocks.

Heap input bytes are unchanged. Context insertion and the external prepass are not transactional; completed external mutations can survive cancellation or an exception. Callers serialize access and keep the current heap/configuration and a fetched parent row stable during symbol mapping. The parent provider can update its owning stored container in the prepass, then exposes that updated live row to later lookups. The API does not imitate native addresses or dereference opaque pointer bytes.

## Independent controls and actual resource integration

`tools/LegacyWordContinuationAlternatives.Probe/reference_controls.py` uses the frozen independent functional symbol-path oracle and a separate byte-slice continuation writer. It generates 256 complete ordinal controls and 512 complete secondary-pool controls with counts1/2/3/16, shuffled/repeated ordinals, variable symbol lineages, random reserved/inactive bytes, signed32 extrema and wrapping arithmetic, unavailable-parent tails, and all rejection predicate combinations. Managed and Windows NativeAOT runs verify 191 required prepasses, 101 prepass failures, and 908 exact parent lookups.

Four additional cancellation controls cover a callback that cancels the token
and returns either true or false, with secondary count1 or count2. All four
throw before early return, parent lookup, or composition.

An additional 65537 physical assignments over 17 heaps prove the ushort wrap at 65536, while preserving adjacent5c fields. Focused controls reject a missing required prepass, reject an anchored slot before invoking external callbacks, stop pre-cancelled work, and prove that an owning prepass invalidating a parent is observed by the later lookup. Unavailable output tails are checked byte-for-byte, including stale increments and identities that must remain untouched.

The actual RU fixture uses the existing regular prepared classifier, final cache, category mapper, and scorer. It routes accepted NN/cache costs into native heaps and assigns initial ordinals with the new writer. The root producer creates7 canonical-context rows (one explicitly diagnostic duplicate); per-heap native initial merging/materialization produces6 real stored parents. A second actual classification supplies10 acoustic increments. The explicit diagnostic parent/increment enumeration generates8 retained continuation records, with `3e=FFFF` supplied to select the proven unanchored scope. Native continuation ordinals, live parent lookup, the new `211eb0` producer, corrected continuation ordering, and materialization produce8 stored secondary-only rows. All96 parent/history lookups address those actual stored rows, and the stored alternative IDs retain their actual parent's symbol lineage plus the supplied current suffix. Cost deltas reproduce the real acoustic path cost, and low-dword ordinal writes preserve auxiliary5c.

The fixture explicitly chooses the proven identity `1f8300` callback. Native callback selection, raw/frame/mask selection, unanchored flag production, dictionary state/cost generation, and scheduling remain unresolved or separately supplied. This is a substantive producer/consumer bridge with real neural costs, not evidence that Russian cursive AI-pero or the entire native decoder is complete.

Commands from the repository root:

```powershell
python tools/LegacyWordContinuationAlternatives.Probe/reference_controls.py .artifacts/legacy-word-continuation-alternatives-controls.json
dotnet run --project tools/LegacyWordContinuationAlternatives.Probe -c Release -- .artifacts/legacy-word-continuation-alternatives-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
dotnet publish tools/LegacyWordContinuationAlternatives.Probe/LegacyWordContinuationAlternatives.Probe.csproj -c Release -r win-x64 -p:PublishAot=true --artifacts-path D:\BlurMask-build-artifacts-20261011\word-continuation-alternatives-artifacts -o D:\BlurMask-build-artifacts-20261011\word-continuation-alternatives-native-bin
```

The NativeAOT executable uses the same five data arguments. No vendor DLL is executed and no GUI, installer, or Explorer is launched.
