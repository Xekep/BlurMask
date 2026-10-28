# Detached textResult from actual sources

`LegacyFreshMainDetachedTextDom.Prepare(actualRangeFormatting).Build()`
constructs the supported fresh031880 subtree from the exact retained
counter/seed/document and actual CHAR/WORD/TEXT sources. The result retains
its new library-owned parameterized BCC no-history arena and name pool.
This explicit portability scope creates no textField parent, SDK page,
BE2 batch or accepted C20 document. The earlier
[static contract](legacy-main-document-dom-roadmap.md) remains separate.

The immutable root has native element names, ordered attributes and ordered
children. CHAR insertion order is preserved, WORD48 choices retain actual
CHAR-ref24 links and selected markers, and TEXT32 alternatives retain their
WORD-ref24 links. No expected labels, spaces or string matching enter the
construction. Literal UTF8 is copied only up to the first NUL, without XML
parse, escaping, replacement decoding or re-encoding.

Values retain native assigned lanes: string storage tags3(empty),2(1..15
bytes),1(16+ out-of-line bytes), tag7/I32,8/U32,4/I64,5/F64 and6/byte. The
16..500 pooled versus501+ allocation branch is proven but represented by
managed owned storage; native page addresses/padding are not fabricated.
Numeric attribute values preserve raw signed bits, NaNs and doubles rather
than formatting them as culture-dependent strings. Attribute/name identities
are local to the retained library arena, not serialized native pointers.
Element-name interning precedes node allocation:0ED3C4 resolves the name,
0ED3D7 retains its handle and0ED3ED enters0EA360(kind3); only0EA3A2 calls
the node allocator. A failed element allocation therefore retains the
already interned name. Managed name storage represents that lifetime;
prepared failure points do not emulate native name-pool page allocation.

Fresh0287B7 clears qwordB8, producing TEXTfreezeB8=0 and selectedBC=0. This
producer is distinct from source coverage's selected alternative, which is
only checked as a supported-scope gate. TEXTtagB0 is the actual pending
TEXT selection ID, not the seed's INPUT tag30. CHAR/WORD tags, source keys,
line metadata and choice indices come from their actual typed owners.
Freeze1C is separate from flags18; forceSelected40 remains minusone while
selected44 can change. The native fresh CHAR/WORD writers establish those
values before the supported source selection stages.

The scoped source seed is fresh with no existing DOM text/prior owner or
caller-supplied UTF8. The separate02A3D0 caller-text writer and02D8C0 existing
DOM loader are not used; ordinary0283B0 does not write E8/F0. Thus no text
override is invented. Existing nonzero prior ranges, external400/math800
CHAR records and enabled transform branches are explicitly unsupported.
Current trimmed line56 rows emit literal x/y/a/h attributes with the native
x threshold and optional signed tagId. UUIDgen uses the retained
`Seed.LibraryIdentifier38`, including its documented cryptographic seed
adaptation.

One range scratch belongs to the whole build. CHAR keys, WORD keys, every
CHAR/WORD reference and TEXT use that same capacity and literal normalization
core. Normalization-copy allocation is separate from scratch-byte capacity:
copying a noncanonical key does not reset a retained string buffer. Existing
fresh-capacity formatting API/captures remain unchanged.

Native-service false follows the actual helper cleanup rules and the outer
031880 child clear/root release. Native-unattached failed nodes can remain
in the arena:030CC0 does not explicitly free an unlinked failed CHAR-ref,
and some parent lists are unattached until success. The managed arena owns
all those nodes until its lifetime ends; diagnostic released/live sidecars
are not claimed as native freed-memory bytes. The prepared031360
textCandidate-allocation-null boundary explicitly refuses because native
then calls V90(null); it is not turned into a fabricated successful cleanup.

Cancellation and managed exceptions have no native cancellation callback
equivalent. They poison the single-use builder and retain the diagnostic
prefix without inventing native normal-return cleanup. Prepared service
faults are test outcomes, not claims about OS/native page allocation timing.
Foreign arena nodes and foreign WORD/CHAR source links are refused; output
trees and values are deep immutable snapshots. Source registry bytes,
pending commands and earlier source snapshots remain unchanged.

`tools/LegacyMainDetachedTextDom.Probe` captures actual raw40/48/ref24/TEXT32/
line56 metadata after inference. Its independent Python oracle reconstructs
the full tree from those rows and verifies arena/link/failure prefixes,
literal NUL/15/16/17/500/501 boundaries, numeric lanes, cancellation and
shared scratch. Failure cleanup is derived separately from the verified
logical tree, the pre-failure attachments and each native helper's release
rules; the oracle checks every surviving orphan and release sequence.
Managed/Native structural parity normalizes only the
environment-dependent retained UUIDgen bytes; the complete native/managed
trees each independently match their own source UUID. No DOM application
or final document acceptance is claimed.
