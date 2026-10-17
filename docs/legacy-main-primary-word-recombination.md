# Main primary heap recombination dispatcher

`LegacyNativePrimaryWordRecombination.Apply` joins the already transferred
`20E6A0` root, ordinary stored-parent, higher-parent reset and full-packet higher
geometry kernels. It consumes one complete heap bucket and the actual configured
alternative pools. It does not change the shared `2144F0` scheduler or manufacture
secondary predecessors for the current fresh no-associated CASE model.

## Ownership and native dispatch

The input retains every 128-byte heap slot, including inactive capacity, and every
40-byte alternative record. Physical heap index and the signed pool ordinal at
heap `58` remain separate. Output buffers are detached copies; unrelated and
inactive storage remains byte-identical. The portable shape bounds are 1–16
configured primary/secondary alternatives and at most 4096 heap slots.

After cancellation and portable byte-shape validation, both counts equal to one
or an empty active bucket return immediately. These native no-op routes do not
validate unused source/context callbacks, row ordinals, or the tracing flag. The
active portable implementation requires nontracing mode.

For each active heap slot in physical order:

1. Compute `previous = currentNode - signedByte(slot20) - 1` with native wrap.
   `FFFFFFFF` takes the existing root producer.
2. A nonnegative predecessor with `slot1E < controller230A` takes the ordinary
   parent producer. Source rows are looked up by that computed node and original
   ordinal, rather than by the current D0 span.
3. A higher predecessor takes the reset producer exactly when mode `22F6` is zero,
   `(sourceHeader & 1C) >= 8`, and label `24` differs from controller symbol `24EA`.
4. The remaining higher predecessor takes the full 80-byte geometry controller.
   It receives the complete previous node's `23F0` source rows, the same owning
   row accessor, source headers, context factory, category profile, runtime
   policy and required `118/140/F0-F8` callbacks. This route currently requires
   at least two configured secondary alternatives.

The row accessor is an explicit prepared source, not a guessed array of retained
rows. Each requested node is snapshotted once, including stored88 and both owned
alternative blocks. `23F0` source extent can differ from `23F8` retained count.
Source-header providers represent stable prepared descriptor values during this
operation; repeated header reads in the component kernels are not a new native
external service. No source IDs, context identities, winner labels or priors are
created to fill missing inputs.

The general route receives only the blocks addressed by the actual pool ordinal,
then writes the complete returned128 slot and the matching blocks back into their
original positions. Its last request scratch is carried into the next general
row, including callback-modified padding. Interleaved root, ordinary-parent and
reset rows do not clear this same native local80 storage. All six provider/callback
boundaries have cancellation controls. Context creation can retain its successful
prefix if a later callback fails; this copy-based API does not claim native
allocator or partial-failure order.

## Static evidence

The audit reads the pinned Text PE without loading it. The complete function
`18020E6A0..180210533` has SHA-256
`03f550eeff2a823efc7fd4ce029595d3d50b079df13817ecabe54013776fcd79`.

Relevant instructions are:

| Native address | Contract |
| --- | --- |
| `20E6CD..20E707` | Count1/1 returns before tracing/source reads. |
| `20E72F..20E747` | Empty bucket guard, then physical index ×128. |
| `20E74B..20E76B` | Signed span, original previous ordinal and actual pool ordinal58. |
| `20E8AB`, `20EB1B` | Root `-1`, then unsigned original ordinal versus230A. |
| `20F3EB..20F422` | Exact mode/header/24EA reset condition. |
| `20FC48`, `20FC68` | Full-packet initial118 then140. |
| `20FEFC..20FF0D` | Higher candidate scan starts230A and ends23F0. |
| `20FFC2`, `210100`, `210120` | Successors and per-candidate118/140. |
| `2102EB`, `210309` | Existing workspace merge and real predecessor writeback. |

The audit also pins `20C347/20C37B/20C380`: controller24EA is the result of
looking up U+0003 at `2ECB48`, rather than an assumed acoustic alphabet header.
The dispatcher still receives this prepared field explicitly; these pins are
not a new full controller-construction API.

## Independent controls and actual resource consumer

The original Python oracle generates 256 mixed five-slot streams in the order
root → general geometry → ordinary parent → reset → general geometry. They use
noncontiguous pool ordinals `[3,1,0,4,2]`, a sixth inactive heap slot, an inactive
pool block, actual Russian category profiles, signed/wrapped costs, and scripted
full-packet callbacks. The second general row receives the first row's request
residue. Checks cover 1280 exact routes, 2598 whole80 callback snapshots, 298
general merges and 12 shape/no-op/cancellation guards. The higher rows and their
geometry responses are explicitly prepared controls, not observed inference.

The additional resource consumer uses the existing actual first→main CASE
fixture and its real D0, four-family main neural scores,195-entry cache, geometry
and same source/session. It produces 36 root queries→25 root slots and 130
continuation queries→125 ordinary-parent slots. The owning stored source has125
rows, including its real unused source extent. No expected text is read. This is
an acoustic root/ordinary-parent prefix: it deliberately does not schedule the
intervening dictionary language/history phase and therefore is not a complete
`2144F0` pass.

The independent actual-output audit checks all 150 slots and 52,000 bytes of full
heap/pool storage, context paths and125 owning source rows. Captured neural/cache
inputs are trusted stage inputs here; neural inference is covered by its existing
separate independent audits. Managed and Windows NativeAOT captures agree at
SHA-256 `8eedd75ad6714e5f32a5c8e291e64bedfa4f7c0680d3a722837cd1608aa5be5e`.

Run the generator and full managed probe from the repository root (all output
paths below are examples outside the source tree):

```powershell
$env:TEMP = 'E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP = $env:TEMP
python tools/LegacyMainPrimaryRecombination.Probe/reference_controls.py E:/BlurMask-build-artifacts-20261014/primary-recombination-controls.json
dotnet run --project tools/LegacyMainPrimaryRecombination.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/primary-recombination-managed -- E:/BlurMask-build-artifacts-20261014/primary-recombination-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/text-unicode/legacy-case-data.json models/text-unicode/legacy-text-publication-data.json E:/BlurMask-build-artifacts-20261014/primary-recombination-actual-managed.json
python tools/LegacyMainPrimaryRecombination.Probe/audit_output.py E:/BlurMask-build-artifacts-20261014/primary-recombination-actual-managed.json
python tools/handwriting/audit_main_primary_word_recombination.py E:/BlurMask-build-artifacts-20261014/primary-recombination-static.json
```

The probe also supports the two-argument `controls AK` mode. The full eight
arguments are self-contained repository resources, with no private corpus.
Native publishing uses `-r win-x64 -p:PublishAot=true`; execute the resulting probe
with the same eight arguments and a separate capture path.

The next missing join is an actual scheduler-produced secondary-parent anchor
with its complete owning rows and native callbacks. Existing fresh one-source
CASE/global fixtures prove no-associated and V60=false and exercise root language
anchors. This dispatcher does not loosen those producer constraints, integrate
later-history scheduling, select a result, calibrate mass as confidence, or enable
Russian replacement in the application.
