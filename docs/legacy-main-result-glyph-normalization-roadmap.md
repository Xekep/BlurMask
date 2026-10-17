# Main21F320 result glyph normalization

This is a read-only inventory of pinned MyScriptText PE
SHA256 `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`,
plus a bounded typed implementation of the current fresh entry's zero-write
return. The enabled contextual glyph-rewrite body is not implemented.

## Current first-result call and actual configuration owner

Successful221840 calls21FBF0 at22198E. It next reads configuration99 at221993..99F;
when99 is zero,2219AD calls21F320. The first guard at21F337..34B loads the
controller's configuration pointer18 and returns when its **byte F4** is zero.
This happens before reading the alternative's WORD head at21F393 or its pool.
Skipping the body is not the same as skipping the preceding quote operation.

F4 is a live2B3 property, not an acoustic-resource scalar or a source-coordinate
field that happens to have the same offset:

| Producer or bridge | Native evidence | Consequence |
| --- | --- | --- |
| Fresh2B3 constructor00C860 | zeroAX;00C923 storesWORD atF4 | FreshF4 andF5 are zero. |
| Property read00EAF0/write00EB00 | read/write byte containerF4 | A nonfresh caller can change it; it is not a permanent constant. |
| Property table2751E0/1E8 | ID0x012D0017; getter/setter pair00EAF0/00EB00 | The table exposes the same live field. |
| Generic property copy00EBA0 |2B3.V70 iterates getter/setter table | Copied configurations may inherit a changedF4. Those lifecycles are excluded. |
| Word-controller binding1EB464..475 | Calls the incoming2B3.V18 | This is00CAB0. |
| Attachment00CAB0 | container60=controller; controller18=the same container |21F320 reads the actual fresh container, not a copied model profile. |
| Preparation00ECA0 | Optional9B/F1 property forwarding; frontend50/60 then controller58 refresh | It does not replace the container or writeF4. |
| Scalar refresh20C8C0 | Reads that container and updates68/A0 as appropriate | Actual selected247 preparation does not turnF4 on. |

The current `LegacyFreshMainSingleLinePlainRecognition` accepts raw ink,
resolution and explicit pinned resources/scratch. It exposes no property packet,
external native container, copied property source or F4-setting input. Its
postprocessing stages transform source218 descriptors, guides and physical
points through24B/24D/29B/265; these are separate owners from the configuration
bound to240. `LegacyFreshCaseMainSourcePolicy` has private construction and
checks the fresh NEW20E, one selected main0 Russian source, default case90,
C4false, tracefalse and no associated/external source. The controller policy
likewise models the checked new, unmodified2B3 branch. Consequently its F4=0
statement is restricted to this typed fresh path. It must not be inferred for
arbitrary native callers, reused or copied settings, other profiles or app
recognizers merely because they use the same acoustic resource.

## Second guard and enabled body prerequisites

IfF4 is nonzero,21F35F..37B returns when replacement2516,2514 and2512 are allFFFF.
It does **not** include2510 in this three-field guard.20C1C0 initializes these
through actual selected-source AL/018F70 lookups:

| Field | Literal | Role in enabled body |
| --- | --- | --- |
|2510|U+201C|Opening double quotation mark|
|2512|U+201D|Closing double quotation mark|
|2514|U+2018|Opening single quotation mark|
|2516|U+2019|Closing single quotation mark|
|24BE|U+201E|Low double quotation mark; retained context/carry|

The pinned Russian192 AL contains none of those five literal strings. That is
independent resource evidence, not a substitute for porting the native lookup
service or a reason to silently enable a general normalizer. The current typed
no-op uses the earlier F4 guard and performs no replacement-label lookup.

The enabled body requires all of the following, beyond the existing simple
synthetic-quote cleanup:

- The actual published279 linked WORD list, its275 records of stride120 and
  each reading-order reference vector20/count28. It uses a three-symbol window
  **across WORD boundaries**, with21F210 recursively advancing over exhausted
  records. Linked glyph head order alone is not this iterator.
- Actual274 scalar reads through139C90, retaining caller scratch where that
  helper leaves bytes untouched.21F320 uses the returned labels without21FBF0's
  alternate-alphabet-byte guard; the two algorithms cannot be substituted.
- Actual signed24 node/signed16 arc decoding of each source reference. Negative
  arcs take distinct paths. Nonnegative arcs ask frontend222.VB8=152040:
  graph68 resolves group ID, then graph88 returns its native **kind**. A zero
  kind, not an invented generic source-eligibility Boolean, permits rewriting.
- Actual frontend48-derived category data. The interior rule reads the previous
  label through data50 and data70 and compares its first category to1. The
  existing sampler category mapper restores those tables, but its source/frame
  owner must be the actual selected graph rather than an unrelated profile.
- Exact ushort wrap tests on neighboring labels, notably the four-value
  FFF9..FFFC interval; distinct leading, trailing and interior rules; actual
  ordinary apostrophe/double-quote24BA/24BC; and retained low-double-quote carry.
  Replacements write only the owning glyph ushort08.

The existing published quote bridge restores21FBF0's leading synthetic193/194
and trailing apostrophe conversion. It does not restore these contextual curly
quotation rules. The generic139C90 scalar reader, owned WORD/pool publication,
source geometry/group-kind helpers and category mapper are useful primitives;
the21F210 cross-word iterator, exact body orchestration, owner bindings and its
independent enabled-branch tests remain separate work.

## Bounded fresh no-op API and tests

`LegacyFreshMainResultGlyphNormalization.Prepare` requires the existing exact
fresh CASE source policy. `Apply` accepts an actual completed first-alternative
snapshot from the same policy, checks terminal/materialization/graph/list
identity and returns a typed no-op proof. There is no caller-suppliedF4 Boolean,
no fabricated WORD, scratch, category or missing control label. Foreign or
changed publisher state and cancellation are refused. Numeric storage and
glyph records are unchanged. This stage does not claim that21FBF0 ran: the
owning compositor must preserve that earlier stage's actual order separately.

The probe executes five original raw-coordinate inputs through actual first
TEXT, source720, main CASE decoding, terminal, compact materialization,
publication and the published quote bridge before this fresh no-op. It checks
the complete owned pool and score bytes before/after and41 null/cancel/foreign
source/changed-head guards. These five geometric inputs are not a handwriting
accuracy corpus or positive evidence for the unimplemented enabled body.
Managed and Windows Native AOT captures are byte-identical. The independent
Python audit compares all2960 captured pool/score bytes and their identities.

The static audit reads the complete21F320 and21F210 ranges, fresh F4 initialization,
property getter/setter/table, same-container attachment, caller guards,
replacement writes and exact UTF8 constants. It never loads a vendor DLL.

```
LegacyMainGlyphNormalizationNoOp.Probe AK LK engine separation case publication line30 line50 capture.json
python tools/LegacyMainGlyphNormalizationNoOp.Probe/audit_output.py capture.json
python tools/handwriting/audit_main_result_glyph_normalization.py static-report.json
```

A future bounded first-candidate completion may compose the owning snapshot,
actual quote stage, this zero-write proof and final numerical channels. It must
remain explicit about post-callback failure order, duplicates/exclusions and
multi-alternative gaps. Restoring enabled21F320 additionally needs an actual
nonzero-property/source-AL owner, observed applicable labels and independent
cross-word/source-kind/context controls; settingF4true or injecting an expected
transcription into the current fresh factory is not that ownership bridge.
