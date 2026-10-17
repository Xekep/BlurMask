# Math2 EF3 observable-token classification

The two EF3 service `+0x10` calls in decoder `038610`, at `0392D6` and `039453`, dispatch to native `03EFF0`. This is an alphabet-derived observable-token classification. Its result alone is not full grammar eligibility, nor a restored beam decoder.

## Source and table ownership

In the pinned `MyScriptDocument.dll`, EF3 is a kind-1 service descriptor at `0x1801FCD70`, with service table `0x1801FCD50`. Its four entries are `03EB00` (initialize), `03EB10` (bind alphabet), `03EFF0` (classify one token), and `03F050` (count observable tokens in a sequence). It is not an independently allocated type-2/type-3 object.

The fresh EEE constructor `035920` initializes its `+0x68` classification table to sixteen `0xFF` bytes. Successful cold EED initialization `045460` then calls the actual EED `+0x220` decoder's `V10`, passing the packet at EED `+0x1D8` (`045953..04596B`). EEE's service `V10 = 035D60` retains that packet at EEE `+0x60`, dereferences its first pointer, and calls EF3 `+8` with that actual alphabet and EEE `+0x68` (`035DEC..035E1C`). It separately forwards the resulting table to EEE owner `+0x80`'s `V10` afterward. The concrete owner `+0x80` implementation and its enable/alias map are separate prerequisites; they are not established by EF3's table binding.

`03EB10` first sets all eight unsigned 16-bit slots to `0xFFFF`. Slot 5 is copied from alphabet field `+0x12`. It looks up U+2402 and U+2403 through the actual alphabet, renders each alphabet token through `03E840`, and decodes Unicode scalars. Non-whitespace, single-scalar tokens excluding those two control IDs establish an inclusive first/last interval. Its finite whitespace test is U+0009..U+000D, U+0020, U+00A0, and U+2028..U+2029. Exact UTF-8 matches also populate the special slots below. Token-rendering aliases and Unicode-service behavior outside the fingerprinted actual resource are not independently ported by this audit.

## Actual Math2 resource

The raw `math2-unwrapped.res` SHA-256 is `2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a`. Its stored source alphabet has 299 symbols; the decoder vocabulary has 297. The extra alphabet IDs 297 and 298 are U+2402 and U+2403 and do not become decoder classes.

| Table slot | Meaning established by initializer | Actual value |
| --- | --- | --- |
| 0 | First qualifying single-scalar label | 2 |
| 1 | U+221A square-root label | 190 |
| 2 | Last qualifying single-scalar label | 261 |
| 3 | `<fracbar>` | 268 |
| 4 | `<slantedfracbar>` | 276 |
| 5 | Alphabet field `+0x12`, excluded before other tests | `0xFFFF` |
| 6 | U+22C5 dot label | 242 |
| 7 | `<vopbar>` | 296 |

The full `03EFF0` instruction stream establishes this order:

1. Return false when the supplied token equals slot 5.
2. Return true for the unsigned inclusive interval `[slot 0, slot 2]`.
3. Return true when it equals a non-`0xFFFF` slot 3, 4, 6, or 7.
4. Otherwise return false.

Slot 1 is not an additional alternative in this predicate. The square-root label is already in the interval for this resource; downstream alias handling uses that slot separately. Across all 65,536 input IDs, the actual table classifies exactly 263 as observable: IDs 2 through 261, plus 268, 276, and 296. `[bos]`, `[eos]`, structural tags, and both extra control IDs classify false. This is a data-derived result for the pinned alphabet, not an assertion that every observable class survives later gates.

## Decoder consumers and missing joins

The two `038610` call sites pass tokens from candidate sequences and the same EEE-owned table. A true classification can trigger complete-hypothesis processing through `037430`; candidate sorting, terminal handling, and failure propagation remain part of the surrounding decoder. `0360B0` and `036360` also classify tokens while consulting owner `+0x80`'s `+0xC8` enable map. `036360` includes special-label alias fallback and heap candidate creation. Thus applying EF3 alone to logits cannot reproduce the native selection behavior.

The next useful ownership join is the concrete EEE owner `+0x80` setup, its score callback, and the producer of the `+0xC8` map. Full `2 * beam` candidate retention, native tie/order behavior, score pruning, and complete-hypothesis handling still require the real decoder consumer. No width-1/greedy equivalence or recognition-quality claim follows from this classification audit.

## Reproduction

```powershell
python tools/handwriting/audit_math2_observable_token_gate.py E:/BlurMask-build-artifacts-20261014/joined-math2-observable-gate-static.json
```

The audit checks the PE fingerprint, seven whole function regions, 65 instruction pins (including the complete short predicate), seven service bindings, exact UTF-8 literals, and the complete raw resource fingerprint. It reconstructs all 299 alphabet labels using the existing bounded resource reader, verifies the observed interval and special IDs, and enumerates all unsigned 16-bit predicate inputs. The predicate enumeration is a static mathematical check, not execution of native instructions. No DLL, ONNX Runtime session, GUI, or inference is executed; application runtime files are unchanged.
