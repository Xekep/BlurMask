# Logical visit census and proof limits

These are bounded logical data-read/control envelopes for the actual C# source,
not machine instruction counts, wall-clock predictions or CLR/RSS certification.
The existing ledger constants, allocation/copy precharges, model and input are
unchanged. Every new debit precedes its corresponding read/call/mutation.

## Selection and emission

`Selection(n)=512+192n+96*n(n-1)/2`. Two insertion/dedup loops each have at most
n(n-1)/2 comparisons. Descriptor/ordinal/score reads plus controls fit96 per
combined pair.192n covers owner snapshots/clones and retained selection copies;
array movement traffic remains covered by the unchanged Copy allowance.
The physical owner row count bounds the packed native selection count. Bounds
can exceed the unchanged cap and then refuse BEFORE the selection operation.

`Emission(B)=512+64B`,0<=B<=64, covers the already-required ordinary one-token
branch: token/map/list state, stored label, UTF8 count/encode/copies and scalar
validation. A preceding512 debit covers40-byte Resolve snapshot, descriptor,
lookup and at most64 UTF16 characters. Valid UTF8<=64 implies UTF16 length<=64.
No macro/control/circled-start branch is admitted by this bound.

`Intern(B,C)=256+8(C+1)(2B+16)`. C is actual pool count. It covers hex-key/hash
work, worst dictionary collision-chain byte comparisons and resize/relink;
there is no average hash-distribution premise. Each original-row identity probe
is separately charged32 BEFORE the pointer read.

## Fresh versus reused publication

`Publication(B,C)=512+2048B+24*32+4B+10*32+2*Intern(B,C+1)`.
Per scalar/byte2048 decomposes into32 decode;17*32 grapheme binary steps
(19controls=>5,225extends=>8,11prepends=>4);7*32 joiner steps(77ranges);
10*32 CCC steps(517ranges);20*32 two script lookups(716ranges);288 branch/
guard/control slack.10*32 separately covers the final first-script lookup.
Literal lookup has at most3 depth phases*8 binary steps;4B covers NUL/suffix
reads. Two Intern bounds cover appended identity and021000 publication.
Fresh213A0's2639/263A replacement changes a3-byte smiley into4bytes: the bound
explicitly uses4 when source length is3. Reuse receives its actual interval.

Warm readiness never skips selection. Reused021760 performs its first Intern,
then scans ONLY original rows. A matching identity performs no new publication,
so there is no021000 charge on that branch. A new branch is debited before its
zero/append stores and both subsequent intern/publication calls. Cancellation
and failed-prefix poisoning remain in the existing Process catch paths.

## Source range processing

When no later eligible source key can be probed, Process cannot call Union,
Intersect or Subtract. Bound:

`4096+96M+32R+512K+256(T+1)(K+1)`.

M=actual original ranges (both geometry constructors issue1);96M includes two
16-byte source clones and <=M binary-search probes at64 each.32R covers every
later skipped/negative check.512K retains32 full16-byte key-copy read passes.
4096 covers32 buffer headers*64, guards768, Process/Contains/joiner controls256,
array/list headers256 and scalar setup768. T is actual retained interner Count,
not merely this choice's reference count. Search/insert/rebalancing need at most
4(T+1) comparisons plus bounded node updates; coefficient256 also covers full
key endpoint/control reads. Positive-probe paths retain the old reservation.
No normalization/unique-endpoint assumption is added.

Post-TEXT range call, with n=rows+cuts, retains the original rows+cuts<=65536
admission and all Allocate/Copy charges. Visit bound:

`512+128n+512n*cuts+128cuts`.

128n covers source/list/key/final copies and exported pair reads. Per cut,
512n covers up to three searches, insertion/removal shifts and loop/endpoint
control.128cuts covers original-object count/previous-next endpoint metadata.
512 covers vector/header/guard setup. Equality-only and clone-only call sites
are conservatively charged this same larger bound. Native01C740 still visits
choices in reverse and CHAR links forward; no call or cancellation is skipped.

## Scalar flags and actual prefix exports

The scalar marker read now reserves `1024+32P`, after64 admits reading actual
publisher history count P. Existing same-context/marker/failed-prefix checks,
lock, flags read and fold remain.32P covers the readiness validator's history
walk; fixed1024 covers its source/phase/lock/control fields. The previous4096
was a fixed envelope retained after an earlier full snapshot had been removed.

Only the authentic periodic/original-selected/pending-TEXT path uses the new
prefix snapshot meter; other routes retain the old prefix reservation.
A64 metadata debit precedes publisher count, then256+64P covers the source
validation/count read.64R is paid before the new authenticated row byte-count
walk. `Snapshot=4096+64P+512R+(canonical?2B:B)+(canonical?Source26Bytes:0)`
is debited before cloning. R and B are actual retained row count and total UTF8
bytes, not physical glyph-owner capacity.512R covers full40-byte snapshot
records, known masks, flags/array-reference passes and loop controls; B covers
actual UTF8 clones, doubled only for canonical diagnostics.4096+64P covers
fixed holders, live guards and publisher-history checks. Original allocation/
copy envelopes remain. Final canonical node/tree/command exports keep their
separate existing reservation. Intermediate unconsumed glyph-choice and UTF8
arrays are not created; consumed historical CHAR prefixes remain detached.

## Limits

Numeric formula checks do not by themselves prove these source-count bounds;
the decomposition above plus pinned source/native call ordering is the argument.
The real1363/46/41 controls test arithmetic and actual Process event/prefix
placement. Existing unrelated ledger sites and allocation/copy policies are not
certified globally by this work. Only original index4 has a newly completed full
owning capture; index5 still refuses earlier, and60/64 remain outside node1.
Public positioning, document apply, quality and default-route admission remain
unissued. All inherited935-to950 score/routing changes need separate review.
