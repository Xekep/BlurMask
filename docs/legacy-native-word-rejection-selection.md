# Prepared rejection candidates and live primary path repair

`LegacyNativePreparedRejectionProducer`, `LegacyNativeRejectionFallbackProducer`,
`LegacyNativeRejectionCandidateSelection`, `LegacyNativePrimaryWordRowOwner` and
`LegacyNativePrimaryRejectionPathStitcher` connect the ordinary 36-byte candidate
append layout to sorting, selection and actual mutable primary alternative rows.
The scalar fields entering the prepared append are explicit. This is **not the
complete `210870` ancestry/cost collector or a final word winner**.

## Native evidence

All addresses refer to the analyzed x64 `MyScriptText.dll` with image base
`180000000`. Private analysis exports remain outside the public source tree.
The implementation does not execute or load that DLL.

* `180210870` writes cost/correction at `00/04`, signed node fields at `08/0c/10`,
  five 16-bit fields at `14/16/18/1a/1c`, alternative bytes at `1e/1f`, and a zero
  selection byte at `20`. Its append copies the entire `24`-byte record, including
  the three remaining stack-derived bytes. The prepared writer requires this
  residue rather than fabricating zero-filled native stack storage.
* `180210720` is the fallback writer: both costs become `04000000`, fields `18`
  and `1c` receive controller `230a`, `1a` becomes `ffff`, and `1f/20` become zero.
  The caller supplies the three nodes, `14/16`, `1e`, and complete stack residue.
* `180210540` compares signed node `08`, then unsigned short `14`, then literal
  signed 32-bit subtraction of cost `00`. The last operation wraps; it is not
  `CompareTo` or a strict total order. `180210570` copies the midpoint pivot and
  swaps every one of the 36 bytes, recursing into the lower partition first.
* `180211210` groups sorted records by node `08` and short `14`. It retains the
  first `2402` distinct short keys `18` in its local 20-short array. Marking stops
  at the first `ffff` sentinel, even if later retained keys exist. A matching
  record with signed cost below `04000000` gets byte `20 = 1`. Existing selected
  bytes are never cleared, including flags on unqualified candidates.
* `1802113f0` first traces **every** selected path and invalidates alternative
  cost `00` unless state `1f` already has bit `40`. Only after this global pass
  does it repeat each path and rebuild links. Starting node/arc/alternative are
  candidate `0c/16/1e`; the first replacement parent is `10/1a/1f`. Primary
  metadata `18/1a/1b` drives reverse tracing, including signed byte spans.
* An available, already matching link is protected with bit `40` and skipped.
  A conflicting available link searches only later alternatives for the first
  unavailable cost. If no gap exists, native abandons the rest of that path.
  Increment repair uses **parent alternative zero**, parent record `14`, current
  record `14`, and current record count `26`; it does not substitute the chosen
  parent alternative's increment. First-path cost is candidate `04`. Later cost
  uses the repaired previous alternative and the physical record predecessor
  from current record `20/1e`. All arithmetic retains signed 32-bit wrap.
* Rebuilt state keeps high bits, replaces low five bits from current record `21`,
  and protects bit `40`. At the first point it also protects the replacement
  parent, clears bit `20`, and sets bit `80`. Alternative index bit `80` at `1b`
  is preserved. Context append/copy uses record `18` versus the actual node and
  signed record `05`; append delegates to the separately verified `1f7e30` /
  `1f7ff0` routines and the explicit controller `2360` symbol mapper.

## Ownership and limits

The row owner clones complete supplied 88-byte stored records and configured
primary/secondary blocks, indexes them by actual `(node, arc)`, and returns owned
copies. The stitcher mutates **live primary storage** only. It preserves record
bytes, secondary blocks, candidate storage, and unrelated alternative residue.
Cancellation after writes does not roll back those writes. Missing rows, invalid
indices, cycles, paths beyond 4096 points, more than 65536 records, and selection
counts over the native 20-slot local array are refused.

An optional owning `higherArcRowLookup` supplies record-only snapshots when the
physical predecessor arc is at or above `230a`. Such a row can have no primary
block, as the real `213410` materializer produces. The callback is not used to
fabricate alternative storage; its cancellation is checked immediately before
reading returned bytes. A focused two-point path verifies the later-point cost
against that higher record and rejects cancellation inside the owning callback.

This checked managed subset assumes successful temporary path-vector allocation.
It does not model `2113f0` allocator failure, the dynamic `210720` vector growth,
or the tracing `2368` branch and its `280`-byte side tables. Tracing is explicitly
refused. Native wrapped comparisons that would scan outside a supplied partition
are refused rather than reading unrelated memory. Prepared scalar values do not
replace source ancestry, rejection policy, marker production, or `210870` heap
candidate collection. Those producer branches still need their own port.

## Independent verification

`tools/LegacyWordRejection.Probe/reference_controls.py` implements independent
recursive 36-byte quicksort, distinct-key marking, complete mutable row storage,
and functional symbol paths. Controls cover 1024 comparisons (including
non-antisymmetric signed overflow), 512 full-capacity selections, 256 prepared
and fallback writes, and 384 live mutations comparing 3456 complete rows. The
generated mutations exercise 702 invalidations, 68 matching links, 58 later-gap
choices, 68 path abandonments, 635 increment repairs, 741 append operations, and
222 context copies. Exact mapper call order is checked separately.

The real-resource control loads the actual Russian acoustic/lexical bundle,
builds NN/final-cache scored roots and children using the existing native record,
alternative ordering and materialization components, supplies a prepared
rejection, selects it, and repairs a child's primary parent/cost/context. Another
native parent-alternative producer then consumes that **owned repaired row** and
materializes a descendant, verifying its inherited repaired cost and symbol
lineage. No expected word, final winner, or acceptance confidence is supplied.
Frames, mapping selection, scheduling and the prepared correction remain explicit
diagnostic inputs; this does not enable production Russian AI-pen recognition.

Run the probe with five arguments: generated controls JSON, acoustic `.res`,
lexical `.res`, engine profile JSON, and separation profile JSON. It supports
managed execution and Windows Native AOT without GUI or vendor runtime loading.
