# Math2 completed records to external output: static boundary

This is a read-only audit after the owning raw-only EF4 and successful final-order
stages. It adds no runtime formatter, accepted formula, layout engine or app path.
`audit_math2_output_route.py` hashes 15 complete Document regions, checks 83
instructions and ten concrete table bindings, reads the real 299-label alphabet
and native 14-rule table, and checks eight prepared decimal-normalization controls.
It executes no vendor DLL or inference.

## Actual ownership route

`038610` returns a vector of complete 168-byte records, not text. The current
completed records carry score, old primary/secondary, post-EF4 units at +50/count
+58, runs +68/count +70, per-unit alternatives +78/count +80, Library3E +88, and
auxiliary +98/count +A0. These planes have different consumers.

`EEE.03A260` can run separate single-/multi-primary passes when its constraint
policy is active. It moves complete records with 168-byte swaps, preserving
ownership, then sorts/truncates the merged vector. In the scoped no-constraint
route, `03AD5A..03AD69` swaps the completed vector into its output argument.
That argument is still the local `047C50` vector, not the final caller result.
This bounded route does not require reconstructing detached records or running
another NN pass.

`EED.047C50` invokes `EEE.V28=03A260`, then, only on success:

1. Calls `046AE0` on the actual completed vector.
2. `046AE0` runs the EE4 formatter on units +50, remaps each nonnegative run's
   unit index through its returned U16 old-to-new map, applies the decimal rule
   below, and swaps the formatted unit vector into the record.
3. `045C60` builds the record's per-unit +78 array through `043650`, using the
   same record Library3E and actual source alphabet. Unit rows are 40 bytes;
   each text-alternative row is 16 bytes, with interned string identity, byte
   length and a separate scalar score lane.
4. If the completed vector is nonempty, optional `043AC0` copies the first record
   into caller storage, including fresh Library ownership and deep payload copies.
   A separate complete-vector swap transfers all alternatives if requested.
5. `043E00` destroys the remaining local vector; after a successful swap it owns
   the caller's former vector, not the newly returned records.

The result is still an owned record/alternative vector. SDK-level source/context,
external `mathResult` tree, symbol ranges, formatting and layout transport are
broader callers. For example, `027E20` builds source-bearing `symbol` nodes and
`028070` manipulates a `mathResult` subtree; their full dispatch/source ownership
is not closed by this audit. No U16 concatenation can replace this route.

## EE4 is present in the actual cold request

Fresh EEB constructor `027830` writes word `0101` at +3C. The request begins at
EEB+38, so both request bytes four and five are one. Byte five causes `04517E`
to take the EE4 construction branch. `045184` supplies **the same EED.1D8 source
alphabet**, and `0451C7` stores the constructed formatter at EED+228.

Concrete bindings are:

| Owner | Bound target |
|---|---|
| EE4 descriptor | `1B7200` |
| Primary/instance table | `1B7248` / `1B7280` |
| Eight-byte packet constructor | `0236B0` |
| Grammar check V10 | `024A20` |
| Formatter V18 | `025980` |
| Normalization/map helper | `0251E0` |

The constructor stores the source AL at formatter+110 and resolves its actual
tags, rather than using decoder indices as generic Unicode code units. It loads
14 rules from 72-byte entries at `254970`: square root, fraction, slanted fraction,
subscript, superscript, sub/sup, under, over, under/over, pre-sup, table, vertical
operator, fence and control U+2402/U+2403. Every referenced name is present in
the fingerprinted source alphabet. `025980` tracks nested rule state and can
change/remove units; after `0251E0` constructs the resulting vector/map, it calls
`024A20` to check the resulting grammar.

The existing actual width-sixteen capture contains retained `<sub>` and `<fence>`
sequences. These are real structured outputs, so the formatter cannot be declared
an identity operation for all retained records. Width one and the explicitly
prepared width-three fixture retain ordinary no-rule-hit units, but that does
not prove general EE4 support.

## Source AL, decoder count, alternatives and decimal rule

The stored source alphabet has **299** entries, including control labels 297/298.
The serialized decoder-vocabulary field immediately after that alphabet is
**297**. For version >=103, `048795..0487C3` reads this U16 into EED+1E2.
`045C8B`/`045DDE` use that count as the ordinary output-label limit. It is not
`AL.LabelCount`; synthetic replacement 299 is another distinct case.

For an ordinary nonzero unit whose byte +2 is zero and whose label is below that
limit, `045C60` calls source `AL.V30` to retrieve the UTF8 interval, then calls
the **same record Library88.V10**. Its concrete Engine3E table is `23CFC8` and
V10 is `0315E0`; it implements interval interning through `030E60`. The existing
`LegacyNativeUtf8StringPool` already models its byte/identity semantics, but the
Math completed Library owner has not yet been connected to it.

`043650` initializes the 40-byte unit-row semantic fields. `045C60` writes row
flag +20=0 and, when detection is absent, metadata +24=FFFFFFFF. The current cold
optional-count field is zero, so the first ordinary alternative has scalar lane
zero; it is not calibrated confidence. Nonzero unit byte +2 selects a separate
existing-string lane through EED+1F0/+200/+208. Optional auxiliary alternatives
use +98; neither lane may be replaced by decoder-label text.

Even the no-rule-hit formatter domain is not universally unchanged. After EE4,
`046D20..046D7D` examines each interior unit. If all three unit bytes +2 are zero,
the middle label is the actual AL label for U+22C5, and both neighboring labels
are within `AL('0')..AL('0')+9`, `046D6E` replaces the middle **low U16 only** with
the AL label for period. Upper metadata bits remain unchanged. The actual
resource's ten digit labels are contiguous. Eight prepared flag combinations
prove a positive no-rule counterexample and seven non-triggering cases. This is
a scalar/control test, not a manufactured actual NN observation.

## Failure publication and cleanup closure

The important outer guard is after the actual `EEE.V28` call:

- `048170/048172` test its returned AL; the false path writes local success byte
  zero at `048174`.
- `048428/048435` bypass `046AE0`, the first-record copy and the external-vector
  swap on that false byte. Formatter failure also bypasses publication.
- `048498` returns false, then `04849A..0484A2` **always** call `043E00` on the
  local vector.
- `043E00` destroys every counted 168-byte row, including its Library +88 and
  unit-alternative payloads, and frees the vector. It does not publish a partial
  vector. The record-copy target and external-vector pointer are not switched
  on these failure paths.

This does not make the later `043AC0` copy transactional. If that copy itself
fails after resetting/reinitializing caller first-record storage, its target may
retain a partial write prefix even though the complete-vector swap is skipped.
That later transport/copy failure boundary has not been implemented by the
current Math owner and must not be described as rolled back.

Thus the no-constraint EEE partial swap is local transport only. A native failure
may first sort/truncate that local vector, but its contents are then all discarded
by `047C50`. For this bounded public outcome, poisoned state + DisposeAll + no
output is an intentional portable cleanup adaptation. Resetting the poison or
sorting a canceled result solely to emulate unobservable intermediate order is
unnecessary. Native allocator/destructor order, ABI pointers, diagnostics and
intermediate library callbacks are not claimed equivalent.

The existing actual-NN logical fault controls already verify exact-once Library
cleanup: recorder failures, recording-loop failures, EF4 consuming failures, and
the 18 final-order fault/cancel prefixes all remain poisoned and dispose every
original Library once. These are managed failure observations, not executed SDK
fault injections. The success-only final token remains appropriate; failure-tail
sorting is not a functional output blocker within this scoped adaptation.

## Smallest owning output proposal

The next library-only stage should consume the exact successful final-order
token and its same Source/Record/Library/AL lineage. It must preflight **every**
retained record against all bound rule opening/continuation/closing IDs; any hit
refuses the entire vector. It must not drop structured alternatives or prefer an
ordinary candidate. This makes the real width-one fixture a positive test and
the real width-sixteen fixture an explicit whole-vector refusal.

For the supported no-rule-hit domain it can restore the `025980/0251E0` identity
unit/map branch, then the genuine decimal rewrite, exact run-index map, and
ordinary `045C60` per-unit alternatives through the same Library's intern pool.
It returns typed unit rows and separate UTF8 alternative intervals. It invents
neither joining spaces nor a complete formula/acceptance flag.

Proposed ownership before runtime edits:

- New output policy/owner files hold the actual bound grammar IDs, decoder count
  and private same-record output lease; no caller no-grammar boolean or AL array.
- A narrow immutable source-AL getter on the existing EF3 gate retains the exact
  same alphabet object; an owner path uses the recorded Source's existing gate.
- A narrow final-token claim and stale-read guard seal its old view after output
  publication begins. Private EF4 partial methods update the exact associated
  unit/run payloads under the existing Source/loop gates.
- A narrow lazy intern-pool capability belongs to each actual completed Library;
  its existing Release disposes the pool. It cannot accept a caller-created pool.
- New probe, independent byte/map/unit-row oracle, static pins and docs cover the
  actual ordinary positive route, structured whole-vector refusal, decimal
  prepared controls, alias identity, ownership, budget and failure prefixes.

Shape/source validation and a combined history/EF4/final/output payload envelope
precede the one-use claim. Consuming cancellation/fault preserves the logical
phase prefix and poisons the owner; no partially accepted output is exposed.
Source disposal invalidates typed output/string references. The full 14-rule
engine, existing-source units, auxiliary alternatives, first-record deep copy,
SDK tree/layout/source binding and application acceptance remain separate work.

Reproduce the static report from the repository root:

```powershell
python tools/handwriting/audit_math2_output_route.py <report.json>
```

No private artifact path or expected transcription is an input to this auditor.
