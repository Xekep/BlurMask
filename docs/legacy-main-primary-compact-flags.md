# Primary compact word flags

The shared compact publisher previously passed `false` for terminal bit 3 at every frame append. Its primary forced flag also omitted the alternative quota, source header class and previous candidates in the same group. This prevented genuine terminal-tag candidates from reaching the subsequent eligible-word scan.

`LegacyPreparedMainPrimaryCompactFlags` now implements the primary policy from native `207CB0`, including the two separately conditional calls to the same owning `207780` reader. Fallback byte 3 comes from the actual source tag's bit 41. The quota counts distinct selected contexts; context/auxiliary deduplication and the forced flag remain separate. Each typed adapter supplies headers copied from its actual source owner. Secondary and marked-row publication still pass literal false. The primary cost adds the language penalty only when the forced flag is set and the actual source header has a nonzero `0x1c` classification.

Source evidence is the complete original `MyScriptText.dll` regions `207CB0`, `206D30` and `207780`, authenticated against module SHA-256 `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`. Root independently verified all three region images and 41 instruction pins. Vendor code was read as data and was not executed.

The prepared probe covers 5,760 flag decisions, independent probe replies, all 16 header classes, quota exhaustion, previous contexts and auxiliary values. The independent oracle checks complete 40-byte record images and rejects ten corruptions or missing/duplicate cases. Its full-record costs deliberately compose the caller's `100 + 8 * bit4`; these fixtures do not establish the source-header-dependent native cost policy. They run without a model or an actual query source.

The shared compact-materialization Python oracle now composes this same independent flag model. It previously omitted terminal bit 3 and retained the obsolete forced-flag rule, causing CI to disagree with the corrected publisher at compact byte 72. Source headers are reconstructed from the authenticated linguistic resource; the fresh alternative quota is 2, separate from the arc limit. Replay of the preserved actual 2,129-record capture checks every compact and beam/pool byte without another inference. A separately labelled prepared allocator variation checks the canonical A7-residue policy; it is not an actual execution capture. The repair packet is `e812668b794a1868da91e6f77902db5ee6f0a17f05b156d38c7b80f65e31ca19`.

Root additionally ran one unchanged Russian recording through a diagnostic copy of the genuine owning source. It produced 2,129 compact records, including 16 with bit 3 set. An independent reader rebuilt the raw linguistic queries, flag decisions, frame normalization, sorting and complete buffers: 163,840 compact bytes and 189,824 beam/alternative bytes. The trace contains 897 primary decisions, 58 probe answers and 1,794 frames. Cached-score inspection did not cause another acoustic evaluation. This route has actual source header low classification 8, so the separately corrected cost condition leaves its numbers unchanged. The run used two CPUs and a 45-second outer watchdog; the observed process time is not a latency guarantee.

This confirms actual eligibility, not recognition quality. The diagnostic stops before no-candidate completion, word augmentation and final text publication. Old zero-bit captures came from the defective writer and cannot establish that the corrected branch is unreachable. The existing no-candidate completion must continue to refuse positive eligible candidates until the owning augmentation path is connected. No acceptance threshold, default engine or model changed. Full Russian and English handwriting support remains unfinished.

Reproduction of prepared controls:

```sh
python -I -B tools/LegacyMainPrimaryCompactFlags.Probe/generate_controls.py input.json
dotnet run --project tools/LegacyMainPrimaryCompactFlags.Probe -c Release -- input.json output.json
python -I -B tools/LegacyMainPrimaryCompactFlags.Probe/audit_output.py input.json output.json
```

Immutable source packets: flags `1c8e0c54be3c7c097e099adf334859cfd40ec347967039dc94d7fc0968f59295`, cost successor `a36e3a29ed7f47f91b1b19bfcef28418a260189edefcb9b96e0a792673c4b67e`, actual diagnostic source/runtime `43fda1ef3af982b0fb63e8e075441e940efa2553125cb0b60d26324a65808c8b`. Root evidence is retained under `E:/BlurMask-build-artifacts-20261014/root-ru-primary-flags-actual-20261024`.
